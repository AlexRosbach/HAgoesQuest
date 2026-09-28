using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HAgoesQuest
{
    [Serializable] public sealed class BindingRecord { public string entityId; public string anchorUuid; public bool displayOnly; }
    [Serializable] public sealed class BindingFile { public int version = 1; public List<BindingRecord> bindings = new List<BindingRecord>(); }

    public sealed class SpatialBindings : MonoBehaviour
    {
        public readonly Dictionary<string, OVRSpatialAnchor> Loaded = new Dictionary<string, OVRSpatialAnchor>();
        public event Action<string, Transform> Added;
        public event Action<string> Removed;
        public string Status { get; private set; } = "";
        public bool Busy { get; private set; }
        public bool Ready { get; private set; }
        BindingFile data = new BindingFile();
        string path;
        public IEnumerable<string> EntityIds => data.bindings.Select(b => b.entityId).ToArray();
        public bool Contains(string id) => data.bindings.Any(b => b.entityId == id);
        public bool DisplayOnly(string id) => data.bindings.FirstOrDefault(b => b.entityId == id)?.displayOnly ?? false;

        public bool ResetForServer(string server)
        {
            if (Busy) return false;
            foreach (var pair in Loaded)
            {
                Removed?.Invoke(pair.Key);
                if (pair.Value) Destroy(pair.Value.gameObject);
            }
            Loaded.Clear();
            data = new BindingFile();
            Ready = false;
            Initialize(server);
            return Ready;
        }

        public void Initialize(string server)
        {
            if (Ready || string.IsNullOrEmpty(server)) return;
            using (var hash = SHA256.Create())
            {
                string key = BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(server))).Replace("-", "").Substring(0, 24);
                path = Path.Combine(Application.persistentDataPath, "bindings-" + key + ".json");
            }
            try
            {
                if (File.Exists(path))
                {
                    var candidate = JsonUtility.FromJson<BindingFile>(File.ReadAllText(path));
                    if (candidate == null || candidate.version != 1 || candidate.bindings == null || candidate.bindings.Count > 50 ||
                        candidate.bindings.Any(b => b == null || !Core.EntityPolicy.Visible(b.entityId) || !Guid.TryParse(b.anchorUuid, out var _)) ||
                        candidate.bindings.Select(b => b.entityId).Distinct().Count() != candidate.bindings.Count)
                        throw new InvalidDataException();
                    data = candidate;
                }
                Ready = true;
            }
            catch { Status = "Zuordnungsdatei beschädigt. Datei sichern und reparieren; sie wird nicht überschrieben."; }
        }

        void Persist()
        {
            string temp = path + ".tmp";
            File.WriteAllText(temp, JsonUtility.ToJson(data, true));
            if (File.Exists(path)) File.Replace(temp, path, path + ".bak");
            else File.Move(temp, path);
        }

        public async Task Load()
        {
            if (Busy || !Ready) return;
            Busy = true;
            Status = "Gespeicherte Positionen werden gesucht …";
            try
            {
                var pending = data.bindings.Where(b => !Loaded.ContainsKey(b.entityId)).ToArray();
                if (pending.Length > 0)
                {
                    var unbound = new List<OVRSpatialAnchor.UnboundAnchor>();
                    var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(pending.Select(b => Guid.Parse(b.anchorUuid)), unbound);
                    if (!result.Success) throw new InvalidOperationException();
                    foreach (var item in unbound.ToArray())
                    {
                        if (!await item.LocalizeAsync(10)) continue;
                        if (!this) return;
                        var record = pending.FirstOrDefault(b => b.anchorUuid == item.Uuid.ToString());
                        if (record == null || !item.TryGetPose(out var pose)) continue;
                        var go = new GameObject("HA Anchor");
                        go.transform.SetPositionAndRotation(pose.position, pose.rotation);
                        var anchor = go.AddComponent<OVRSpatialAnchor>();
                        item.BindTo(anchor);
                        Loaded[record.entityId] = anchor;
                        Added?.Invoke(record.entityId, anchor.transform);
                    }
                }
                int missing = data.bindings.Count - Loaded.Count;
                Status = missing == 0 ? "Gespeicherte Positionen geladen." : missing + " Position(en) noch nicht gefunden. Im jeweiligen Raum erneut laden.";
            }
            catch { Status = "Positionen konnten nicht geladen werden. Raum/Berechtigung prüfen und erneut laden."; }
            finally { Busy = false; }
        }

        public async Task Place(string entityId, Vector3 position, bool displayOnly = false)
        {
            if (Busy || !Ready || Contains(entityId)) return;
            if (data.bindings.Count >= 50) { Status = "Der Prototyp unterstützt bis zu 50 Zuordnungen."; return; }
            Busy = true;
            Status = "Position wird gespeichert …";
            OVRSpatialAnchor anchor = null;
            BindingRecord added = null;
            bool saved = false;
            try
            {
                var go = new GameObject("HA Anchor");
                go.transform.position = position;
                anchor = go.AddComponent<OVRSpatialAnchor>();
                if (!await anchor.WhenCreatedAsync() || !await anchor.WhenLocalizedAsync()) throw new InvalidOperationException();
                var result = await anchor.SaveAnchorAsync();
                if (!result.Success) throw new IOException();
                saved = true;
                added = new BindingRecord { entityId = entityId, anchorUuid = anchor.Uuid.ToString(), displayOnly = displayOnly };
                data.bindings.Add(added);
                Persist();
                Loaded[entityId] = anchor;
                Added?.Invoke(entityId, anchor.transform);
                Status = "Position gespeichert.";
            }
            catch
            {
                if (added != null) data.bindings.Remove(added);
                if (anchor)
                {
                    if (saved)
                    {
                        try { await anchor.EraseAnchorAsync(); }
                        catch { /* Best-effort rollback; no successful placement is reported. */ }
                    }
                    Destroy(anchor.gameObject);
                }
                Status = "Position konnte nicht gespeichert werden. Berechtigung und Raumtracking prüfen.";
            }
            finally { Busy = false; }
        }

        public async Task Remove(string id)
        {
            if (Busy || !Ready) return;
            var record = data.bindings.FirstOrDefault(b => b.entityId == id);
            if (record == null) return;
            Busy = true;
            try
            {
                data.bindings.Remove(record);
                try { Persist(); }
                catch { data.bindings.Add(record); throw; }
                Loaded.TryGetValue(id, out var anchor);
                Loaded.Remove(id);
                Removed?.Invoke(id);
                if (anchor) Destroy(anchor.gameObject);
                Status = "Zuordnung entfernt.";
                // Removing a placement must not depend on finding its room again.
                // The persisted association is authoritative even if OS cleanup fails.
                if (!Application.isEditor)
                {
                    var cleanup = EraseSavedAnchor(Guid.Parse(record.anchorUuid));
                    if (await Task.WhenAny(cleanup, Task.Delay(5000)) != cleanup || !await cleanup)
                        Status = "Zuordnung entfernt. Raumanker konnte noch nicht bereinigt werden.";
                }
            }
            catch { Status = Contains(id) ? "Löschen fehlgeschlagen. Zuordnungsdatei prüfen." : "Zuordnung entfernt; Raumanker-Bereinigung fehlgeschlagen."; }
            finally { Busy = false; }
        }

        static async Task<bool> EraseSavedAnchor(Guid uuid)
        {
            try { return (await OVRSpatialAnchor.EraseAnchorsAsync(null, new[] { uuid })).Success; }
            catch { return false; }
        }
    }
}
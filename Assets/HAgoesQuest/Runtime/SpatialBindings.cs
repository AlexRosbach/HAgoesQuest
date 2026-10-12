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

    /// <summary>Persists server-scoped entity/anchor links and exposes only stable tracked anchors.</summary>
    /// <remarks>Stored UUIDs are resolved by the SDK; saved world coordinates are not used as a fallback.</remarks>
    [DefaultExecutionOrder(10000)]
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
        readonly Core.TrackingGate headGate = new Core.TrackingGate();
        readonly Dictionary<string, Core.TrackingGate> poseGates = new Dictionary<string, Core.TrackingGate>();
        readonly Dictionary<string, Pose> previousPoses = new Dictionary<string, Pose>();
        readonly HashSet<string> trackedIds = new HashSet<string>();
        public bool TrackingReady { get; private set; }
        int trackingEpoch;
        float retryAt;
        public bool CanShow(string id) => trackedIds.Contains(id) && Loaded.TryGetValue(id, out var anchor) && anchor && anchor.IsTracked && TrackingReady;

        /// <summary>Hides loaded anchors and invalidates in-flight placement after tracking changes.</summary>
        public void InvalidateTracking()
        {
            trackingEpoch++;
            TrackingReady = false;
            headGate.Reset(); trackedIds.Clear(); poseGates.Clear(); previousPoses.Clear();
        }

        void OnApplicationPause(bool paused) { InvalidateTracking(); }
        void OnApplicationFocus(bool focused) { InvalidateTracking(); }

        void LateUpdate()
        {
            if (Application.isEditor) return;
            bool headTracked = Application.isFocused && OVRManager.hasVrFocus && OVRManager.hasInputFocus &&
                OVRManager.isHmdPresent && OVRManager.tracker != null && OVRManager.tracker.isPositionTracked && Camera.main;
            if (TrackingReady && !headTracked) InvalidateTracking();
            TrackingReady = headGate.Step(headTracked, false, Time.unscaledTime);
            trackedIds.Clear();
            foreach (var pair in Loaded)
            {
                if (!poseGates.TryGetValue(pair.Key, out var gate)) poseGates[pair.Key] = gate = new Core.TrackingGate();
                var anchor = pair.Value;
                if (!anchor) { gate.Reset(); continue; }
                var pose = new Pose(anchor.transform.position, anchor.transform.rotation);
                bool moved = !previousPoses.TryGetValue(pair.Key, out var previous) ||
                    Vector3.Distance(previous.position, pose.position) > 0.03f || Quaternion.Angle(previous.rotation, pose.rotation) > 3;
                // Compare against the start of the stable interval, not just the previous frame.
                if (moved) previousPoses[pair.Key] = pose;
                if (gate.Step(TrackingReady && anchor.Localized && anchor.IsTracked, moved, Time.unscaledTime)) trackedIds.Add(pair.Key);
            }
            if (Ready && !Busy && TrackingReady && Loaded.Count < data.bindings.Count && Time.unscaledTime >= retryAt)
            {
                retryAt = Time.unscaledTime + 15;
                _ = Load();
            }
        }

        async Task<bool> WaitForTracking()
        {
            float deadline = Time.realtimeSinceStartup + 20;
            while (this && !TrackingReady && Time.realtimeSinceStartup < deadline) await Task.Yield();
            return this && TrackingReady;
        }
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
            InvalidateTracking();
            data = new BindingFile();
            Ready = false;
            Initialize(server);
            return Ready;
        }

        /// <summary>Loads up to 50 validated records from the file for this server identity.</summary>
        /// <remarks>A corrupt file leaves Ready false rather than being replaced with an empty map.</remarks>
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
            catch { Status = L10n.T("Zuordnungsdatei beschädigt. Datei sichern und reparieren; sie wird nicht überschrieben."); }
        }

        void Persist()
        {
            string temp = path + ".tmp";
            File.WriteAllText(temp, JsonUtility.ToJson(data, true));
            if (File.Exists(path)) File.Replace(temp, path, path + ".bak");
            else File.Move(temp, path);
        }

        /// <summary>Finds saved anchors after tracking settles; missing rooms remain eligible for retry.</summary>
        public async Task Load()
        {
            if (Busy || !Ready) return;
            Busy = true;
            Status = L10n.T("Gespeicherte Positionen werden gesucht …");
            try
            {
                if (!await WaitForTracking()) { Status = L10n.T("Raumtracking wird gesucht. Bitte umsehen; Positionen werden automatisch erneut geladen."); return; }
                var pending = data.bindings.Where(b => !Loaded.ContainsKey(b.entityId)).ToArray();
                if (pending.Length > 0)
                {
                    var unbound = new List<OVRSpatialAnchor.UnboundAnchor>();
                    var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(pending.Select(b => Guid.Parse(b.anchorUuid)), unbound);
                    if (!result.Success) throw new InvalidOperationException();
                    // Rooms that cannot currently be seen must not delay all other rooms serially.
                    await Task.WhenAll(unbound.Select(item => BindLoaded(item, pending)).ToArray());
                }
                int missing = data.bindings.Count - Loaded.Count;
                Status = missing == 0 ? L10n.T("Anker geladen. Icons erscheinen bei stabilem Raumtracking.") : missing + L10n.T(" Position(en) noch nicht gefunden. Bitte im jeweiligen Raum umsehen; erneuter Versuch automatisch.");
            }
            catch { Status = L10n.T("Positionen konnten nicht geladen werden. Raum/Berechtigung prüfen und erneut laden."); }
            finally { Busy = false; retryAt = Time.unscaledTime + 15; }
        }

        async Task BindLoaded(OVRSpatialAnchor.UnboundAnchor item, BindingRecord[] pending)
        {
            if (!await item.LocalizeAsync(10) || !this) return;
            var record = pending.FirstOrDefault(b => b.anchorUuid == item.Uuid.ToString());
            if (record == null || Loaded.ContainsKey(record.entityId) || !item.TryGetPose(out var pose)) return;
            var go = new GameObject("HA Anchor");
            go.transform.SetPositionAndRotation(pose.position, pose.rotation);
            var anchor = go.AddComponent<OVRSpatialAnchor>();
            item.BindTo(anchor);
            Loaded[record.entityId] = anchor;
            Added?.Invoke(record.entityId, anchor.transform);
        }

        /// <summary>Saves an SDK anchor and its entity link only while the tracking epoch remains valid.</summary>
        /// <remarks>Failures update Status and attempt to roll back an anchor already saved by the SDK.</remarks>
        public async Task Place(string entityId, Vector3 position, bool displayOnly = false, Quaternion? rotation = null)
        {
            if (Busy || !Ready || !Core.EntityPolicy.Visible(entityId) || Contains(entityId)) return;
            if (!TrackingReady) { Status = L10n.T("Bitte warten, bis das Raumtracking wieder stabil ist, und erneut platzieren."); return; }
            if (data.bindings.Count >= 50) { Status = L10n.T("Der Prototyp unterstützt bis zu 50 Zuordnungen."); return; }
            Busy = true;
            Status = L10n.T("Position wird gespeichert …");
            OVRSpatialAnchor anchor = null;
            BindingRecord added = null;
            bool saved = false;
            int epoch = trackingEpoch;
            try
            {
                var go = new GameObject("HA Anchor");
                go.transform.position = position;
                go.transform.rotation = rotation ?? Quaternion.identity;
                anchor = go.AddComponent<OVRSpatialAnchor>();
                if (!await anchor.WhenCreatedAsync() || !await anchor.WhenLocalizedAsync()) throw new InvalidOperationException();
                if (!TrackingReady || epoch != trackingEpoch) throw new InvalidOperationException();
                var result = await anchor.SaveAnchorAsync();
                if (!result.Success) throw new IOException();
                saved = true;
                if (!TrackingReady || epoch != trackingEpoch) throw new InvalidOperationException();
                added = new BindingRecord { entityId = entityId, anchorUuid = anchor.Uuid.ToString(), displayOnly = displayOnly };
                data.bindings.Add(added);
                Persist();
                Loaded[entityId] = anchor;
                Added?.Invoke(entityId, anchor.transform);
                Status = L10n.T("Position gespeichert.");
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
                Status = L10n.T("Position konnte nicht gespeichert werden. Berechtigung und Raumtracking prüfen.");
            }
            finally { Busy = false; }
        }

        /// <summary>Removes the local link before attempting SDK anchor cleanup, even if its room is absent.</summary>
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
                Status = L10n.T("Zuordnung entfernt.");
                // Removing a placement must not depend on finding its room again.
                // The persisted association is authoritative even if OS cleanup fails.
                if (!Application.isEditor)
                {
                    var cleanup = EraseSavedAnchor(Guid.Parse(record.anchorUuid));
                    if (await Task.WhenAny(cleanup, Task.Delay(5000)) != cleanup || !await cleanup)
                        Status = L10n.T("Zuordnung entfernt. Raumanker konnte noch nicht bereinigt werden.");
                }
            }
            catch { Status = Contains(id) ? L10n.T("Löschen fehlgeschlagen. Zuordnungsdatei prüfen.") : L10n.T("Zuordnung entfernt; Raumanker-Bereinigung fehlgeschlagen."); }
            finally { Busy = false; }
        }

        static async Task<bool> EraseSavedAnchor(Guid uuid)
        {
            try { return (await OVRSpatialAnchor.EraseAnchorsAsync(null, new[] { uuid })).Success; }
            catch { return false; }
        }
    }
}

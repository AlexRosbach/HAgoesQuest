using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HAgoesQuest.Core;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace HAgoesQuest
{
    [Serializable]
    public sealed class ConnectionSettings
    {
        public string baseUrl = "https://homeassistant.example.com";
        public string accessToken = "";
        public bool allowInsecureHttp;
    }

    public sealed class HaEntity
    {
        public string Id, Name, State, Unit, AreaId = "", AreaName = "";
        public string DeviceClass = "";
        public bool CanDim, CanVolume;
        public float Level;
        public string DisplayValue => State == "on" ? L10n.T("An") : State == "off" ? L10n.T("Aus") :
            State == "unavailable" ? L10n.T("Nicht verfügbar") : State == "unknown" ? L10n.T("Unbekannt") :
            State + (string.IsNullOrEmpty(Unit) ? "" : " " + Unit);
    }

    public sealed class HomeAssistantConnection : MonoBehaviour
    {
        public event Action Changed;
        public event Action ServerChanged;
        public readonly Dictionary<string, HaEntity> Entities = new Dictionary<string, HaEntity>();
        public string Status { get; private set; } = L10n.T("Verbindung wird vorbereitet …");
        public bool Online { get; private set; }
        public bool Busy { get; private set; }
        public bool Demo { get; private set; }
        public bool Connecting { get; private set; }
        public string BaseUrl => settings?.baseUrl ?? "";
        public bool AllowsInsecureHttp => remembered?.allowInsecureHttp ?? false;
        ConnectionSettings remembered;
        public bool CanReuseToken(string url, bool allowHttp)
        {
            if (remembered == null || string.IsNullOrWhiteSpace(remembered.accessToken)) return false;
            try { return HaAddress.Parse(url.Trim(), allowHttp) == HaAddress.Parse(remembered.baseUrl, remembered.allowInsecureHttp); }
            catch { return false; }
        }
        public string ServerIdentity { get; private set; }
        public float LastSnapshot { get; private set; } = -100;
        public bool Fresh => Online && Time.realtimeSinceStartup - LastSnapshot < 6;
        ConnectionSettings settings;
        Uri address;
        int generation;
        bool refresh;
        readonly Dictionary<string, string[]> areas = new Dictionary<string, string[]>();
        public string AreaStatus { get; private set; } = L10n.T("Räume noch nicht geladen");
        public bool AreasLoading { get; private set; }
        int connectionRevision;

        void Start()
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HA_QUEST_TEST_RESULT"))) { EnterDemo(); return; }
#endif
            string path = Path.Combine(Application.persistentDataPath, "connection.json");
            if (!File.Exists(path) && !ConnectionVault.Exists)
            {
                EnterDemo();
                return;
            }
            try
            {
                settings = ConnectionVault.Exists ? ConnectionVault.Read() : JsonUtility.FromJson<ConnectionSettings>(File.ReadAllText(path));
                address = HaAddress.Parse(settings.baseUrl, settings.allowInsecureHttp);
                if (string.IsNullOrWhiteSpace(settings.accessToken) || settings.accessToken.Contains("\n") || settings.accessToken.Contains("\r"))
                    throw new ArgumentException();
                ServerIdentity = address.AbsoluteUri;
                remembered = settings;
            }
            catch
            {
                Status = L10n.T("Anmeldung konnte nicht geladen werden. Im Menü neu verbinden.");
                Changed?.Invoke();
                return;
            }
            if (!ConnectionVault.Exists)
            {
                Connecting = true;
                StartCoroutine(ValidateConnection(_ => { }));
            }
            else { StartCoroutine(Poll()); RefreshAreas(); }
        }

        public void EnterDemo()
        {
            StopAllCoroutines();
            generation++;
            connectionRevision++; areas.Clear(); AreasLoading = false;
            settings = null;
            address = null;
            Demo = Online = true;
            Busy = Connecting = false;
            ServerIdentity = "demo";
            Entities.Clear();
            Entities["light.demo"] = new HaEntity { Id = "light.demo", Name = "Leselampe", State = "off", Unit = "" };
            Entities["sensor.demo_temperature"] = new HaEntity { Id = "sensor.demo_temperature", Name = "Raumtemperatur", State = "21.5", Unit = "°C" };
            Entities["switch.demo"] = new HaEntity { Id = "switch.demo", Name = "Steckdose", State = "off", Unit = "", AreaId = "living", AreaName = "Wohnzimmer" };
            Entities["light.demo"].AreaId = "living"; Entities["light.demo"].AreaName = "Wohnzimmer";
            Entities["sensor.demo_temperature"].AreaId = "kitchen"; Entities["sensor.demo_temperature"].AreaName = "Küche";
            AreaStatus = L10n.T("Demo-Räume");
            LastSnapshot = Time.realtimeSinceStartup;
            Status = L10n.T("DEMO · Über „Verbinden“ dein Zuhause einrichten.");
            ServerChanged?.Invoke();
            Changed?.Invoke();
        }

        public void Connect(string url, string token, bool allowHttp, Action<bool> done)
        {
            if (Connecting || Busy) { Status = L10n.T("Bitte den laufenden Vorgang abwarten."); done(false); return; }
            Uri candidate;
            try
            {
                candidate = HaAddress.Parse(url.Trim(), allowHttp);
                // A blank field reuses only validated credentials for the exact same server.
                if (string.IsNullOrWhiteSpace(token) && CanReuseToken(url, allowHttp)) token = remembered.accessToken;
                if (string.IsNullOrWhiteSpace(token) || token.Contains("\n") || token.Contains("\r")) throw new ArgumentException();
            }
            catch { Status = L10n.T("Basis-URL und Token prüfen. HTTP muss ausdrücklich erlaubt sein."); done(false); return; }
            StopAllCoroutines();
            generation++;
            connectionRevision++; areas.Clear(); AreasLoading = false;
            Demo = Online = Busy = false;
            Connecting = true;
            settings = new ConnectionSettings { baseUrl = candidate.AbsoluteUri, accessToken = token.Trim(), allowInsecureHttp = allowHttp };
            address = candidate;
            StartCoroutine(ValidateConnection(done));
        }

        IEnumerator ValidateConnection(Action<bool> done)
        {
            Status = L10n.T("Verbindung wird geprüft …");
            bool success = false;
            using (var request = Request("api/states", UnityWebRequest.kHttpVerbGET))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var next = ParseEntities(request.downloadHandler.text);
                        bool saved = ConnectionVault.Save(settings);
                        remembered = settings;
                        Entities.Clear();
                        foreach (var entity in next) Entities[entity.Id] = entity;
                        ServerIdentity = address.AbsoluteUri;
                        LastSnapshot = Time.realtimeSinceStartup;
                        Online = success = true;
                        Status = saved ? L10n.T("Verbunden · Anmeldung geschützt gespeichert.") : L10n.T("Verbunden · Anmeldung gilt nur für diese Editor-Sitzung.");
                        string legacy = Path.Combine(Application.persistentDataPath, "connection.json");
                        if (saved && File.Exists(legacy)) File.Delete(legacy);
                    }
                    catch { Status = L10n.T("Anmeldung konnte nicht sicher gespeichert oder Antwort nicht gelesen werden."); }
                }
                else Status = request.responseCode == 401 || request.responseCode == 403
                    ? L10n.T("Zugriff abgelehnt · Token und Berechtigung prüfen.")
                    : L10n.T("Verbindung fehlgeschlagen · Adresse, WLAN und Zertifikat prüfen.");
            }
            Connecting = false;
            if (success) { ServerChanged?.Invoke(); StartCoroutine(Poll()); RefreshAreas(); }
            Changed?.Invoke();
            done(success);
        }

        public void ForgetConnection()
        {
            ConnectionVault.Forget();
            remembered = null;
            EnterDemo();
        }

        void Update() { if (Demo) LastSnapshot = Time.realtimeSinceStartup; }

        UnityWebRequest Request(string path, string method, string body = null)
        {
            var request = new UnityWebRequest(new Uri(address, path), method);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = 8;
            request.redirectLimit = 0;
            request.SetRequestHeader("Authorization", "Bearer " + settings.accessToken);
            if (body != null)
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                request.SetRequestHeader("Content-Type", "application/json");
            }
            return request;
        }

        IEnumerator Poll()
        {
            while (true)
            {
                if (!Busy)
                {
                    int revision = generation;
                    using (var request = Request("api/states", UnityWebRequest.kHttpVerbGET))
                    {
                        yield return request.SendWebRequest();
                        // A response started before a command must not undo the newer state.
                        if (revision == generation && !Busy)
                        {
                            if (request.result == UnityWebRequest.Result.Success)
                            {
                                try
                                {
                                    var next = ParseEntities(request.downloadHandler.text);
                                    Entities.Clear();
                                    foreach (var entity in next) { ApplyArea(entity); Entities[entity.Id] = entity; }
                                    Online = true;
                                    LastSnapshot = Time.realtimeSinceStartup;
                                    Status = L10n.T("Home Assistant verbunden");
                                }
                                catch { Online = false; Status = L10n.T("Ungültige Antwort von Home Assistant."); }
                            }
                            else
                            {
                                Online = false;
                                Status = request.responseCode == 401 || request.responseCode == 403
                                    ? L10n.T("Zugriff abgelehnt · Token/Berechtigung prüfen.")
                                    : L10n.T("Home Assistant nicht erreichbar · Verbindung wird erneut versucht.");
                            }
                            Changed?.Invoke();
                        }
                    }
                }
                float until = Time.realtimeSinceStartup + (Online ? 1.5f : 5f);
                while (!refresh && Time.realtimeSinceStartup < until) yield return null;
                refresh = false;
            }
        }

        static List<HaEntity> ParseEntities(string json)
        {
            var result = new List<HaEntity>();
            foreach (JObject item in JArray.Parse(json))
            {
                string id = (string)item["entity_id"];
                if (!EntityPolicy.Visible(id)) continue;
                var attributes = item["attributes"] as JObject;
                bool light = EntityPolicy.Domain(id) == "light";
                var colorModes = attributes?["supported_color_modes"] as JArray;
                result.Add(new HaEntity { Id = id, Name = (string)attributes?["friendly_name"] ?? id,
                    State = (string)item["state"] ?? "unknown", Unit = (string)attributes?["unit_of_measurement"] ?? "",
                    DeviceClass = (string)attributes?["device_class"] ?? "",
                    CanDim = light && colorModes != null && colorModes.Any(m => new[] { "brightness", "color_temp", "hs", "xy", "rgb", "rgbw", "rgbww", "white" }.Contains((string)m)),
                    CanVolume = EntityPolicy.Domain(id) == "media_player" && (((int?)attributes?["supported_features"] ?? 0) & 4) != 0,
                    Level = Mathf.Clamp01(light ? ((float?)attributes?["brightness"] ?? 0) / 255f : (float?)attributes?["volume_level"] ?? 0) });
            }
            return result;
        }

        void ApplyArea(HaEntity entity)
        {
            if (areas.TryGetValue(entity.Id, out var area)) { entity.AreaId = area[0]; entity.AreaName = area[1]; }
        }

        public void RefreshAreas()
        {
            if (Demo || Connecting || AreasLoading || address == null) return;
            StartCoroutine(LoadAreas(connectionRevision));
        }

        IEnumerator LoadAreas(int revision)
        {
            AreasLoading = true; AreaStatus = L10n.T("Räume werden geladen …");
            // HA resolves the entity's explicit area, falling back to its device's area.
            const string template = "[{% for s in states %}{\"entity_id\":{{ s.entity_id | to_json }},\"area_id\":{{ area_id(s.entity_id) | to_json }},\"area_name\":{{ area_name(s.entity_id) | to_json }}}{% if not loop.last %},{% endif %}{% endfor %}]";
            using (var request = Request("api/template", UnityWebRequest.kHttpVerbPOST, new JObject { ["template"] = template }.ToString()))
            {
                yield return request.SendWebRequest();
                if (revision != connectionRevision) yield break;
                try
                {
                    if (request.result != UnityWebRequest.Result.Success) throw new InvalidOperationException();
                    var parsed = JArray.Parse(request.downloadHandler.text);
                    areas.Clear();
                    foreach (JObject item in parsed)
                    {
                        string id = (string)item["entity_id"];
                        if (id != null) areas[id] = new[] { (string)item["area_id"] ?? "", (string)item["area_name"] ?? "" };
                    }
                    foreach (var entity in Entities.Values) ApplyArea(entity);
                    AreaStatus = L10n.T("Home-Assistant-Räume geladen");
                }
                catch { AreaStatus = L10n.T("Räume nicht verfügbar · neu laden oder HA-Berechtigung prüfen"); }
            }
            AreasLoading = false; Changed?.Invoke();
        }

        public bool Switch(string id)
        {
            if (Busy || !Fresh || !Entities.TryGetValue(id, out var entity) || !EntityPolicy.CanSwitch(id, entity.State)) return false;
            if (Demo)
            {
                entity.State = entity.State == "on" ? "off" : "on";
                InteractionFeedback.Instance?.Play(id);
                Changed?.Invoke();
                return true;
            }
            Busy = true;
            generation++;
            StartCoroutine(SendCommand(entity.Id, EntityPolicy.Domain(id), EntityPolicy.Service(id, entity.State)));
            return true;
        }

        public bool SetLevel(string id, float value)
        {
            if (Busy || !Fresh || float.IsNaN(value) || float.IsInfinity(value) || !Entities.TryGetValue(id, out var entity) ||
                (!entity.CanDim && !entity.CanVolume) || entity.State == "unavailable" || entity.State == "unknown") return false;
            value = Mathf.Clamp01(value);
            if (Demo) { entity.Level = value; Changed?.Invoke(); return true; }
            Busy = true; generation++;
            var body = new JObject { ["entity_id"] = id };
            body[entity.CanDim ? "brightness_pct" : "volume_level"] = entity.CanDim ? value * 100 : value;
            StartCoroutine(SendCommand(id, entity.CanDim ? "light" : "media_player", entity.CanDim ? "turn_on" : "volume_set", body));
            return true;
        }

        public UnityWebRequest MediaRequest(string id)
        {
            string domain = EntityPolicy.Domain(id);
            if (Demo || !Online || address == null || !Entities.ContainsKey(id) || (domain != "camera" && domain != "image")) return null;
            // Only the configured HA origin gets the bearer token. Never follow entity_picture URLs or redirects.
            return Request("api/" + (domain == "camera" ? "camera_proxy/" : "image_proxy/") + id, UnityWebRequest.kHttpVerbGET);
        }

        IEnumerator SendCommand(string id, string domain, string service, JObject parameters = null)
        {
            Status = L10n.T("Befehl wird gesendet …");
            Changed?.Invoke();
            using (var request = Request("api/services/" + domain + "/" + service, UnityWebRequest.kHttpVerbPOST,
                (parameters ?? new JObject { ["entity_id"] = id }).ToString()))
            {
                yield return request.SendWebRequest();
                // Never retry commands: a timeout does not mean HA did not execute them.
                Status = request.result == UnityWebRequest.Result.Success
                    ? L10n.T("Befehl gesendet · Status wird abgefragt …")
                    : L10n.T("Befehl nicht bestätigt · Status prüfen und bei Bedarf erneut auslösen.");
                if (request.result == UnityWebRequest.Result.Success && parameters == null) InteractionFeedback.Instance?.Play(id);
            }
            Busy = false;
            Online = false;
            LastSnapshot = -100;
            refresh = true;
            Changed?.Invoke();
        }

        void OnApplicationPause(bool paused)
        {
            if (!paused) { Online = Demo; LastSnapshot = -100; refresh = true; }
        }
    }
}

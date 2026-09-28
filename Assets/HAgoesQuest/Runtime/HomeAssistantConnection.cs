using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        public string Id, Name, State, Unit;
        public string DisplayValue => State == "on" ? "An" : State == "off" ? "Aus" :
            State == "unavailable" ? "Nicht verfügbar" : State == "unknown" ? "Unbekannt" :
            State + (string.IsNullOrEmpty(Unit) ? "" : " " + Unit);
    }

    public sealed class HomeAssistantConnection : MonoBehaviour
    {
        public event Action Changed;
        public event Action ServerChanged;
        public readonly Dictionary<string, HaEntity> Entities = new Dictionary<string, HaEntity>();
        public string Status { get; private set; } = "Verbindung wird vorbereitet …";
        public bool Online { get; private set; }
        public bool Busy { get; private set; }
        public bool Demo { get; private set; }
        public bool Connecting { get; private set; }
        public string BaseUrl => settings?.baseUrl ?? "";
        public string ServerIdentity { get; private set; }
        public float LastSnapshot { get; private set; } = -100;
        public bool Fresh => Online && Time.realtimeSinceStartup - LastSnapshot < 6;
        ConnectionSettings settings;
        Uri address;
        int generation;
        bool refresh;

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
            }
            catch
            {
                Status = "Anmeldung konnte nicht geladen werden. Im Menü neu verbinden.";
                Changed?.Invoke();
                return;
            }
            if (!ConnectionVault.Exists)
            {
                Connecting = true;
                StartCoroutine(ValidateConnection(_ => { }));
            }
            else StartCoroutine(Poll());
        }

        public void EnterDemo()
        {
            StopAllCoroutines();
            generation++;
            settings = null;
            address = null;
            Demo = Online = true;
            Busy = Connecting = false;
            ServerIdentity = "demo";
            Entities.Clear();
            Entities["light.demo"] = new HaEntity { Id = "light.demo", Name = "Leselampe", State = "off", Unit = "" };
            Entities["sensor.demo_temperature"] = new HaEntity { Id = "sensor.demo_temperature", Name = "Raumtemperatur", State = "21.5", Unit = "°C" };
            LastSnapshot = Time.realtimeSinceStartup;
            Status = "DEMO · Über „Verbinden“ dein Zuhause einrichten.";
            ServerChanged?.Invoke();
            Changed?.Invoke();
        }

        public void Connect(string url, string token, bool allowHttp, Action<bool> done)
        {
            if (Connecting || Busy) { Status = "Bitte den laufenden Vorgang abwarten."; done(false); return; }
            Uri candidate;
            try
            {
                candidate = HaAddress.Parse(url.Trim(), allowHttp);
                if (string.IsNullOrWhiteSpace(token) || token.Contains("\n") || token.Contains("\r")) throw new ArgumentException();
            }
            catch { Status = "Basis-URL und Token prüfen. HTTP muss ausdrücklich erlaubt sein."; done(false); return; }
            StopAllCoroutines();
            generation++;
            Demo = Online = Busy = false;
            Connecting = true;
            settings = new ConnectionSettings { baseUrl = candidate.AbsoluteUri, accessToken = token.Trim(), allowInsecureHttp = allowHttp };
            address = candidate;
            StartCoroutine(ValidateConnection(done));
        }

        IEnumerator ValidateConnection(Action<bool> done)
        {
            Status = "Verbindung wird geprüft …";
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
                        Entities.Clear();
                        foreach (var entity in next) Entities[entity.Id] = entity;
                        ServerIdentity = address.AbsoluteUri;
                        LastSnapshot = Time.realtimeSinceStartup;
                        Online = success = true;
                        Status = saved ? "Verbunden · Anmeldung geschützt gespeichert." : "Verbunden · Anmeldung gilt nur für diese Editor-Sitzung.";
                        string legacy = Path.Combine(Application.persistentDataPath, "connection.json");
                        if (saved && File.Exists(legacy)) File.Delete(legacy);
                    }
                    catch { Status = "Anmeldung konnte nicht sicher gespeichert oder Antwort nicht gelesen werden."; }
                }
                else Status = request.responseCode == 401 || request.responseCode == 403
                    ? "Zugriff abgelehnt · Token und Berechtigung prüfen."
                    : "Verbindung fehlgeschlagen · Adresse, WLAN und Zertifikat prüfen.";
            }
            Connecting = false;
            if (success) { ServerChanged?.Invoke(); StartCoroutine(Poll()); }
            Changed?.Invoke();
            done(success);
        }

        public void ForgetConnection()
        {
            ConnectionVault.Forget();
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
                                    foreach (var entity in next) Entities[entity.Id] = entity;
                                    Online = true;
                                    LastSnapshot = Time.realtimeSinceStartup;
                                    Status = "Home Assistant verbunden";
                                }
                                catch { Online = false; Status = "Ungültige Antwort von Home Assistant."; }
                            }
                            else
                            {
                                Online = false;
                                Status = request.responseCode == 401 || request.responseCode == 403
                                    ? "Zugriff abgelehnt · Token/Berechtigung prüfen."
                                    : "Home Assistant nicht erreichbar · Verbindung wird erneut versucht.";
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
                result.Add(new HaEntity { Id = id, Name = (string)attributes?["friendly_name"] ?? id,
                    State = (string)item["state"] ?? "unknown", Unit = (string)attributes?["unit_of_measurement"] ?? "" });
            }
            return result;
        }

        public void Switch(string id)
        {
            if (Busy || !Fresh || !Entities.TryGetValue(id, out var entity) || !EntityPolicy.CanSwitch(id, entity.State)) return;
            if (Demo)
            {
                entity.State = entity.State == "on" ? "off" : "on";
                Changed?.Invoke();
                return;
            }
            Busy = true;
            generation++;
            StartCoroutine(SendCommand(entity.Id, EntityPolicy.Domain(id), EntityPolicy.Service(id, entity.State)));
        }

        IEnumerator SendCommand(string id, string domain, string service)
        {
            Status = "Befehl wird gesendet …";
            Changed?.Invoke();
            using (var request = Request("api/services/" + domain + "/" + service, UnityWebRequest.kHttpVerbPOST,
                new JObject { ["entity_id"] = id }.ToString()))
            {
                yield return request.SendWebRequest();
                // Never retry commands: a timeout does not mean HA did not execute them.
                Status = request.result == UnityWebRequest.Result.Success
                    ? "Befehl gesendet · Status wird abgefragt …"
                    : "Befehl nicht bestätigt · Status prüfen und bei Bedarf erneut auslösen.";
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

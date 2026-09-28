#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Networking;

namespace HAgoesQuest.Testing
{
    // Invoked only in an isolated editor process by tools/Test-Network.ps1.
    // The endpoint is a loopback mock. No real Home Assistant instance is contacted.
    public sealed class PlaymodeProbe : MonoBehaviour
    {
        HomeAssistantConnection ha;
        int assertions;
        string resultPath;

        public static void Run()
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HA_QUEST_TEST_RESULT")))
                throw new InvalidOperationException("Run using the test script.");
            EditorSceneManager.OpenScene("Assets/HAgoesQuest/Scenes/Home.unity");
            var hands = UnityEngine.Object.FindObjectsByType<OVRHand>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (hands.Length != 2 || hands.Any(h => h.GetComponentsInChildren<Renderer>(true).Any(r => r.enabled) ||
                h.GetComponentsInChildren<OVRMeshRenderer>(true).Any(r => r.enabled)))
                throw new InvalidOperationException("Both tracked hands must have all visual renderers disabled.");
            if (!UnityEngine.Object.FindFirstObjectByType<OVRPassthroughLayer>() || !UnityEngine.Object.FindFirstObjectByType<QrTokenScanner>())
                throw new InvalidOperationException("Passthrough/QR components missing from the actual scene.");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorApplication.isPlaying = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HA_QUEST_TEST_RESULT"))) return;
            var host = new GameObject("Network integration probe");
            host.AddComponent<HomeAssistantConnection>();
            host.AddComponent<PlaymodeProbe>();
        }

        IEnumerator Start()
        {
            resultPath = Environment.GetEnvironmentVariable("HA_QUEST_TEST_RESULT");
            ha = GetComponent<HomeAssistantConnection>();
            yield return null;
            string testToken = "example-token-only-for-tests-123456789";
            if (!Check(QrConnection.TryParse(testToken, out _, out var raw) && raw == testToken, "Raw QR token accepted")) yield break;
            string payload = new JObject { ["baseUrl"] = "https://ha.example.com", ["accessToken"] = testToken }.ToString();
            if (!Check(QrConnection.TryParse(payload, out var qrUrl, out var qrToken) && qrUrl == "https://ha.example.com" && qrToken == testToken, "QR connection JSON parsed")) yield break;
            if (!Check(!QrConnection.TryParse("https://evil.example/token", out _, out _) && !QrConnection.TryParse("{broken", out _, out _), "QR links and malformed JSON rejected")) yield break;
            if (!Check(!QrConnection.TryParse(new JObject { ["baseUrl"] = "https://ha.example.com/api?token=x", ["accessToken"] = testToken }.ToString(), out _, out _), "QR URL credentials/path/query rejected")) yield break;
            var writer = new ZXing.BarcodeWriterPixelData { Format = ZXing.BarcodeFormat.QR_CODE, Options = new ZXing.Common.EncodingOptions { Width = 512, Height = 512, Margin = 4 } };
            var pixels = writer.Write(payload);
            var reader = new ZXing.BarcodeReaderGeneric { AutoRotate = true };
            var decoded = reader.Decode(pixels.Pixels, pixels.Width, pixels.Height, ZXing.RGBLuminanceSource.BitmapFormat.BGRA32);
            if (!Check(decoded != null && QrConnection.TryParse(decoded.Text, out _, out var roundTrip) && roundTrip == testToken, "Actual QR encode/decode round-trip")) yield break;
            var marker = WorldCard.Create(null, Vector3.zero, new Vector2(340, 115), "Test name");
            marker.SetIcon("switch"); marker.MakeCompactMarker();
            if (!Check(marker.GetComponent<UnityEngine.UI.Image>().enabled && marker.transform.Find("Menu style ring") && marker.GetComponent<BoxCollider>().size.x == 90, "Compact marker has circular menu-style background and ring")) yield break;
            marker.SetDistance(1.5f); yield return new WaitForSeconds(0.5f);
            float distantScale = marker.transform.localScale.x;
            marker.SetDistance(0.25f); yield return new WaitForSeconds(0.5f);
            if (!Check(marker.transform.localScale.x > distantScale * 1.7f, "Approaching enlarges the actual marker transform and hit target")) yield break;
            marker.SetNearby(false); yield return new WaitForSeconds(0.2f);
            if (!Check(marker.Label.color.a == 0, "Distant entity label hidden")) yield break;
            marker.SetNearby(true); yield return new WaitForSeconds(0.2f);
            if (!Check(marker.Label.color.a > 0.99f, "Nearby entity label appears")) yield break;
            Destroy(marker.gameObject);
            var bindingHost = new GameObject("Isolated placement persistence test");
            var spatial = bindingHost.AddComponent<SpatialBindings>();
            spatial.Initialize("test-unlocalized-delete-" + Guid.NewGuid());
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            var stored = (BindingFile)typeof(SpatialBindings).GetField("data", flags).GetValue(spatial);
            stored.bindings.Add(new BindingRecord { entityId = "light.missing_from_ha", anchorUuid = Guid.NewGuid().ToString() });
            typeof(SpatialBindings).GetMethod("Persist", flags).Invoke(spatial, null);
            string isolatedPath = (string)typeof(SpatialBindings).GetField("path", flags).GetValue(spatial);
            bool removedEvent = false;
            spatial.Removed += _ => removedEvent = true;
            var deletion = spatial.Remove("light.missing_from_ha");
            yield return Wait(() => deletion.IsCompleted, 8, "Unlocalized placement removal completes");
            if (!Check(!spatial.Contains("light.missing_from_ha") && !spatial.EntityIds.Any() && removedEvent &&
                JsonUtility.FromJson<BindingFile>(File.ReadAllText(isolatedPath)).bindings.Count == 0,
                "Unlocalized and unavailable entity removal persists without a loaded anchor")) yield break;
            File.Delete(isolatedPath); if (File.Exists(isolatedPath + ".bak")) File.Delete(isolatedPath + ".bak");
            var ui = bindingHost.AddComponent<QuestHome>(); ui.enabled = false;
            var testHead = new GameObject("Test head").transform;
            void Set(string field, object value) => typeof(QuestHome).GetField(field, flags).SetValue(ui, value);
            object Get(string field) => typeof(QuestHome).GetField(field, flags).GetValue(ui);
            void Invoke(string method, params object[] args) => typeof(QuestHome).GetMethod(method, flags).Invoke(ui, args);
            void Press(string prefix) => ((GameObject)Get("menu")).GetComponentsInChildren<WorldCard>().Single(c => c.Press != null && c.Label.text.StartsWith(prefix)).Activate();
            Set("head", testHead); Set("ha", ha); Set("bindings", spatial);
            Invoke("BeginPlacement", "light.ui_test", 1f);
            Invoke("FreezePlacement", new Vector3(0.2f, 0, 1));
            if (!Check((bool)Get("placementFrozen") && ((WorldCard)Get("preview")).IsMarker &&
                ((WorldCard)Get("preview")).transform.position == new Vector3(0.2f, 0, 1), "One-pinch placement freezes compact preview before saving")) yield break;
            Press("Weiter verschieben");
            if (!Check(!(bool)Get("placementFrozen"), "Placement can be adjusted before committing")) yield break;
            Invoke("FreezePlacement", new Vector3(0.3f, 0, 1));
            Press("Position speichern");
            Invoke("PlacedMenu", 0);
            testHead.position = new Vector3(3, 0, 2);
            Invoke("OnApplicationFocus", true);
            if (!Check(Vector3.Distance(((GameObject)Get("menu")).transform.position, testHead.position) < 1.5f,
                "Menu returns in front of user after system focus/room change")) yield break;
            Press("Löschen"); Press("Ja");
            if (!Check(!((System.Collections.Generic.Dictionary<string, WorldCard>)Get("markers")).Any(),
                "Placed-entity management deletes an entity absent from HA listing")) yield break;
            Invoke("CloseMenu"); Destroy(testHead.gameObject);
            Destroy(bindingHost);
            bool completed = false, accepted = false;
            ha.Connect("http://127.0.0.1:18123", "test-token", true, ok => { completed = true; accepted = ok; });
            yield return Wait(() => completed, 12, "Connection callback");
            if (!Check(accepted && ha.Fresh && !ha.Demo, "Authenticated HTTP connection")) yield break;
            if (!Check(ha.Entities.ContainsKey("light.test") && ha.Entities["sensor.temperature"].Unit == "°C", "Parse actual JSON state and unit")) yield break;
            yield return Wait(() => !ha.AreasLoading, 12, "Area metadata loaded");
            if (!Check(ha.Entities["light.test"].AreaId == "living" && ha.Entities["sensor.temperature"].AreaName == "Küche", "HA room mapping applied")) yield break;
            var filter = new EntityBrowser { Domain = "light", Area = "living", Search = "TEST" };
            if (!Check(filter.Apply(ha.Entities.Values).Single().Id == "light.test", "Type, room and case-insensitive search combine")) yield break;
            filter.Area = "kitchen";
            if (!Check(!filter.Apply(ha.Entities.Values).Any(), "Conflicting filters give zero results")) yield break;
            filter.Domain = ""; filter.Area = "__none"; filter.Search = "lock.door";
            if (!Check(filter.Apply(ha.Entities.Values).Single().Id == "lock.door", "Unassigned area and ID search; other domains display only")) yield break;
            ha.Switch("light.test");
            yield return Wait(() => !ha.Busy && ha.Fresh && ha.Entities["light.test"].State == "on", 12, "Command and state refresh");
            if (!Check(ha.Entities["light.test"].AreaId == "living", "Room survives state polling")) yield break;
            ha.Switch("sensor.temperature");
            yield return Stats(1);
            yield return Control("offline");
            yield return Wait(() => !ha.Fresh, 12, "Connection becomes stale/offline");
            ha.Switch("light.test");
            yield return Stats(1);
            yield return Control("online");
            yield return Wait(() => ha.Fresh, 15, "Automatic connection recovery");
            completed = accepted = false;
            ha.Connect("http://127.0.0.1:18123", "invalid-token", true, ok => { completed = true; accepted = ok; });
            yield return Wait(() => completed, 12, "Authentication rejection callback");
            if (!Check(!accepted && !ha.Online, "Invalid token is rejected")) yield break;
            if (!Check(ha.CanReuseToken("http://127.0.0.1:18123/", true), "Failed replacement preserves last validated credentials")) yield break;
            if (!Check(!ha.CanReuseToken("http://127.0.0.1:18124", true) && !ha.CanReuseToken("https://127.0.0.1:18123", true), "Stored token cannot follow a changed port or scheme")) yield break;
            completed = accepted = false;
            ha.Connect("http://127.0.0.1:18124", "", true, ok => { completed = true; accepted = ok; });
            if (!Check(completed && !accepted, "Different server requires an explicit token before networking")) yield break;
            completed = accepted = false;
            ha.Connect("http://127.0.0.1:18123/", "", true, ok => { completed = true; accepted = ok; });
            yield return Wait(() => completed, 12, "Reconnect using remembered credentials callback");
            if (!Check(accepted && ha.Fresh && ha.AllowsInsecureHttp, "Blank token reconnects with previous validated token and HTTP preference")) yield break;
            completed = accepted = false;
            ha.Connect("http://127.0.0.1:18123", "test-token", false, ok => { completed = true; accepted = ok; });
            if (!Check(completed && !accepted, "HTTP requires opt-in in the actual client")) yield break;
            File.WriteAllText(resultPath, "PASS: " + assertions + " Unity network integration assertions.");
            EditorApplication.Exit(0);
        }

        IEnumerator Wait(Func<bool> predicate, float seconds, string name)
        {
            float until = Time.realtimeSinceStartup + seconds;
            while (!predicate() && Time.realtimeSinceStartup < until) yield return null;
            Check(predicate(), name);
        }

        IEnumerator Control(string command)
        {
            using (var request = UnityWebRequest.Get("http://127.0.0.1:18123/test/" + command))
            { yield return request.SendWebRequest(); Check(request.result == UnityWebRequest.Result.Success, command); }
        }

        IEnumerator Stats(int expected)
        {
            using (var request = UnityWebRequest.Get("http://127.0.0.1:18123/test/stats"))
            {
                yield return request.SendWebRequest();
                Check(request.result == UnityWebRequest.Result.Success && (int)JObject.Parse(request.downloadHandler.text)["calls"] == expected,
                    "No unauthorized sensor/offline commands");
            }
        }

        bool Check(bool condition, string name)
        {
            assertions++;
            if (condition) return true;
            File.WriteAllText(resultPath, "FAIL: " + name);
            EditorApplication.Exit(1);
            throw new InvalidOperationException("Integration assertion failed: " + name);
        }
    }
}
#endif

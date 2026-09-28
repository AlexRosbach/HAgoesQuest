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
            if (!Check(!marker.GetComponent<UnityEngine.UI.Image>().enabled && marker.GetComponent<BoxCollider>().size.x == 90, "Compact icon has no card background")) yield break;
            marker.SetNearby(false); yield return new WaitForSeconds(0.2f);
            if (!Check(marker.Label.color.a == 0, "Distant entity label hidden")) yield break;
            marker.SetNearby(true); yield return new WaitForSeconds(0.2f);
            if (!Check(marker.Label.color.a > 0.99f, "Nearby entity label appears")) yield break;
            Destroy(marker.gameObject);
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

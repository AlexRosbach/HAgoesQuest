#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
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
            bool completed = false, accepted = false;
            ha.Connect("http://127.0.0.1:18123", "test-token", true, ok => { completed = true; accepted = ok; });
            yield return Wait(() => completed, 12, "Connection callback");
            if (!Check(accepted && ha.Fresh && !ha.Demo, "Authenticated HTTP connection")) yield break;
            if (!Check(ha.Entities.ContainsKey("light.test") && ha.Entities["sensor.temperature"].Unit == "°C", "Parse actual JSON state and unit")) yield break;
            ha.Switch("light.test");
            yield return Wait(() => !ha.Busy && ha.Fresh && ha.Entities["light.test"].State == "on", 12, "Command and state refresh");
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

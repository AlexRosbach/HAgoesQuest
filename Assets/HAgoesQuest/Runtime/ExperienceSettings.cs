using UnityEngine;

namespace HAgoesQuest
{
    public static class ExperienceSettings
    {
        const string Prefix = "HAgoesQuest.experience.";
        static bool Get(string key, bool fallback = false) => PlayerPrefs.GetInt(Prefix + key, fallback ? 1 : 0) == 1;
        static void Set(string key, bool value) { PlayerPrefs.SetInt(Prefix + key, value ? 1 : 0); PlayerPrefs.Save(); }
        public static bool Wrist { get => Get("wrist"); set => Set("wrist", value); }
        public static bool LeftHanded { get => Get("leftHanded"); set => Set("leftHanded", value); }
        public static bool Sound { get => Get("sound", true); set => Set("sound", value); }
        public static bool GestureHints { get => Get("gestureHints", true); set => Set("gestureHints", value); }
        public static bool HandGlow { get => Get("handGlow", true); set => Set("handGlow", value); }
        static bool english;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void LoadLanguage() { english = Get("english"); }
        // Translation is also used by serialized component field initializers;
        // those must not call Unity's PlayerPrefs API.
        public static bool English { get => english; set { english = value; Set("english", value); } }
    }
}

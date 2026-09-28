using UnityEngine;

namespace HAgoesQuest
{
    // Original synthesized sounds; no recorded or third-party audio assets.
    public sealed class InteractionFeedback : MonoBehaviour
    {
        AudioSource source;
        AudioClip lightClick, switchClick;
        public static InteractionFeedback Instance { get; private set; }
        void Awake()
        {
            Instance = this; source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false; source.spatialBlend = 0; source.volume = 0.2f;
            lightClick = Create(false); switchClick = Create(true);
        }
        static AudioClip Create(bool mechanical)
        {
            int count = mechanical ? 4800 : 2400;
            var samples = new float[count]; var noise = new System.Random(73);
            for (int i = 0; i < count; i++)
            {
                float t = i / 48000f;
                float impulse = Mathf.Exp(-t * (mechanical ? 65 : 130));
                float second = mechanical && t > 0.028f ? Mathf.Exp(-(t - 0.028f) * 140) * 0.45f : 0;
                samples[i] = (float)(noise.NextDouble() * 2 - 1) * (impulse + second) * 0.35f +
                    Mathf.Sin(t * (mechanical ? 900 : 1600) * Mathf.PI * 2) * impulse * 0.35f;
            }
            var clip = AudioClip.Create(mechanical ? "Switch relay" : "Light click", count, 1, 48000, false);
            clip.SetData(samples, 0); return clip;
        }
        public void Play(string entityId)
        {
            if (ExperienceSettings.Sound && source) source.PlayOneShot(entityId.StartsWith("switch.") ? switchClick : lightClick);
        }
        void OnDestroy() { if (Instance == this) Instance = null; Destroy(lightClick); Destroy(switchClick); }
    }
}

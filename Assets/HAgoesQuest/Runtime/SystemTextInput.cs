using System;
using UnityEngine;

namespace HAgoesQuest
{
    public sealed class SystemTextInput : MonoBehaviour
    {
        TouchScreenKeyboard overlay;
        Action<string> accept;
        Action cancel;
        public static void Open(Transform parent, string initial, bool secret, Action<string> accept, Action cancel)
        {
            if (Application.isEditor) { new WorldKeyboard(parent, initial, secret, accept, cancel); return; }
            var input = parent.gameObject.AddComponent<SystemTextInput>();
            input.accept = accept; input.cancel = cancel;
            // Only Default is supported by Meta's system keyboard overlay.
            input.overlay = TouchScreenKeyboard.Open(initial, TouchScreenKeyboardType.Default, false, false, secret, false, "", 4096);
            if (input.overlay == null) { Destroy(input); cancel(); }
        }
        void Update()
        {
            if (overlay == null) return;
            if (overlay.status == TouchScreenKeyboard.Status.Done)
            {
                string value = overlay.text; overlay = null; accept(value); Destroy(this);
            }
            else if (overlay.status == TouchScreenKeyboard.Status.Canceled || overlay.status == TouchScreenKeyboard.Status.LostFocus)
            { overlay = null; cancel(); Destroy(this); }
        }
        void OnDestroy() { if (overlay != null) overlay.active = false; }
    }
}

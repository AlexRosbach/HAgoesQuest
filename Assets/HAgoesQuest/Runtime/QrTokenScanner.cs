using System;
using System.Collections;
using System.Threading.Tasks;
using Meta.XR;
using UnityEngine;
using UnityEngine.Rendering;
using ZXing;
using ZXing.Common;

namespace HAgoesQuest
{
    public sealed class QrTokenScanner : MonoBehaviour
    {
        public string Status { get; private set; }
        public bool Running { get; private set; }
        Action<string, string> found;
        GameObject cameraHost;
        PassthroughCameraAccess cameraAccess;
        static BarcodeReaderGeneric Reader() => new BarcodeReaderGeneric {
            AutoRotate = true,
            Options = new DecodingOptions { TryHarder = true, PossibleFormats = new[] { BarcodeFormat.QR_CODE } }
        };
        int session;

        public void Begin(Action<string, string> callback)
        {
            Stop(); found = callback; Running = true;
            StartCoroutine(Scan(++session));
        }

        IEnumerator Scan(int revision)
        {
            if (Application.isEditor || !PassthroughCameraAccess.IsSupported)
            { Status = L10n.T("QR-Scan benötigt eine Quest 3/3S mit Horizon OS 74 oder neuer."); Running = false; yield break; }
#if UNITY_ANDROID && !UNITY_EDITOR
            const string permission = "horizonos.permission.HEADSET_CAMERA";
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(permission))
            {
                Status = L10n.T("Kamerazugriff für den QR-Scan erlauben …");
                bool answered = false;
                var callbacks = new UnityEngine.Android.PermissionCallbacks();
                callbacks.PermissionGranted += _ => answered = true;
                callbacks.PermissionDenied += _ => answered = true;
                callbacks.PermissionDeniedAndDontAskAgain += _ => answered = true;
                UnityEngine.Android.Permission.RequestUserPermission(permission, callbacks);
                float deadline = Time.realtimeSinceStartup + 60;
                while (!answered && Time.realtimeSinceStartup < deadline && revision == session) yield return null;
                if (revision != session) yield break;
                if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(permission))
                { Status = L10n.T("Kamerazugriff nicht erlaubt. Token kann weiterhin eingegeben werden."); Running = false; yield break; }
            }
#endif
            cameraHost = new GameObject("QR camera (scan only)");
            cameraHost.SetActive(false);
            cameraAccess = cameraHost.AddComponent<PassthroughCameraAccess>();
            cameraAccess.enabled = false;
            cameraAccess.RequestedResolution = new Vector2Int(1280, 960);
            cameraAccess.MaxFramerate = 15;
            cameraAccess.enabled = true;
            cameraHost.SetActive(true);
            Status = L10n.T("QR-Code auf dem Handy vor die Quest halten. Kamera wird gestartet …");
            float readyUntil = Time.realtimeSinceStartup + 15;
            while (!cameraAccess.IsPlaying && Time.realtimeSinceStartup < readyUntil) yield return null;
            if (!cameraAccess.IsPlaying)
            { Fail(L10n.T("Kamera nicht verfügbar. Berechtigung und Headset-Version prüfen.")); yield break; }
            Status = L10n.T("QR-Code ruhig, hell und groß vor die Quest halten. Bilder bleiben auf der Quest.");
            while (revision == session && Running)
            {
                var texture = cameraAccess.GetTexture();
                if (!texture) { yield return null; continue; }
                int width = texture.width, height = texture.height;
                var request = AsyncGPUReadback.Request(texture, 0, TextureFormat.RGBA32);
                while (!request.done) yield return null;
                if (request.hasError) { Fail(L10n.T("Kamerabild konnte nicht gelesen werden. Bitte erneut versuchen.")); yield break; }
                var bytes = request.GetData<byte>().ToArray();
                var decode = Task.Run(() => {
                    try { return Reader().Decode(bytes, width, height, RGBLuminanceSource.BitmapFormat.RGBA32); }
                    finally { Array.Clear(bytes, 0, bytes.Length); }
                });
                while (!decode.IsCompleted) yield return null;
                if (revision != session) yield break;
                if (decode.IsFaulted) { Fail("QR-Code konnte nicht verarbeitet werden."); yield break; }
                if (decode.Result != null)
                {
                    if (QrConnection.TryParse(decode.Result.Text, out var url, out var token))
                    {
                        var callback = found; Stop(); callback?.Invoke(url, token); yield break;
                    }
                    Status = L10n.T("Kein gültiger HA-Zugangscode. Token als Text oder Verbindungs-JSON verwenden.");
                }
                yield return new WaitForSecondsRealtime(0.35f);
            }
        }

        void Fail(string message) { Stop(); Status = message; }
        public void Stop()
        {
            session++; Running = false; found = null; StopAllCoroutines();
            if (cameraHost) { cameraHost.SetActive(false); Destroy(cameraHost); }
            cameraHost = null; cameraAccess = null;
        }
        void OnDisable() => Stop();
        void OnApplicationPause(bool paused) { if (paused && cameraHost) Fail(L10n.T("Scan pausiert. Bitte erneut starten.")); }
    }
}

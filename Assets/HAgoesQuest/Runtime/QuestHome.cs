using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HAgoesQuest.Core;
using Meta.XR.MRUtilityKit;
using UnityEngine;

namespace HAgoesQuest
{
    public sealed class QuestHome : MonoBehaviour
    {
        public OVRCameraRig Rig;
        public OVRHand LeftHand;
        public OVRHand RightHand;
        HomeAssistantConnection ha;
        SpatialBindings bindings;
        readonly FocusPinchGate gate = new FocusPinchGate();
        readonly Dictionary<string, WorldCard> markers = new Dictionary<string, WorldCard>();
        Transform head;
        GameObject menu;
        WorldCard header, status, focused, preview, reticle;
        string placing;
        float placementDistance;
        bool previousMenuPinch, scanBusy, operationBusy;
        string notice = "";
        int page;

        IEnumerator Start()
        {
            head = Rig.centerEyeAnchor;
            ha = GetComponent<HomeAssistantConnection>();
            bindings = GetComponent<SpatialBindings>();
            bindings.Added += AddMarker;
            bindings.Removed += RemoveMarker;
            reticle = WorldCard.Create(null, Vector3.zero, new Vector2(16, 16), "");
            reticle.GetComponent<Collider>().enabled = false;
            Home();
            while (string.IsNullOrEmpty(ha.ServerIdentity)) yield return null;
            bindings.Initialize(ha.ServerIdentity);
            if (Application.platform == RuntimePlatform.Android) _ = bindings.Load();
        }

        static bool HandReady(OVRHand hand) => hand && hand.IsTracked &&
            hand.HandConfidence == OVRHand.TrackingConfidence.High && !hand.IsSystemGestureInProgress;

        void Update()
        {
            if (!head || ha == null) return;
#if UNITY_EDITOR
            if (Input.GetMouseButton(1))
            {
                Rig.transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * 2, Space.World);
                Rig.transform.Rotate(Vector3.right, -Input.GetAxis("Mouse Y") * 2, Space.Self);
            }
#endif
            bool leftDown = HandReady(LeftHand) && LeftHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
            bool menuPressed = (leftDown && !previousMenuPinch) || OVRInput.GetDown(OVRInput.RawButton.X) || Input.GetKeyDown(KeyCode.Tab);
            previousMenuPinch = leftDown;
            if (menuPressed && !operationBusy) { CancelPlacement(); Home(); }

            bool hand = HandReady(RightHand);
            bool controller = !hand && OVRInput.IsControllerConnected(OVRInput.Controller.RTouch) &&
                OVRInput.GetControllerPositionTracked(OVRInput.Controller.RTouch);
            bool tracked = hand || controller;
            bool down = hand ? RightHand.GetFingerIsPinching(OVRHand.HandFinger.Index) :
                controller && OVRInput.Get(OVRInput.RawButton.RIndexTrigger);
            if (Application.isEditor) { tracked = true; down = Input.GetMouseButton(0); }

            if (status) status.Label.text = Short(ha.Status + "\n" + (string.IsNullOrEmpty(notice) ? bindings.Status : notice), 185);
            foreach (var pair in markers)
            {
                var card = pair.Value;
                bool localized = !bindings.Loaded.TryGetValue(pair.Key, out var anchor) || anchor.Localized;
                card.gameObject.SetActive(localized);
                if (!localized) continue;
                card.GetComponent<Collider>().enabled = !menu && !operationBusy && placing == null;
                card.Face(head);
                if (ha.Entities.TryGetValue(pair.Key, out var entity))
                {
                    string value = ha.Fresh ? entity.DisplayValue : "Veraltet · " + entity.DisplayValue;
                    card.Label.text = Short(entity.Name, 42) + "\n" + Short(value, 42) + (ha.Demo ? " · DEMO" : "");
                    card.Interactable = !menu && !operationBusy && ha.Fresh && !ha.Busy && EntityPolicy.CanSwitch(entity.Id, entity.State);
                }
                else { card.Label.text = pair.Key + "\nNicht verfügbar"; card.Interactable = false; }
            }

            Ray ray = new Ray(head.position, head.forward);
            Vector3 aim = ray.GetPoint(1.5f);
            if (focused) focused.Focus(false);
            focused = null;
            string key = null;
            bool placementValid = false;
            if (placing != null)
            {
                if (placementDistance > 0)
                {
                    aim = ray.GetPoint(placementDistance);
                    placementValid = true;
                }
                else
                {
                    var room = MRUK.Instance.GetCurrentRoom();
                    if (room != null && room.Raycast(ray, 8, out var hit))
                    {
                        aim = hit.point - ray.direction * 0.07f;
                        placementValid = hit.distance > 0.25f;
                    }
                }
                preview.transform.position = aim;
                preview.Face(head);
                preview.Label.text = placementValid ? "Hier platzieren\nRechts pinchen · Links: zurück" : "Keine Scanfläche\nLinks pinchen: zurück";
                key = placementValid ? "place:" + placing : null;
            }
            else if (!operationBusy && Physics.Raycast(ray, out var hit, 8))
            {
                var card = hit.collider.GetComponent<WorldCard>();
                if (card && card.Interactable && card.Press != null)
                {
                    focused = card;
                    focused.Focus(true);
                    key = card.Key;
                }
                aim = hit.point - ray.direction * 0.01f;
            }
            reticle.transform.position = aim;
            reticle.Face(head);
            reticle.Focus(key != null);
            if (gate.Step(key, down, tracked && !operationBusy && Application.isFocused, Time.realtimeSinceStartup))
            {
                if (placing != null && placementValid) Place(aim);
                else if (focused) focused.Press?.Invoke();
            }
        }

        void NewMenu(string title)
        {
            if (menu) { menu.SetActive(false); Destroy(menu); }
            gate.Reset();
            menu = new GameObject("HA Menu");
            Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;
            menu.transform.SetPositionAndRotation(head.position + forward * 1.4f, Quaternion.LookRotation(forward));
            header = WorldCard.Create(menu.transform, new Vector3(0, 0.39f, 0), new Vector2(760, 90), title);
            header.Interactable = false;
            status = WorldCard.Create(menu.transform, new Vector3(0, -0.43f, 0), new Vector2(760, 150), "");
            status.Label.fontSize = 19;
            status.Interactable = false;
        }

        void Button(int row, string label, Action action)
        {
            WorldCard.Create(menu.transform, new Vector3(0, 0.275f - row * 0.1f, 0), new Vector2(720, 86), label, action);
        }

        void CloseMenu()
        {
            if (menu) { menu.SetActive(false); Destroy(menu); }
            menu = null;
            gate.Reset();
        }

        void Home()
        {
            notice = "";
            NewMenu("HA goes Quest\nAnvisieren · kurz warten · rechts pinchen");
            Button(0, "Entität zuordnen", () => { page = 0; EntityMenu(); });
            Button(1, "Raum laden / erstmals scannen", () => Scan(false));
            Button(2, "Raum neu scannen", () => Scan(true));
            Button(3, "Gespeicherte Positionen erneut laden", LoadBindings);
            Button(4, "Menü schließen · Links pinchen öffnet es wieder", CloseMenu);
        }

        void EntityMenu()
        {
            var entities = ha.Entities.Values.OrderBy(e => e.Name, StringComparer.CurrentCultureIgnoreCase).ThenBy(e => e.Id).ToArray();
            int pages = Math.Max(1, (entities.Length + 3) / 4);
            page = Math.Min(page, pages - 1);
            NewMenu("Entitäten · Seite " + (page + 1) + "/" + pages);
            for (int i = 0; i < 4 && page * 4 + i < entities.Length; i++)
            {
                var entity = entities[page * 4 + i];
                Button(i, Short(entity.Name, 34) + " · " + entity.Id + (bindings.Contains(entity.Id) ? " ✓" : ""), () => Details(entity));
            }
            Button(4, "Nächste Seite", () => { page = (page + 1) % pages; EntityMenu(); });
            Button(5, "Zurück", Home);
        }

        void Details(HaEntity entity)
        {
            NewMenu(Short(entity.Name, 45) + "\n" + entity.Id);
            if (bindings.Contains(entity.Id))
            {
                Button(0, "Zuordnung entfernen", async () =>
                {
                    if (operationBusy) return;
                    operationBusy = true;
                    try { await bindings.Remove(entity.Id); }
                    finally { operationBusy = false; gate.Reset(); }
                    Details(entity);
                });
                Button(1, "Zurück", EntityMenu);
                return;
            }
            Button(0, "Auf gescannter Fläche platzieren", () => BeginPlacement(entity.Id, 0));
            Button(1, "Frei platzieren · Abstand 0,5 m", () => BeginPlacement(entity.Id, 0.5f));
            Button(2, "Frei platzieren · Abstand 1 m", () => BeginPlacement(entity.Id, 1));
            Button(3, "Frei platzieren · Abstand 2 m", () => BeginPlacement(entity.Id, 2));
            Button(4, "Zurück", EntityMenu);
        }

        void BeginPlacement(string id, float distance)
        {
            if (!bindings.Ready || bindings.Busy) { notice = "Positionen werden noch vorbereitet. " + bindings.Status; return; }
            if (distance == 0 && MRUK.Instance.GetCurrentRoom() == null)
            { notice = "Zuerst im Hauptmenü den Raum laden/scannen."; return; }
            CloseMenu();
            placing = id;
            placementDistance = distance;
            preview = WorldCard.Create(null, Vector3.zero, new Vector2(420, 110), "");
            preview.GetComponent<Collider>().enabled = false;
        }

        async void Place(Vector3 position)
        {
            if (operationBusy) return;
            string id = placing;
            CancelPlacement();
            operationBusy = true;
            string resultNotice = null;
            try
            {
                if (Application.isEditor)
                {
                    var temporary = new GameObject("Editor preview anchor");
                    temporary.transform.position = position;
                    AddMarker(id, temporary.transform);
                    resultNotice = "Editor-Vorschau · Position wird erst auf der Quest dauerhaft gespeichert.";
                }
                else await bindings.Place(id, position);
            }
            finally { operationBusy = false; gate.Reset(); }
            Home();
            if (resultNotice != null) notice = resultNotice;
        }

        void CancelPlacement()
        {
            placing = null;
            if (preview) Destroy(preview.gameObject);
            gate.Reset();
        }

        void AddMarker(string id, Transform anchor)
        {
            RemoveMarker(id);
            var card = WorldCard.Create(anchor, Vector3.zero, new Vector2(340, 115), id, () => ha.Switch(id));
            card.EntityId = id;
            card.IsMarker = true;
            markers[id] = card;
        }

        void RemoveMarker(string id)
        {
            if (markers.TryGetValue(id, out var card)) { Destroy(card.gameObject); markers.Remove(id); }
        }

        async void LoadBindings()
        {
            if (operationBusy) return;
            operationBusy = true;
            try { await bindings.Load(); }
            finally { operationBusy = false; gate.Reset(); }
        }

        async void Scan(bool rescan)
        {
            if (scanBusy || operationBusy) return;
            if (Application.isEditor) { notice = "Raumscan ist auf der Quest verfügbar. Im Editor frei platzieren."; return; }
            scanBusy = operationBusy = true;
            notice = "Raumerfassung wird geöffnet …";
            try
            {
                if (!await ScenePermission()) { notice = "Raumzugriff abgelehnt. In den Quest-App-Berechtigungen erlauben."; return; }
                if (rescan && !await OVRScene.RequestSpaceSetup()) { notice = "Raumscan abgebrochen oder nicht verfügbar."; return; }
                var result = await MRUK.Instance.LoadSceneFromDevice(!rescan);
                notice = result == MRUK.LoadDeviceResult.Success ? "Raum geladen. Jetzt Entitäten zuordnen." : "Kein Raum geladen. Raumscan und Berechtigung prüfen.";
            }
            catch { notice = "Raumerfassung fehlgeschlagen. Bitte auf der Quest erneut versuchen."; }
            finally { scanBusy = operationBusy = false; gate.Reset(); }
        }

        static Task<bool> ScenePermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            const string permission = "com.oculus.permission.USE_SCENE";
            if (UnityEngine.Android.Permission.HasUserAuthorizedPermission(permission)) return Task.FromResult(true);
            var completion = new TaskCompletionSource<bool>();
            var callbacks = new UnityEngine.Android.PermissionCallbacks();
            callbacks.PermissionGranted += _ => completion.TrySetResult(true);
            callbacks.PermissionDenied += _ => completion.TrySetResult(false);
            callbacks.PermissionDeniedAndDontAskAgain += _ => completion.TrySetResult(false);
            UnityEngine.Android.Permission.RequestUserPermission(permission, callbacks);
            return completion.Task;
#else
            return Task.FromResult(true);
#endif
        }

        static string Short(string value, int length) => value.Length <= length ? value : value.Substring(0, length - 1) + "…";
        void OnApplicationFocus(bool focus) { gate.Reset(); }
        void OnApplicationPause(bool pause) { gate.Reset(); }
    }
}

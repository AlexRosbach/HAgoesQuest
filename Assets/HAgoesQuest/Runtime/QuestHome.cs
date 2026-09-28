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
        bool handPlacement;
        bool placementReadOnly;
        readonly HandPlacement placement = new HandPlacement();
        Vector3 heldPosition;
        OVRSkeleton rightSkeleton;
        WorldKeyboard keyboard;
        string draftUrl = "https://", draftToken = "";
        bool allowHttp;
        bool previousMenuPinch, scanBusy, operationBusy;
        string notice = "";
        int page;
        readonly List<Vector3> walkTrail = new List<Vector3>();

        IEnumerator Start()
        {
            head = Rig.centerEyeAnchor;
            ha = GetComponent<HomeAssistantConnection>();
            bindings = GetComponent<SpatialBindings>();
            bindings.Added += AddMarker;
            bindings.Removed += RemoveMarker;
            ha.ServerChanged += ServerChanged;
            rightSkeleton = RightHand ? RightHand.GetComponent<OVRSkeleton>() : null;
            reticle = WorldCard.Create(null, Vector3.zero, new Vector2(16, 16), "");
            reticle.GetComponent<Collider>().enabled = false;
            Home();
            while (string.IsNullOrEmpty(ha.ServerIdentity)) yield return null;
            if (!bindings.Ready) ServerChanged();
        }

        void ServerChanged()
        {
            if (!bindings || string.IsNullOrEmpty(ha.ServerIdentity)) return;
            if (bindings.Busy) return;
            foreach (var card in markers.Values) if (card) Destroy(card.gameObject);
            markers.Clear();
            if (bindings.ResetForServer(ha.ServerIdentity) && Application.platform == RuntimePlatform.Android) _ = bindings.Load();
        }

        static bool HandReady(OVRHand hand) => hand && hand.IsTracked &&
            hand.HandConfidence == OVRHand.TrackingConfidence.High && !hand.IsSystemGestureInProgress;

        void Update()
        {
            if (!head || ha == null) return;
            bool inputAllowed = Application.isFocused && (Application.isEditor || (OVRManager.hasInputFocus && OVRManager.hasVrFocus));
            if (MRUK.Instance.GetCurrentRoom() != null && (walkTrail.Count == 0 || Vector3.Distance(walkTrail[walkTrail.Count - 1], head.position) > 0.3f))
            {
                walkTrail.Add(head.position);
                if (walkTrail.Count > 1600) walkTrail.RemoveRange(0, 400);
            }
#if UNITY_EDITOR
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null && mouse.rightButton.isPressed)
            {
                Vector2 delta = mouse.delta.ReadValue();
                Rig.transform.Rotate(Vector3.up, delta.x * 0.15f, Space.World);
                Rig.transform.Rotate(Vector3.right, -delta.y * 0.15f, Space.Self);
            }
#endif
            bool leftDown = HandReady(LeftHand) && LeftHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
            bool menuPressed = (leftDown && !previousMenuPinch) || OVRInput.GetDown(OVRInput.RawButton.X);
#if UNITY_EDITOR
            menuPressed |= UnityEngine.InputSystem.Keyboard.current?.tabKey.wasPressedThisFrame == true;
#endif
            previousMenuPinch = leftDown;
            if (menuPressed && inputAllowed && !operationBusy && !ha.Connecting) { CancelPlacement(); Home(); }

            bool hand = HandReady(RightHand);
            bool controller = !hand && OVRInput.IsControllerConnected(OVRInput.Controller.RTouch) &&
                OVRInput.GetControllerPositionTracked(OVRInput.Controller.RTouch);
            bool tracked = hand || controller;
            bool down = hand ? RightHand.GetFingerIsPinching(OVRHand.HandFinger.Index) :
                controller && OVRInput.Get(OVRInput.RawButton.RIndexTrigger);
#if UNITY_EDITOR
            tracked = true;
            down = mouse != null && mouse.leftButton.isPressed;
#endif

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
                    card.SetState(entity.State);
                    card.Interactable = !card.ReadOnly && !menu && !operationBusy && ha.Fresh && !ha.Busy && EntityPolicy.CanSwitch(entity.Id, entity.State);
                }
                else { card.Label.text = pair.Key + "\nNicht verfügbar"; card.Interactable = false; }
            }

            Ray ray = new Ray(head.position, head.forward);
            if (menu && hand && RightHand.IsPointerPoseValid && RightHand.PointerPose)
                ray = new Ray(RightHand.PointerPose.position, RightHand.PointerPose.forward);
            Vector3 aim = ray.GetPoint(1.5f);
            if (focused) focused.Focus(false);
            focused = null;
            string key = null;
            bool placementValid = false;
            if (placing != null && handPlacement)
            {
                bool positionTracked = TryHandPosition(out var pinchPoint);
                placement.Step(positionTracked && inputAllowed, down);
                if (placement.Phase == PlacementPhase.Holding || placement.Phase == PlacementPhase.Ready)
                    heldPosition = pinchPoint;
                aim = heldPosition;
                preview.transform.position = heldPosition;
                preview.Face(head);
                preview.Label.text = !positionTracked ? "Rechte Hand ins Sichtfeld halten" :
                    placement.Phase == PlacementPhase.Holding ? "Bewegen · Loslassen zum Ablegen" : "Rechts greifen und an deinen Wunschort bewegen";
                if (placement.Phase == PlacementPhase.Confirm && !menu) ConfirmHandPlacement();
                if (menu && Physics.Raycast(ray, out var confirmHit, 8))
                {
                    var candidate = confirmHit.collider.GetComponent<WorldCard>();
                    if (candidate && candidate.Interactable && candidate.Press != null) { focused = candidate; focused.Focus(true); key = candidate.Key; }
                    aim = confirmHit.point;
                }
            }
            else if (placing != null)
            {
                if (placementDistance > 0)
                {
                    aim = ray.GetPoint(placementDistance);
                    placementValid = true;
                }
                else
                {
                    var room = MRUK.Instance.GetCurrentRoom();
                    if (room != null && room.Raycast(ray, 8, out var surfaceHit))
                    {
                        aim = surfaceHit.point - ray.direction * 0.07f;
                        placementValid = surfaceHit.distance > 0.25f;
                    }
                }
                preview.transform.position = aim;
                preview.Face(head);
                preview.Label.text = placementValid ? "Hier platzieren\nRechts pinchen · Links: zurück" : "Keine Scanfläche\nLinks pinchen: zurück";
                key = placementValid ? "place:" + placing : null;
            }
            else if (!operationBusy && Physics.Raycast(ray, out var cardHit, 8))
            {
                var card = cardHit.collider.GetComponent<WorldCard>();
                if (card && card.Interactable && card.Press != null)
                {
                    focused = card;
                    focused.Focus(true);
                    key = card.Key;
                }
                aim = cardHit.point - ray.direction * 0.01f;
            }
            reticle.transform.position = aim;
            reticle.Face(head);
            reticle.Focus(key != null);
            if (gate.Step(key, down, tracked && !operationBusy && inputAllowed, Time.realtimeSinceStartup))
            {
                if (placing != null && placementValid) Place(aim);
                else if (focused) focused.Activate();
            }
        }

        void NewMenu(string title)
        {
            keyboard = null;
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

        void Button(int row, string label, Action action, string icon = null)
        {
            var card = WorldCard.Create(menu.transform, new Vector3(0, 0.275f - row * 0.1f, 0), new Vector2(720, 86), label, action);
            card.SetIcon(icon);
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
            NewMenu("HA goes Quest\nDein Zuhause. Direkt vor dir.");
            Button(0, ha.Demo ? "Mit Home Assistant verbinden" : "Home-Assistant-Verbindung", ConnectionMenu, "lamp");
            Button(1, "Entität mit der Hand platzieren", () => { page = 0; EntityMenu(); }, "lamp");
            Button(2, "Wohnung erfassen & Karte ansehen", MapMenu, "map");
            Button(3, "Gespeicherte Positionen wiederfinden", LoadBindings, "map");
            Button(4, "Menü schließen · Linker Pinch öffnet es", CloseMenu);
        }

        void ConnectionMenu()
        {
            if (bindings.Busy || operationBusy) { notice = "Bitte warten, bis die Positionen geladen sind."; return; }
            if (draftUrl == "https://" && !string.IsNullOrEmpty(ha.BaseUrl)) draftUrl = ha.BaseUrl;
            NewMenu("Verbinde dein Zuhause");
            Button(0, "Adresse: " + Short(draftUrl, 47), () => OpenKeyboard(false));
            Button(1, draftToken.Length == 0 ? "Zugangstoken eingeben / einfügen" : "Zugangstoken bereit · " + draftToken.Length + " Zeichen", () => OpenKeyboard(true));
            Button(2, allowHttp ? "Lokales HTTP: erlaubt (unverschlüsselt)" : "Lokales HTTP: aus · HTTPS empfohlen", () => { allowHttp = !allowHttp; ConnectionMenu(); });
            Button(3, "Verbindung prüfen & speichern", () =>
            {
                if (bindings.Busy || ha.Connecting) return;
                operationBusy = true;
                ha.Connect(draftUrl, draftToken, allowHttp, success =>
                {
                    operationBusy = false;
                    gate.Reset();
                    if (success) { draftToken = ""; Home(); }
                });
            }, "lamp");
            Button(4, "Anmeldung auf diesem Gerät entfernen", () =>
            {
                NewMenu("Anmeldung entfernen?");
                Button(0, "Ja · Token auf dieser Quest löschen", () =>
                {
                    try { ha.ForgetConnection(); draftToken = ""; Home(); }
                    catch { notice = "Anmeldung konnte nicht entfernt werden."; }
                });
                Button(1, "Zurück", ConnectionMenu);
            });
            Button(5, "Zurück", Home);
            notice = "In Home Assistant: Profil → Sicherheit → langlebigen Zugriffstoken erstellen. Hier einfügen. Anmeldung wird auf der Quest verschlüsselt gespeichert.";
        }

        void OpenKeyboard(bool token)
        {
            NewMenu(token ? "Home-Assistant-Zugangstoken" : "Adresse deines Home Assistant");
            keyboard = new WorldKeyboard(menu.transform, token ? draftToken : draftUrl, token,
                value => { if (token) draftToken = value; else draftUrl = value; ConnectionMenu(); }, ConnectionMenu);
            notice = token ? "Der Token bleibt verdeckt. „Einfügen“ nutzt die Zwischenablage der Quest." : "Zum Beispiel https://ha.example.com oder http://192.168.1.10:8123";
        }

        void MapMenu()
        {
            NewMenu("Erfasse deine Wohnung");
            Button(0, "Vorhandene Räume laden / ersten Scan starten", () => Scan(false), "map");
            Button(1, "Durch die Wohnung gehen & neu erfassen", () => Scan(true), "map");
            Button(2, "Wohnungskarte ansehen", ShowMap, "map");
            Button(3, "Entität im Raum platzieren", () => { page = 0; EntityMenu(); }, "lamp");
            Button(4, "Zurück", Home);
            notice = "Die Quest-Raumerfassung führt dich durch den Scan. Danach zeigt die Karte geladene Räume, Geräte und deinen aktuellen Standort.";
        }

        void ShowMap()
        {
            if (MRUK.Instance.Rooms.Count == 0) { notice = "Zuerst Räume laden oder die Wohnung erfassen."; return; }
            NewMenu("Deine Wohnung · " + MRUK.Instance.Rooms.Count + " Raum/Räume");
            var card = WorldCard.Create(menu.transform, new Vector3(0, 0.055f, 0), new Vector2(740, 420), "Grün: Räume · Gold: Geräte · Weiß: Du");
            card.Interactable = false;
            card.Label.alignment = TextAnchor.LowerCenter;
            card.Label.fontSize = 18;
            var mapObject = new GameObject("Apartment map", typeof(RectTransform), typeof(ApartmentMap));
            mapObject.transform.SetParent(card.transform, false);
            var rect = mapObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(20, 40); rect.offsetMax = new Vector2(-20, -15);
            var map = mapObject.GetComponent<ApartmentMap>();
            map.raycastTarget = false;
            map.Head = head; map.Bindings = bindings; map.Trail = walkTrail;
            Button(5, "Zurück", MapMenu);
            notice = "Die blaue Spur zeigt deinen Weg in dieser Sitzung. Weitere Räume über die Quest-Raumerfassung hinzufügen.";
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
                Button(i, Short(entity.Name, 30) + "\n" + entity.Id + (bindings.Contains(entity.Id) ? " ✓" : ""), () => { placementReadOnly = !EntityPolicy.CanSwitch(entity.Id, entity.State); Details(entity); }, IconFor(entity.Id));
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
            Button(0, "Mit der Hand greifen & platzieren", () => BeginHandPlacement(entity.Id), IconFor(entity.Id));
            Button(1, "Auf gescannter Fläche platzieren", () => BeginPlacement(entity.Id, 0), "map");
            Button(2, "Frei vor mir platzieren · 1 Meter", () => BeginPlacement(entity.Id, 1));
            if (EntityPolicy.Domain(entity.Id) == "light" || EntityPolicy.Domain(entity.Id) == "switch")
                Button(3, placementReadOnly ? "Modus: nur anzeigen" : "Modus: anzeigen & schalten", () => { placementReadOnly = !placementReadOnly; Details(entity); });
            Button(4, "Zurück", EntityMenu);
        }

        void BeginPlacement(string id, float distance)
        {
            if (!bindings.Ready || bindings.Busy) { notice = "Positionen werden noch vorbereitet. " + bindings.Status; return; }
            if (distance == 0 && MRUK.Instance.GetCurrentRoom() == null)
            { notice = "Zuerst im Hauptmenü den Raum laden/scannen."; return; }
            CloseMenu();
            placing = id;
            handPlacement = false;
            placementDistance = distance;
            preview = WorldCard.Create(null, Vector3.zero, new Vector2(420, 110), "");
            preview.GetComponent<Collider>().enabled = false;
            preview.SetIcon(IconFor(id));
        }

        void BeginHandPlacement(string id)
        {
            BeginPlacement(id, 1);
            if (placing == null) return;
            handPlacement = true;
            placement.Reset();
            heldPosition = head.position + head.forward * 0.5f;
        }

        bool TryHandPosition(out Vector3 point)
        {
            point = heldPosition;
            if (HandReady(RightHand) && rightSkeleton && rightSkeleton.Bones != null)
            {
                Transform index = null, thumb = null;
                foreach (var bone in rightSkeleton.Bones)
                {
                    if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip || bone.Id == OVRSkeleton.BoneId.XRHand_IndexTip) index = bone.Transform;
                    if (bone.Id == OVRSkeleton.BoneId.Hand_ThumbTip || bone.Id == OVRSkeleton.BoneId.XRHand_ThumbTip) thumb = bone.Transform;
                }
                if (index && thumb) { point = (index.position + thumb.position) * 0.5f; return true; }
            }
            if (OVRInput.IsControllerConnected(OVRInput.Controller.RTouch) && OVRInput.GetControllerPositionTracked(OVRInput.Controller.RTouch))
            { point = Rig.rightControllerAnchor.position + Rig.rightControllerAnchor.forward * 0.12f; return true; }
            if (Application.isEditor) { point = head.position + head.forward * 0.6f; return true; }
            return false;
        }

        void ConfirmHandPlacement()
        {
            NewMenu("Hier bleibt deine Entität");
            Button(0, "Position speichern", () => Place(heldPosition), IconFor(placing));
            Button(1, "Weiter verschieben", () => { CloseMenu(); placement.Reset(); });
            Button(2, "Abbrechen", () => { CancelPlacement(); Home(); });
            preview.Pulse();
        }

        async void Place(Vector3 position)
        {
            if (operationBusy) return;
            string id = placing;
            bool displayOnly = placementReadOnly;
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
                    markers[id].ReadOnly = displayOnly;
                    resultNotice = "Editor-Vorschau · Position wird erst auf der Quest dauerhaft gespeichert.";
                }
                else await bindings.Place(id, position, displayOnly);
            }
            finally { operationBusy = false; gate.Reset(); }
            Home();
            if (resultNotice != null) notice = resultNotice;
        }

        void CancelPlacement()
        {
            placing = null;
            handPlacement = false;
            if (preview) Destroy(preview.gameObject);
            gate.Reset();
        }

        void AddMarker(string id, Transform anchor)
        {
            RemoveMarker(id);
            var card = WorldCard.Create(anchor, Vector3.zero, new Vector2(340, 115), id, () => ha.Switch(id));
            card.EntityId = id;
            card.IsMarker = true;
            card.ReadOnly = bindings.DisplayOnly(id);
            card.SetIcon(IconFor(id));
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
        static string IconFor(string id) => EntityPolicy.Domain(id) == "sensor" || EntityPolicy.Domain(id) == "binary_sensor" ? "sensor" : "lamp";
        void OnApplicationFocus(bool focus)
        {
            gate.Reset();
            if (!focus && placement.Phase != PlacementPhase.Confirm) placement.Reset();
        }
        void OnApplicationPause(bool pause)
        {
            gate.Reset();
            if (pause && placement.Phase != PlacementPhase.Confirm) placement.Reset();
        }
    }
}

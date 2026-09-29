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
    [DefaultExecutionOrder(11000)]
    public sealed partial class QuestHome : MonoBehaviour
    {
        public OVRCameraRig Rig;
        public OVRHand LeftHand;
        public OVRHand RightHand;
        HomeAssistantConnection ha;
        SpatialBindings bindings;
        readonly FocusPinchGate gate = new FocusPinchGate(0, 0.4);
        readonly Dictionary<string, WorldCard> markers = new Dictionary<string, WorldCard>();
        Transform head;
        GameObject menu;
        WorldCard header, status, focused, preview, reticle;
        string placing;
        float placementDistance;
        bool handPlacement;
        bool placementFrozen;
        float freeDistance = 1;
        bool placementReadOnly;
        readonly HandPlacement placement = new HandPlacement();
        Vector3 heldPosition;
        OVRSkeleton rightSkeleton;
        WorldKeyboard keyboard;
        string draftUrl = "https://", draftToken = "";
        bool allowHttp;
        bool scanBusy, operationBusy;
        readonly FocusPinchGate menuGate = new FocusPinchGate(0, 0.25);
        OVRDisplay subscribedDisplay;
        string notice = "";
        int page;
        readonly List<Vector3> walkTrail = new List<Vector3>();
        readonly EntityBrowser browser = new EntityBrowser();
        QrTokenScanner qr;
        bool qrMenu;
        LineRenderer pointer;
        MagicCasting magic;

        IEnumerator Start()
        {
            head = Rig.centerEyeAnchor;
            ha = GetComponent<HomeAssistantConnection>();
            magic = new MagicCasting();
            InitializeExperience();
            bindings = GetComponent<SpatialBindings>();
            bindings.Added += AddMarker;
            bindings.Removed += RemoveMarker;
            ha.ServerChanged += ServerChanged;
            rightSkeleton = RightHand ? RightHand.GetComponent<OVRSkeleton>() : null;
            RepairHandPointers();
            RouteHands();
            if(areaMenuPage>=0&&displayedAreaRevision!=ha.AreaRevision)AreaMenu(areaMenuPage);
            OVRManager.InputFocusAcquired += RestoreHandInput;
            subscribedDisplay = OVRManager.display;
            if (subscribedDisplay != null) subscribedDisplay.RecenteredPose += RestoreHandInput;
            qr = GetComponent<QrTokenScanner>() ?? gameObject.AddComponent<QrTokenScanner>();
            pointer = new GameObject("Finger selection ray").AddComponent<LineRenderer>();
            pointer.positionCount = 2; pointer.useWorldSpace = true;
            pointer.startWidth = 0.002f; pointer.endWidth = 0.001f;
            pointer.material = new Material(Shader.Find("Sprites/Default"));
            pointer.startColor = new Color(0.2f, 1, 0.9f, 0.8f);
            pointer.endColor = new Color(0.5f, 1, 0.95f, 0.95f);
            pointer.enabled = false;
            reticle = WorldCard.Create(null, Vector3.zero, new Vector2(16, 16), "");
            reticle.GetComponent<Collider>().enabled = false; reticle.MakeCursor();
            Home();
            if (ExperienceSettings.Wrist && !ha.Demo) CloseMenu();
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

        static bool HandReady(OVRHand hand) => hand && hand.IsTracked && !hand.IsSystemGestureInProgress;

        public static void BindPointerToTrackingSpace(OVRHand hand, Transform trackingSpace)
        {
            if (hand && hand.PointerPose && trackingSpace && hand.PointerPose.parent != trackingSpace)
                hand.PointerPose.SetParent(trackingSpace, false);
        }
        void RepairHandPointers()
        {
            if (!Rig) return;
            BindPointerToTrackingSpace(LeftHand, Rig.trackingSpace);
            BindPointerToTrackingSpace(RightHand, Rig.trackingSpace);
        }
        void RestoreHandInput()
        {
            bindings?.InvalidateTracking();
            bool wasPlacing = placing != null;
            CancelPlacement();
            if (wasPlacing && head && ha) Home();
            ResetGaze(); RepairHandPointers(); gate.Reset(); menuGate.Reset(); magic?.Cancel(); RecenterMenu();
        }
        void ToggleMenu()
        {
            CancelPlacement();
            if (menu) CloseMenu(); else Home();
        }

        void Update()
        {
            if (!head || ha == null) return;
            RefreshRoomScope();
            UpdateExperience();
            RepairHandPointers();
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
            bool leftDown = HandReady(MenuHand) && MenuHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
            bool menuContext=!capturedTarget&&!grip.Closed&&(menu?!HandOverMenuControl():!GazeTarget());
            inputAllowed &= NavigationAllowed();
            bool menuPressed = menuGate.Step("menu", leftDown, HandReady(MenuHand) && inputAllowed && menuContext, Time.unscaledTime) || OVRInput.GetDown(ExperienceSettings.LeftHanded?OVRInput.RawButton.A:OVRInput.RawButton.X);
#if UNITY_EDITOR
            menuPressed |= UnityEngine.InputSystem.Keyboard.current?.tabKey.wasPressedThisFrame == true;
#endif
            if (menuPressed && inputAllowed && !operationBusy && !ha.Connecting) ToggleMenu();

            inputAllowed &= NavigationAllowed();
            bool hand = HandReady(ControlHand);
            bool controller = !hand && OVRInput.IsControllerConnected(ControlController) &&
                OVRInput.GetControllerPositionTracked(ControlController);
            bool tracked = hand || controller;
            bool down = hand ? ControlHand.GetFingerIsPinching(OVRHand.HandFinger.Index) :
                controller && OVRInput.Get(ExperienceSettings.LeftHanded?OVRInput.RawButton.LIndexTrigger:OVRInput.RawButton.RIndexTrigger);
#if UNITY_EDITOR
            tracked = true;
            down = mouse != null && mouse.leftButton.isPressed;
#endif

            if (status) status.Label.text = Short(ha.Status + "\n" + (string.IsNullOrEmpty(notice) ? bindings.Status : notice), 185);
            if (qrMenu && status) status.Label.text = string.IsNullOrEmpty(qr.Status) ? notice : qr.Status;
            sensorGaze=!menu&&placing==null?GazeTarget():null;
            foreach (var pair in markers)
            {
                var card = pair.Value;
                bool localized = MarkerVisibleHere(pair.Key,card);
                card.gameObject.SetActive(localized);
                if (!localized) continue;
                card.GetComponent<Collider>().enabled = !menu && !operationBusy && placing == null;
                if (!card.IsMedia) card.Face(head);
                card.SetDistance(Vector3.Distance(head.position, card.transform.position));
                if (EntityPolicy.Domain(pair.Key) == "sensor" || EntityPolicy.Domain(pair.Key) == "binary_sensor") card.ShowSensorValue(LookingAt(card), Vector3.Distance(head.position,card.transform.position));
                if (ha.Entities.TryGetValue(pair.Key, out var entity))
                {
                    string value = ha.Fresh ? entity.DisplayValue : L10n.T("Veraltet · ") + entity.DisplayValue;
                    if (!card.IsMedia) card.Label.text = Short(entity.Name, 42) + "\n" + Short(value, 42) + (ha.Demo ? " · DEMO" : "");
                    card.SetState(entity.State);
                    if(!card.IsMedia)card.SetIcon(IconFor(pair.Key));
                    EnsureAdjustment(card,entity);
                    card.Interactable = !card.ReadOnly && !menu && !operationBusy && ha.Fresh && !ha.Busy && (EntityPolicy.CanSwitch(entity.Id, entity.State) || entity.CanVolume);
                }
                else { card.Label.text = pair.Key + L10n.T("\nNicht verfügbar"); card.Interactable = false; }
            }

            if(!menu && placing==null){ TickGaze(inputAllowed&&!operationBusy&&!scanBusy,tracked,down); return; }
            if(gestureHint)gestureHint.gameObject.SetActive(false);
            readinessGlow?.Hide();
            reticle.UnitScale=.001f;
            Ray ray = new Ray(head.position, head.forward);
            if (hand && ControlHand.IsPointerPoseValid && ControlHand.PointerPose)
                ray = new Ray(ControlHand.PointerPose.position, ControlHand.PointerPose.forward);
            else if (controller) ray = new Ray(ControllerAnchor.position, ControllerAnchor.forward);
            ray = SmoothAim(ray, down);
            var fingerTip=MagicCasting.Bone(ControlSkeleton,OVRSkeleton.BoneId.Hand_IndexTip,OVRSkeleton.BoneId.XRHand_IndexTip);
            metaInput?.Step(ray,down,inputAllowed&&tracked&&!operationBusy,hand&&fingerTip,fingerTip?fingerTip.position:Vector3.zero);
            Vector3 aim = ray.GetPoint(5);
            if (focused) focused.Focus(false);
            focused = null;
            string key = null;
            bool placementValid = false;
            if (placing != null && handPlacement && !placementFrozen)
            {
                bool positionTracked = TryHandPosition(out var pinchPoint);
                if (positionTracked && inputAllowed)
                    heldPosition = Vector3.Lerp(heldPosition, pinchPoint + head.forward * 0.10f, 1 - Mathf.Exp(-12 * Time.unscaledDeltaTime));
                aim = heldPosition;
                preview.transform.position = heldPosition;
                preview.Face(head);
                preview.Label.text = !positionTracked ? Words("Bedienhand ins Sichtfeld halten","Keep your control hand visible") :
                    L10n.T("Hand zum Wunschort bewegen\nEinmal pinchen: Position festhalten");
                placementValid = positionTracked;
                key = placementValid ? "place:" + placing : null;
            }
            else if (placing != null && !placementFrozen)
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
                preview.Label.text = placementValid ? L10n.T("Hier platzieren\nEinmal pinchen: Position festhalten") : L10n.T("Keine Scanfläche\nLinks pinchen: zurück");
                key = placementValid ? "place:" + placing : null;
            }
            else if (!operationBusy && metaInput && metaInput.Hovered)
            {
                var card = metaInput.Hovered;
                if (card && (card.IsMarker || (card.Interactable && card.Press != null)))
                {
                    focused = card;
                    focused.Focus(true);
                    if (card.Interactable && card.Press != null) key = card.Key;
                }
                aim = metaInput.HitPoint - ray.direction * 0.01f;
            }
            reticle.transform.position = aim;
            reticle.Face(head);
            reticle.Focus(key != null);
            reticle.gameObject.SetActive(inputAllowed && (focused || placing != null));
            pointer.enabled = inputAllowed && tracked && (focused || down || MagicCasting.IndexExtended(ControlSkeleton) || (placing != null && !placementFrozen)) &&
                (!hand || (ControlHand.IsPointerPoseValid && ControlHand.PointerPose));
            if (pointer.enabled)
            {
                Vector3 start = ray.origin;
                if (hand && ControlSkeleton && ControlSkeleton.Bones != null)
                {
                    var tip = MagicCasting.Bone(ControlSkeleton, OVRSkeleton.BoneId.Hand_IndexTip, OVRSkeleton.BoneId.XRHand_IndexTip);
                    if (tip) start = tip.position;
                }
                pointer.SetPosition(0, start); pointer.SetPosition(1, aim);
                UpdateBeam(start, aim, focused && focused.IsMarker);
            }
            else if (beamGlow) beamGlow.enabled = false;

            bool consumed = UpdateAlternativeInput(inputAllowed, tracked, down, ray);
            placementValid=placementValid&&PointInCurrentRoom(aim);
            if(placing!=null&&!placementFrozen&&!placementValid){key=null;preview.Label.text=Words("Innerhalb deines aktuellen Raums platzieren","Place inside your current room");}
            bool placementPressed=placing!=null&&!placementFrozen&&gate.Step(consumed?null:key,down,tracked&&!operationBusy&&inputAllowed,Time.realtimeSinceStartup);
            bool selected=!consumed&&metaInput&&metaInput.RaySelected&&metaInput.RaySelected==focused&&key!=null;
            if (placementPressed || selected)
            {
                if (placing != null && placementValid) FreezePlacement(aim);
                else if (focused) focused.Activate();
            }
        }

        void NewMenu(string title)
        {
            BlockNavigation();
            watchEditing=false;
            ResetGaze();
            magic?.Cancel();
            if (qrMenu) { qr.Stop(); qrMenu = false; }
            keyboard = null;
            if (menu) { menu.SetActive(false); Destroy(menu); }
            gate.Reset();
            menu = new GameObject("HA Menu",typeof(RectTransform),typeof(Canvas));
            menu.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
            menu.GetComponent<RectTransform>().sizeDelta=new Vector2(.9f,1.04f);
            entityList = null;
            Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;
            menu.transform.SetPositionAndRotation(head.position + forward * 1.4f, Quaternion.LookRotation(forward));
            header = WorldCard.Create(menu.transform, new Vector3(0, 0.39f, 0), new Vector2(820, 90), title);
            header.Interactable = false; header.SetSurface(WorldCard.Surface.Header);
            MenuDesign.Frame(menu.transform);
            status = WorldCard.Create(menu.transform, new Vector3(0, -0.44f, 0), new Vector2(800, 105), "");
            status.Label.fontSize = 19;
            status.Interactable = false; status.SetSurface(WorldCard.Surface.Caption);
        }

        void Button(int row, string label, Action action, string icon = null)
        {
            var card = WorldCard.Create(menu.transform, new Vector3(0, 0.275f - row * 0.1f, 0), new Vector2(750, 82), label, action);
            card.SetIcon(icon);
            if(row==0)card.SetSurface(WorldCard.Surface.Primary);
        }

        void CloseMenu()
        {
            BlockNavigation();
            watchEditing=false;
            ResetGaze();
            if (qrMenu) { qr.Stop(); qrMenu = false; }
            if (menu) { menu.SetActive(false); Destroy(menu); }
            menu = null;
            gate.Reset();
        }

        void Home()
        {
            notice = "";
            NewMenu(Words("Dein Zuhause","Your home"));
            HomeTile(0,Words("Entitäten\nPlatzieren & steuern","Entities\nPlace & control"),()=>EntityMenu(),"lamp",true);
            HomeTile(1,Words("Wohnung\nRäume & Karte","Apartment\nRooms & map"),MapMenu,"map");
            HomeTile(2,Words("Verbindung\nHome Assistant","Connection\nHome Assistant"),ConnectionMenu,"connection");
            HomeTile(3,Words("Einstellungen\nUhr & Gesten","Settings\nWatch & gestures"),ExperienceMenu,"settings");
            var manage=WorldCard.Create(menu.transform,new Vector3(-.19f,-.275f,0),new Vector2(365,72),Words("Zuordnungen verwalten","Manage placements"),()=>PlacedMenu(0));manage.Label.fontSize=21;
            var close=WorldCard.Create(menu.transform,new Vector3(.19f,-.275f,0),new Vector2(365,72),Words("Menü schließen","Close menu"),CloseMenu);close.Label.fontSize=21;
            notice=Words("Ansehen + pinchen · Faust drehen: regeln\nMagie: Faust schließen und öffnen","Look + pinch · Twist fist to adjust\nMagic: close and open fist");
        }
        void HomeTile(int index,string label,Action action,string shape,bool primary=false)
        {
            var card=WorldCard.Create(menu.transform,new Vector3(index%2==0?-.195f:.195f,.19f-(index/2)*.255f,0),new Vector2(375,238),label,action);
            if(primary)card.SetSurface(WorldCard.Surface.Primary);MenuDesign.Tile(card,shape);
        }

        void MagicSettings()
        {
            if (magic == null) magic = new MagicCasting();
            NewMenu(L10n.T("Magie & Positionen"));
            Button(0, magic.Enabled ? L10n.T("Magie: AN · ausschalten") : L10n.T("Magie: AUS · einschalten"), () => { magic.SetEnabled(!magic.Enabled); MagicSettings(); });
            Button(1, L10n.T("Gespeicherte Positionen wiederfinden"), LoadBindings, "map");
            Button(2, L10n.T("Übungsziel · ohne Geräte zu schalten"), StartMagicPractice);
            Button(3, L10n.T("Menü schließen & ausprobieren"), CloseMenu);
            Button(4, L10n.T("Zurück"), ExperienceMenu);
            notice = Words("Ziel ansehen, Hand öffnen bis bereit. Faust schließen, dann öffnen: Feuerball. Faust drehen regelt stattdessen Helligkeit/Lautstärke.","Look at a target, open hand until ready. Close fist, then open to cast. Twisting adjusts brightness/volume instead.");
        }

        void ConnectionMenu()
        {
            if (bindings.Busy || operationBusy) { notice = L10n.T("Bitte warten, bis die Positionen geladen sind."); return; }
            if (draftUrl == "https://" && !string.IsNullOrEmpty(ha.BaseUrl)) { draftUrl = ha.BaseUrl; allowHttp = ha.AllowsInsecureHttp; }
            NewMenu(L10n.T("Verbinde dein Zuhause"));
            Button(0, L10n.T("Adresse: ") + Short(draftUrl, 47), () => OpenKeyboard(false));
            Button(1, draftToken.Length == 0 ? (ha.CanReuseToken(draftUrl, allowHttp) ? L10n.T("Gespeicherter Token wird verwendet · ändern") : L10n.T("Zugangstoken eingeben / einfügen")) : L10n.T("Zugangstoken bereit · ") + draftToken.Length + L10n.T(" Zeichen"), () => OpenKeyboard(true));
            Button(2, allowHttp ? L10n.T("Lokales HTTP: erlaubt (unverschlüsselt)") : L10n.T("Lokales HTTP: aus · HTTPS empfohlen"), () => { allowHttp = !allowHttp; ConnectionMenu(); });
            Button(3, L10n.T("Verbindung prüfen & speichern"), () =>
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
            Button(4, L10n.T("Token per QR-Code vom Handy einlesen"), QrMenu);
            Button(5, L10n.T("Zurück / Anmeldung verwalten"), () =>
            {
                NewMenu(L10n.T("Verbindung verwalten"));
                Button(0, L10n.T("Zurück zum Hauptmenü"), Home);
                Button(1, L10n.T("Anmeldung auf diesem Gerät entfernen"), () =>
                {
                    NewMenu(L10n.T("Anmeldung entfernen?"));
                    Button(0, L10n.T("Ja · Token auf dieser Quest löschen"), () =>
                    {
                        try { ha.ForgetConnection(); draftToken = ""; Home(); }
                        catch { notice = L10n.T("Anmeldung konnte nicht entfernt werden."); }
                    });
                    Button(1, L10n.T("Abbrechen"), ConnectionMenu);
                });
                Button(2, L10n.T("Zurück zur Verbindung"), ConnectionMenu);
            });
            notice = L10n.T("In Home Assistant: Profil → Sicherheit → langlebigen Zugriffstoken erstellen. Hier einfügen. Anmeldung wird auf der Quest verschlüsselt gespeichert.");
        }

        void QrMenu()
        {
            NewMenu(L10n.T("Zugang per QR-Code"));
            qrMenu = true;
            Button(0, L10n.T("Scan starten / erneut versuchen"), () => qr.Begin((url, token) =>
            {
                if (!string.IsNullOrEmpty(url)) draftUrl = url;
                draftToken = token;
                ConnectionMenu();
                notice = L10n.T("QR-Code gelesen. Adresse prüfen und „Verbindung prüfen & speichern“ wählen. HTTP bleibt eine separate Freigabe.");
            }));
            Button(1, L10n.T("Zurück"), ConnectionMenu);
            notice = L10n.T("Token-QR oder JSON mit baseUrl und accessToken. Die Verbindung erfolgt erst nach deiner Bestätigung.");
            status.Label.text = notice;
        }

        void OpenKeyboard(bool token)
        {
            NewMenu(token ? L10n.T("Home-Assistant-Zugangstoken") : L10n.T("Adresse deines Home Assistant"));
            SystemTextInput.Open(menu.transform, token ? draftToken : draftUrl, token,
                value => { if (token) draftToken = value; else draftUrl = value; ConnectionMenu(); }, ConnectionMenu);
            notice = token ? L10n.T("Der Token bleibt verdeckt. „Einfügen“ nutzt die Zwischenablage der Quest.") : L10n.T("Zum Beispiel https://ha.example.com oder http://192.168.1.10:8123");
        }

        void MapMenu()
        {
            NewMenu(L10n.T("Erfasse deine Wohnung"));
            Button(0, L10n.T("Vorhandene Räume laden / ersten Scan starten"), () => Scan(false), "map");
            Button(1, L10n.T("Durch die Wohnung gehen & neu erfassen"), () => Scan(true), "map");
            Button(2, L10n.T("Wohnungskarte ansehen"), ShowMap, "map");
            Button(3, L10n.T("Entität im Raum platzieren"), () => { page = 0; EntityMenu(); }, "lamp");
            Button(4, L10n.T("Zurück"), Home);
            notice = L10n.T("Die Quest-Raumerfassung führt dich durch den Scan. Danach zeigt die Karte geladene Räume, Geräte und deinen aktuellen Standort.");
        }

        void ShowMap()
        {
            if (MRUK.Instance.Rooms.Count == 0) { notice = L10n.T("Zuerst Räume laden oder die Wohnung erfassen."); return; }
            NewMenu(L10n.T("Deine Wohnung · ") + MRUK.Instance.Rooms.Count + L10n.T(" Raum/Räume"));
            var card = WorldCard.Create(menu.transform, new Vector3(0, 0.055f, 0), new Vector2(740, 420), L10n.T("Grün: Räume · Gold: Geräte · Weiß: Du"));
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
            Button(5, L10n.T("Zurück"), MapMenu);
            notice = L10n.T("Die blaue Spur zeigt deinen Weg in dieser Sitzung. Weitere Räume über die Quest-Raumerfassung hinzufügen.");
        }

        void EntityMenu()
        {
            var entities = browser.Apply(ha.Entities.Values).ToArray();
            NewMenu(entities.Length + L10n.T(" Entitäten"));
            var filters=WorldCard.Create(menu.transform, new Vector3(-0.2f, 0.29f, 0), new Vector2(390, 65), L10n.T("Suchen & Filtern"), Filters);
            filters.SetIcon("search");filters.SetSurface(WorldCard.Surface.Primary);
            WorldCard.Create(menu.transform, new Vector3(0.2f, 0.29f, 0), new Vector2(390, 65), L10n.T("Zurück"), Home);
            entityList = menu.AddComponent<EntityScrollList>();
            entityList.Initialize(entities, ha, entity => { placementReadOnly = !EntityPolicy.CanSwitch(entity.Id, entity.State) && !entity.CanVolume; Details(entity); });
            notice = entities.Length == 0 ? L10n.T("Keine Treffer. Suche oder Filter ändern.") : L10n.T("Zielen, pinchen und ziehen · oder mit dem Finger wischen\n") + FilterSummary();
        }

        string FilterSummary() => Words("Art: ","Type: ")+(browser.Domain.Length==0?L10n.T("Alle"):EntityBrowser.DomainName(browser.Domain))+" · "+L10n.T("Raum: ") + AreaLabel() + L10n.T(" · Suche: ") + (browser.Search.Length == 0 ? "–" : browser.Search);
        string AreaLabel() => browser.Area.Length == 0 ? L10n.T("Alle") : browser.Area == "__none" ? L10n.T("Ohne Raum") :
            (ha.AreaNames.TryGetValue(browser.Area,out var name)?name:ha.Entities.Values.FirstOrDefault(e => e.AreaId == browser.Area)?.AreaName ?? browser.Area);
        void Pager(int row, Action previous, Action next)
        {
            WorldCard.Create(menu.transform, new Vector3(-0.19f, 0.275f - row * 0.1f, 0), new Vector2(350, 86), L10n.T("‹ Zurück"), previous);
            WorldCard.Create(menu.transform, new Vector3(0.19f, 0.275f - row * 0.1f, 0), new Vector2(350, 86), L10n.T("Weiter ›"), next);
        }
        void Filters()
        {
            NewMenu(L10n.T("Entitäten finden"));
            Button(0, L10n.T("Art: ") + (browser.Domain.Length == 0 ? L10n.T("Alle") : EntityBrowser.DomainName(browser.Domain)), () => TypeMenu(0));
            Button(1, L10n.T("Raum: ") + AreaLabel(), () => AreaMenu(0));
            Button(2, L10n.T("Suchen: ") + (browser.Search.Length == 0 ? L10n.T("Name oder Entitäts-ID") : Short(browser.Search, 35)), () =>
            {
                NewMenu(L10n.T("Entitäten suchen"));
                SystemTextInput.Open(menu.transform, browser.Search, false, value => { browser.Search = value.Trim(); page = 0; EntityMenu(); }, Filters);
            });
            Button(3, L10n.T("Alle Filter zurücksetzen"), () => { browser.Domain = browser.Area = browser.Search = ""; page = 0; EntityMenu(); });
            Button(4, L10n.T("Raumzuordnungen neu laden"), () => { ha.RefreshAreas(); notice = ha.AreaStatus; });
            Button(5, L10n.T("Ergebnisse anzeigen"), () => { page = 0; EntityMenu(); });
            notice = ha.AreaStatus;
        }
        void TypeMenu(int index)
        {
            var types = ha.Entities.Values.Select(e => EntityPolicy.Domain(e.Id)).Distinct().OrderBy(EntityBrowser.DomainName).ToArray();
            int pages = Math.Max(1, (types.Length + 2) / 3);
            index = Math.Max(0, Math.Min(index, pages - 1));
            NewMenu(L10n.T("Entitätsarten · ") + (index + 1) + "/" + pages);
            Button(0, L10n.T("Alle Arten"), () => { browser.Domain = ""; Filters(); });
            for (int i = 0; i < 3 && index * 3 + i < types.Length; i++)
            {
                string type = types[index * 3 + i];
                Button(i + 1, EntityBrowser.DomainName(type) + " · " + ha.Entities.Values.Count(e => EntityPolicy.Domain(e.Id) == type), () => { browser.Domain = type; Filters(); });
            }
            Pager(4, () => TypeMenu((index + pages - 1) % pages), () => TypeMenu((index + 1) % pages));
            Button(5, L10n.T("Zurück"), Filters);
        }
        void AreaMenu(int index)
        {
            bool opening=areaMenuPage<0;
            var areas = ha.AreaNames.Select(p=>new HaEntity{AreaId=p.Key,AreaName=p.Value}).Concat(ha.Entities.Values.Where(e=>!string.IsNullOrEmpty(e.AreaId))).GroupBy(e=>e.AreaId).Select(g=>g.First()).OrderBy(e=>e.AreaName).ToArray();
            int pages = Math.Max(1, (areas.Length + 1) / 2);
            index = Math.Max(0, Math.Min(index, pages - 1));
            NewMenu(L10n.T("Räume · ") + (index + 1) + "/" + pages);
            areaMenuPage=index;displayedAreaRevision=ha.AreaRevision;
            if(opening&&areas.Length==0&&!ha.AreasLoading)ha.RefreshAreas();
            Button(0, L10n.T("Alle Räume"), () => { browser.Area = ""; Filters(); });
            Button(1, L10n.T("Ohne Raumzuordnung"), () => { browser.Area = "__none"; Filters(); });
            for (int i = 0; i < 2 && index * 2 + i < areas.Length; i++)
            {
                var area = areas[index * 2 + i];
                Button(i + 2, area.AreaName, () => { browser.Area = area.AreaId; Filters(); });
            }
            Pager(4, () => AreaMenu((index + pages - 1) % pages), () => AreaMenu((index + 1) % pages));
            Button(5, L10n.T("Zurück"), Filters);
            notice = ha.AreaStatus;
        }

        void Details(HaEntity entity)
        {
            NewMenu(Short(entity.Name, 45) + "\n" + entity.Id);
            if (bindings.Contains(entity.Id) || markers.ContainsKey(entity.Id))
            {
                Button(0, L10n.T("Zuordnung entfernen"), async () =>
                {
                    if (operationBusy) return;
                    operationBusy = true;
                    try { if (Application.isEditor) RemoveMarker(entity.Id); else await bindings.Remove(entity.Id); }
                    finally { operationBusy = false; gate.Reset(); }
                    Details(entity);
                });
                Button(1, L10n.T("Zurück"), EntityMenu);
                if (EntityAllowedHere(entity.Id)&&!bindings.DisplayOnly(entity.Id) && (entity.CanDim || entity.CanVolume)) Button(2, L10n.T("Helligkeit / Lautstärke ändern"), () => OpenLevelControls(entity), entity.CanDim ? "sun" : "speaker");
                return;
            }
            Button(0, L10n.T("Zeigen & pinchen · ") + freeDistance.ToString("0.0") + L10n.T(" Meter"), () => BeginPlacement(entity.Id, freeDistance), IconFor(entity.Id));
            Button(1, L10n.T("Direkt an der Hand platzieren · einmal pinchen"), () => BeginHandPlacement(entity.Id));
            Button(2, L10n.T("Auf gescannter Fläche platzieren"), () => BeginPlacement(entity.Id, 0), "map");
            if (EntityPolicy.Domain(entity.Id) == "light" || EntityPolicy.Domain(entity.Id) == "switch")
                Button(3, placementReadOnly ? L10n.T("Modus: nur anzeigen") : L10n.T("Modus: anzeigen & schalten"), () => { placementReadOnly = !placementReadOnly; Details(entity); });
            Button(4, L10n.T("Zurück"), EntityMenu);
            Button(5, L10n.T("Abstand ändern · ") + freeDistance.ToString("0.0") + L10n.T(" Meter"), () => { freeDistance = freeDistance >= 2 ? 0.5f : freeDistance + 0.5f; Details(entity); });
        }

        void BeginPlacement(string id, float distance)
        {
            if (!bindings.Ready || bindings.Busy) { notice = L10n.T("Positionen werden noch vorbereitet. ") + bindings.Status; return; }
            if (distance == 0 && MRUK.Instance.GetCurrentRoom() == null)
            { notice = L10n.T("Zuerst im Hauptmenü den Raum laden/scannen."); return; }
            CloseMenu();
            placing = id;
            placementFrozen = false;
            handPlacement = false;
            placementDistance = distance;
            preview = WorldCard.Create(null, Vector3.zero, new Vector2(420, 110), "");
            preview.GetComponent<Collider>().enabled = false;
            preview.SetIcon(IconFor(id));
            preview.MakeCompactMarker();
            preview.SetNearby(true);
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
            if (HandReady(ControlHand) && ControlSkeleton && ControlSkeleton.Bones != null)
            {
                Transform index = MagicCasting.Bone(ControlSkeleton, OVRSkeleton.BoneId.Hand_IndexTip, OVRSkeleton.BoneId.XRHand_IndexTip);
                Transform thumb = MagicCasting.Bone(ControlSkeleton, OVRSkeleton.BoneId.Hand_ThumbTip, OVRSkeleton.BoneId.XRHand_ThumbTip);
                if (index && thumb) { point = (index.position + thumb.position) * 0.5f; return true; }
            }
            if (OVRInput.IsControllerConnected(ControlController) && OVRInput.GetControllerPositionTracked(ControlController))
            { point = ControllerAnchor.position + ControllerAnchor.forward * 0.12f; return true; }
            if (HandReady(ControlHand) && ControlHand.PointerPose) { point = ControlHand.PointerPose.position; return true; }
            if (Application.isEditor) { point = head.position + head.forward * 0.6f; return true; }
            return false;
        }

        void FreezePlacement(Vector3 position)
        {
            heldPosition = position;
            placementFrozen = true;
            preview.transform.position = position;
            ConfirmHandPlacement();
        }

        void ConfirmHandPlacement()
        {
            NewMenu(L10n.T("Hier bleibt deine Entität"));
            Button(0, L10n.T("Position speichern"), () => Place(heldPosition), IconFor(placing));
            Button(1, L10n.T("Weiter verschieben"), () => { CloseMenu(); placementFrozen = false; placement.Reset(); });
            Button(2, L10n.T("Abbrechen"), () => { CancelPlacement(); Home(); });
            preview.Pulse();
            preview.Label.text = L10n.T("Vorschau · Position bestätigen");
        }

        async void Place(Vector3 position)
        {
            if (operationBusy || string.IsNullOrEmpty(placing)) return;
            if(!PointInCurrentRoom(position)){notice=Words("Die Position liegt nicht in deinem aktuellen Raum.","The position is outside your current room.");return;}
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
                    resultNotice = L10n.T("Editor-Vorschau · Position wird erst auf der Quest dauerhaft gespeichert.");
                }
                else await bindings.Place(id, position, displayOnly, Quaternion.LookRotation(position-head.position, Vector3.up));
            }
            finally { operationBusy = false; gate.Reset(); }
            Home();
            if (resultNotice != null) notice = resultNotice;
        }

        void CancelPlacement()
        {
            placing = null;
            handPlacement = false;
            placementFrozen = false;
            if (preview) Destroy(preview.gameObject);
            gate.Reset();
        }

        void AddMarker(string id, Transform anchor)
        {
            RemoveMarker(id);
            bool media = EntityPolicy.Domain(id) == "camera" || EntityPolicy.Domain(id) == "image";
            var card = WorldCard.Create(anchor, Vector3.zero, media ? new Vector2(600, 400) : new Vector2(340, 115), id, () =>
            {
                if(!EntityAllowedHere(id))return;
                if (ha.Entities.TryGetValue(id, out var entity) && entity.CanVolume) OpenLevelControls(entity);
                else ha.Switch(id);
            });
            card.EntityId = id;
            card.IsMarker = true;
            card.ReadOnly = bindings.DisplayOnly(id);
            if (media) { card.IsMedia = true; card.gameObject.AddComponent<MediaFrame>().Initialize(ha,id); }
            else { card.SetIcon(IconFor(id)); card.MakeCompactMarker(); }
            if(ha.Entities.TryGetValue(id,out var adjustable))EnsureAdjustment(card,adjustable);
            card.gameObject.SetActive(Application.isEditor);
            markers[id] = card;
        }
        void EnsureAdjustment(WorldCard card,HaEntity entity)
        {
            if(card.Adjustment||card.ReadOnly||(!entity.CanDim&&!entity.CanVolume))return;
            var control=WorldCard.Create(card.transform,new Vector3(0,-76,0),new Vector2(85,46),"",()=>
            {if(ha.Fresh&&ha.Entities.TryGetValue(entity.Id,out var current))OpenLevelControls(current);});
            control.UnitScale=1;control.SetIcon(entity.CanDim?"sun":"speaker");card.Adjustment=control;control.gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            // Run after SDK anchor updates and the tracking stability check, before rendering.
            if (Application.isEditor || !bindings) return;
            foreach (var pair in markers)
                if (pair.Value) pair.Value.gameObject.SetActive(MarkerVisibleHere(pair.Key,pair.Value));
        }

        void RemoveMarker(string id)
        {
            if (markers.TryGetValue(id, out var card)) { Destroy(card.gameObject); markers.Remove(id); }
        }

        async void LoadBindings()
        {
            if (operationBusy || bindings.Busy) return;
            await bindings.Load();
            gate.Reset();
        }

        async void Scan(bool rescan)
        {
            if (scanBusy || operationBusy) return;
            if (Application.isEditor) { notice = L10n.T("Raumscan ist auf der Quest verfügbar. Im Editor frei platzieren."); return; }
            scanBusy = true;
            bindings.InvalidateTracking();
            CancelPlacement();
            notice = L10n.T("Raumerfassung wird geöffnet …");
            try
            {
                if (!await WithTimeout(ScenePermission(), 30)) { notice = L10n.T("Raumzugriff abgelehnt. In den Quest-App-Berechtigungen erlauben."); return; }
                if (rescan && !await WithTimeout(RequestScan(), 300)) { notice = L10n.T("Raumscan abgebrochen oder nicht verfügbar."); return; }
                var result = await WithTimeout(MRUK.Instance.LoadSceneFromDevice(!rescan), 60);
                notice = result == MRUK.LoadDeviceResult.Success ? L10n.T("Raum geladen. Jetzt Entitäten zuordnen.") : L10n.T("Kein Raum geladen. Raumscan und Berechtigung prüfen.");
            }
            catch { notice = L10n.T("Raumerfassung fehlgeschlagen. Bitte auf der Quest erneut versuchen."); }
            finally { scanBusy = false; RestoreHandInput(); }
        }

        static async Task<bool> RequestScan() => await OVRScene.RequestSpaceSetup();
        static async Task<T> WithTimeout<T>(Task<T> task, int seconds)
        {
            if (await Task.WhenAny(task, Task.Delay(seconds * 1000)) != task) throw new TimeoutException();
            return await task;
        }

        void RecenterMenu()
        {
            if (!menu || !head) return;
            Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;
            menu.transform.SetPositionAndRotation(head.position + forward * 1.4f, Quaternion.LookRotation(forward));
        }

        void PlacedMenu(int index)
        {
            var ids = bindings.EntityIds.Concat(markers.Keys).Distinct().OrderBy(id => id).ToArray();
            int pages = Math.Max(1, (ids.Length + 2) / 3);
            index = Math.Max(0, Math.Min(index, pages - 1));
            NewMenu(L10n.T("Platzierte Entitäten · ") + (index + 1) + "/" + pages);
            for (int i = 0; i < 3 && index * 3 + i < ids.Length; i++)
            {
                string id = ids[index * 3 + i];
                string name = ha.Entities.TryGetValue(id, out var entity) ? entity.Name : id;
                Button(i, L10n.T("Löschen · ") + Short(name, 38), () =>
                {
                    NewMenu(L10n.T("Platzierung löschen?\n") + Short(name, 40));
                    Button(0, L10n.T("Ja · Platzierung entfernen"), async () =>
                    {
                        if (operationBusy || bindings.Busy) { notice = L10n.T("Positionen werden gerade verarbeitet. Bitte kurz warten."); return; }
                        operationBusy = true;
                        try { if (Application.isEditor) RemoveMarker(id); else await bindings.Remove(id); }
                        finally { operationBusy = false; gate.Reset(); }
                        PlacedMenu(index);
                    });
                    Button(1, L10n.T("Abbrechen"), () => PlacedMenu(index));
                }, IconFor(id));
            }
            Pager(4, () => PlacedMenu((index + pages - 1) % pages), () => PlacedMenu((index + 1) % pages));
            Button(5, L10n.T("Zurück"), Home);
            notice = ids.Length == 0 ? L10n.T("Noch keine Entitäten platziert.") : L10n.T("Entfernt nur die Platzierung in der App. Das Gerät bleibt in Home Assistant erhalten.");
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
        string IconFor(string id) => IconLibrary.ForEntity(ha != null && ha.Entities.TryGetValue(id ?? "", out var value) ? value : new HaEntity { Id = id });
        void OnDestroy()
        {
            OVRManager.InputFocusAcquired -= RestoreHandInput;
            if (subscribedDisplay != null) subscribedDisplay.RecenteredPose -= RestoreHandInput;
            magic?.Cancel(); if (pointer) { Destroy(pointer.material); Destroy(pointer.gameObject); }
            if (beamGlow) { Destroy(beamGlow.material); Destroy(beamGlow.gameObject); }
            if (beamTexture) Destroy(beamTexture);
            if (wristWatch) Destroy(wristWatch.gameObject);
            if (magicHint) Destroy(magicHint.gameObject);
            if (readinessGlow) Destroy(readinessGlow.gameObject);
            if (gestureHint) Destroy(gestureHint.gameObject);
        }
        void OnApplicationFocus(bool focus)
        {
            ResetGaze();
            magic?.Cancel();
            gate.Reset();
            menuGate.Reset();
            if (focus) RestoreHandInput();
            if (!focus && placement.Phase != PlacementPhase.Confirm) placement.Reset();
        }
        void OnApplicationPause(bool pause)
        {
            ResetGaze();
            magic?.Cancel();
            gate.Reset();
            menuGate.Reset();
            if (!pause) RestoreHandInput();
            if (pause && placement.Phase != PlacementPhase.Confirm) placement.Reset();
        }
    }
}


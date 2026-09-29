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
            bool priorLanguage=ExperienceSettings.English;
            ExperienceSettings.English=true;
            bool englishWorks=L10n.T("Zurück")=="Back" && new HaEntity { State="off" }.DisplayValue=="Off";
            ExperienceSettings.English=priorLanguage;
            if(!Check(englishWorks,"English UI and HA state labels use the persisted language preference"))yield break;
            var inputHost=new GameObject("ISDK input fixture");var input=inputHost.AddComponent<MetaInputBridge>();
            var sdkTarget=WorldCard.Create(null,new Vector3(0,0,2),new Vector2(300,100),"SDK target",()=>{});
            yield return null;yield return null;Physics.SyncTransforms();
            var testRay=new Ray(Vector3.zero,Vector3.forward);
            input.Step(testRay,false,true,false,Vector3.zero);yield return null;
            input.Step(testRay,true,true,false,Vector3.zero);
            if(!Check(input.Hovered==sdkTarget&&input.RaySelected==sdkTarget,"Meta SDK ray hover and fresh pinch select the visible card"))yield break;
            input.Step(testRay,true,true,false,Vector3.zero);
            if(!Check(input.RaySelected==null,"Holding a pinch does not repeat a Meta SDK activation"))yield break;
            input.Step(testRay,true,false,false,Vector3.zero);
            input.Step(testRay,true,true,false,Vector3.zero);
            if(!Check(input.RaySelected==null,"Tracking recovery while held cannot select another card"))yield break;
            input.Step(testRay,false,true,true,new Vector3(0,0,1.8f));yield return null;
            input.Step(testRay,false,true,true,new Vector3(0,0,1.95f));yield return null;
            input.Step(testRay,false,true,true,new Vector3(0,0,2.001f));
            if(!Check(input.PokeSelected==sdkTarget,"Meta SDK poke recognizes index fingertip crossing the front surface"))yield break;
            Destroy(inputHost);Destroy(sdkTarget.gameObject);yield return null;
            var trackingRoot = new GameObject("Shifted tracking origin").transform;
            trackingRoot.SetPositionAndRotation(new Vector3(3,1,-2), Quaternion.Euler(0,70,0));
            var pointerHand = new GameObject("Pointer calibration fixture").AddComponent<OVRHand>(); pointerHand.enabled = false;
            pointerHand.PointerPose.localPosition = new Vector3(0.2f,-0.1f,0.5f);
            pointerHand.PointerPose.localRotation = Quaternion.identity;
            var expectedOrigin = trackingRoot.TransformPoint(pointerHand.PointerPose.localPosition);
            if (!Check(Vector3.Distance(pointerHand.PointerPose.position, expectedOrigin) > 1, "Unbound SDK pointer reproduces offset after tracking-space movement")) yield break;
            QuestHome.BindPointerToTrackingSpace(pointerHand, trackingRoot);
            if (!Check(Vector3.Distance(pointerHand.PointerPose.position, expectedOrigin) < 0.001f && Vector3.Dot(pointerHand.PointerPose.forward, trackingRoot.forward) > 0.999f,
                "Pointer pose converts tracking-local position and direction into world space")) yield break;
            var pointerTarget = WorldCard.Create(null, pointerHand.PointerPose.position + pointerHand.PointerPose.forward*1.4f, new Vector2(400,100), "Calibration target", () => {});
            pointerTarget.transform.rotation = pointerHand.PointerPose.rotation;
            trackingRoot.SetPositionAndRotation(new Vector3(-2,0.5f,1), Quaternion.Euler(0,-35,0));
            pointerHand.PointerPose.localRotation = Quaternion.LookRotation(trackingRoot.InverseTransformPoint(pointerTarget.transform.position)-pointerHand.PointerPose.localPosition);
            QuestHome.BindPointerToTrackingSpace(pointerHand, trackingRoot);
            Physics.SyncTransforms();
            if (!Check(Physics.Raycast(new Ray(pointerHand.PointerPose.position,pointerHand.PointerPose.forward), out var pointerHit, 10) && pointerHit.collider.GetComponent<WorldCard>() == pointerTarget,
                "Right-hand ray still hits world menu after translation and rotation during recenter")) yield break;
            Destroy(pointerTarget.gameObject); Destroy(pointerHand.gameObject); Destroy(trackingRoot.gameObject);
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
            marker.SetIcon("temperature");marker.SetIcon("temperature");
            if(!Check(marker.GetComponentsInChildren<UnityEngine.UI.Image>().Single(i=>i.name=="Icon").rectTransform.anchoredPosition==Vector2.zero,"Refreshing a marker icon preserves its centered layout"))yield break;
            marker.ShowSensorValue(true,1);float nearText=marker.Label.rectTransform.lossyScale.x;
            marker.ShowSensorValue(true,4);float farText=marker.Label.rectTransform.lossyScale.x;
            if(!Check(Mathf.Abs(farText/nearText-4)<.01f&&marker.Label.fontSize>=28,"Sensor label keeps constant angular size at one and four meters"))yield break;
            marker.ShowSensorValue(true,100);
            if(!Check(marker.Label.rectTransform.lossyScale.x<=.0093f,"Angular label scaling has a far-distance limit"))yield break;
            Destroy(marker.gameObject);
            var handFixture = new GameObject("Synthetic right-hand geometry");
            var skeleton = handFixture.AddComponent<OVRSkeleton>(); skeleton.enabled = false;
            var bones = new System.Collections.Generic.List<OVRBone>();
            Transform AddBone(OVRSkeleton.BoneId id, Vector3 position)
            {
                var bone = new GameObject(id.ToString()).transform; bone.SetParent(handFixture.transform);
                bone.position = position; bones.Add(new OVRBone(id, 0, bone)); return bone;
            }
            AddBone(OVRSkeleton.BoneId.Hand_WristRoot, new Vector3(0,0,0.5f));
            var roots = new[] { OVRSkeleton.BoneId.Hand_Index1, OVRSkeleton.BoneId.Hand_Middle1, OVRSkeleton.BoneId.Hand_Ring1, OVRSkeleton.BoneId.Hand_Pinky1 };
            var joints = new[] { OVRSkeleton.BoneId.Hand_Index2, OVRSkeleton.BoneId.Hand_Middle2, OVRSkeleton.BoneId.Hand_Ring2, OVRSkeleton.BoneId.Hand_Pinky2 };
            var tips = new[] { OVRSkeleton.BoneId.Hand_IndexTip, OVRSkeleton.BoneId.Hand_MiddleTip, OVRSkeleton.BoneId.Hand_RingTip, OVRSkeleton.BoneId.Hand_PinkyTip };
            Transform indexTip = null;
            for (int i=0;i<4;i++)
            {
                float x = -0.03f+i*0.02f;
                AddBone(roots[i], new Vector3(x,0.075f,0.5f)); AddBone(joints[i], new Vector3(x,0.10f,0.5f));
                var tip = AddBone(tips[i], new Vector3(x,0.14f,0.5f)); if (i == 0) indexTip = tip;
            }
            typeof(OVRSkeleton).GetProperty("Bones").SetValue(skeleton, bones);
            typeof(OVRSkeleton).GetProperty("IsDataValid").SetValue(skeleton, true);
            if (!Check(MagicCasting.OpenPalm(skeleton, out _, out var palmNormal) && Vector3.Dot(palmNormal, Vector3.forward) > 0.99f, "Extended right-hand bones produce forward-facing palm normal")) yield break;
            indexTip.position = new Vector3(-0.03f,0.07f,0.46f);
            if (!Check(!MagicCasting.OpenPalm(skeleton, out _, out _), "Curled index finger rejects open-palm spell")) yield break;
            indexTip.position = new Vector3(-0.03f,0.14f,0.5f);
            var xrRoots = new[] { OVRSkeleton.BoneId.XRHand_IndexProximal, OVRSkeleton.BoneId.XRHand_MiddleProximal, OVRSkeleton.BoneId.XRHand_RingProximal, OVRSkeleton.BoneId.XRHand_LittleProximal };
            var xrJoints = new[] { OVRSkeleton.BoneId.XRHand_IndexIntermediate, OVRSkeleton.BoneId.XRHand_MiddleIntermediate, OVRSkeleton.BoneId.XRHand_RingIntermediate, OVRSkeleton.BoneId.XRHand_LittleIntermediate };
            var xrTips = new[] { OVRSkeleton.BoneId.XRHand_IndexTip, OVRSkeleton.BoneId.XRHand_MiddleTip, OVRSkeleton.BoneId.XRHand_RingTip, OVRSkeleton.BoneId.XRHand_LittleTip };
            var xrBones = new System.Collections.Generic.List<OVRBone> { new OVRBone(OVRSkeleton.BoneId.XRHand_Wrist, 0, bones[0].Transform) };
            for (int i=0;i<4;i++)
            {
                xrBones.Add(new OVRBone(xrRoots[i],0,bones[1+i*3].Transform));
                xrBones.Add(new OVRBone(xrJoints[i],0,bones[2+i*3].Transform));
                xrBones.Add(new OVRBone(xrTips[i],0,bones[3+i*3].Transform));
            }
            typeof(OVRSkeleton).GetProperty("Bones").SetValue(skeleton, xrBones);
            typeof(OVRSkeleton).GetField("_skeletonType", System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(skeleton, OVRSkeleton.SkeletonType.XRHandRight);
            if (!Check(MagicCasting.OpenPalm(skeleton, out _, out palmNormal) && palmNormal.z > 0.99f &&
                MagicCasting.Bone(skeleton, OVRSkeleton.BoneId.Hand_IndexTip, OVRSkeleton.BoneId.XRHand_IndexTip) == indexTip,
                "OpenXR bone identifiers stay distinct from overlapping OVR identifiers")) yield break;
            if(!Check(!HandPose.IsFist(skeleton)&&HandPose.PinchIntent(skeleton),"Open hand is valid pinch intent but not a fist"))yield break;
            for(int i=0;i<3;i++)bones[3+i*3].Transform.position=new Vector3(-.03f+i*.02f,.065f,.47f);
            if(!Check(HandPose.IsFist(skeleton)&&!HandPose.PinchIntent(skeleton),"Curled tracked fingers distinguish a fist from selection"))yield break;
            var rollHead=new GameObject("Roll reference").transform;
            float rollBefore=HandPose.Roll(skeleton,rollHead);handFixture.transform.Rotate(Vector3.forward,30);
            float rollAfter=HandPose.Roll(skeleton,rollHead);
            if(!Check(Mathf.Abs(Mathf.Abs(Mathf.DeltaAngle(rollBefore,rollAfter))-30)<.1f,"Fist roll follows actual hand-bone rotation"))yield break;
            Destroy(rollHead.gameObject);Destroy(handFixture);
            var magicPreference = new MagicCasting(); bool oldMagic = magicPreference.Enabled;
            magicPreference.SetEnabled(!oldMagic);
            bool rememberedMagic = new MagicCasting().Enabled;
            magicPreference.SetEnabled(oldMagic);
            if (!Check(rememberedMagic == !oldMagic, "Magic toggle survives settings reload")) yield break;
            var fireTarget = WorldCard.Create(null, new Vector3(0, 0, 1), new Vector2(90,90), "Magic target");
            fireTarget.MakeCompactMarker();
            var fireHead = new GameObject("Magic test head").transform;
            var fireball = MagicFireball.Launch(Vector3.zero, fireTarget, fireHead);
            var secondFireball = MagicFireball.Launch(new Vector3(0.1f,0,0), fireTarget, fireHead);
            if (!Check(fireball && secondFireball && fireball != secondFireball, "Fireballs can coexist with independent flight effects")) yield break;
            if (!Check(fireball && fireball.GetComponentsInChildren<SpriteRenderer>().Length == 16, "Generated emerald texture loads into actual fireball and trail")) yield break;
            yield return new WaitForSeconds(0.12f);
            var lead = fireball.GetComponentsInChildren<SpriteRenderer>()[0];
            if (!Check(lead.transform.position.z > 0 && lead.transform.position.z < 1 && lead.color.a > 0, "Fireball visibly advances from hand origin toward entity")) yield break;
            yield return new WaitForSeconds(0.23f);
            if (!Check(fireball && fireball.GetComponentInChildren<LineRenderer>().enabled, "Arrival creates expanding impact ring")) yield break;
            yield return new WaitForSeconds(0.5f);
            if (!Check(!fireball, "Effect releases its objects after impact")) yield break;
            Destroy(fireTarget.gameObject); Destroy(fireHead.gameObject);
            var bindingHost = new GameObject("Isolated placement persistence test");
            var spatial = bindingHost.AddComponent<SpatialBindings>();
            string fixtureServer="test-unlocalized-delete-"+Guid.NewGuid();
            spatial.Initialize(fixtureServer);
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            var roomFile=(BindingFile)typeof(SpatialBindings).GetField("data",flags).GetValue(spatial);
            string roomA=Guid.NewGuid().ToString(),roomB=Guid.NewGuid().ToString();
            roomFile.bindings.Add(new BindingRecord { entityId="light.room_a",anchorUuid=roomA });
            roomFile.bindings.Add(new BindingRecord { entityId="sensor.room_b",anchorUuid=roomB });
            typeof(SpatialBindings).GetMethod("Persist",flags).Invoke(spatial,null);
            var reloaded=new GameObject("Multi-room reload").AddComponent<SpatialBindings>();
            reloaded.Initialize(fixtureServer);
            var records=(BindingFile)typeof(SpatialBindings).GetField("data",flags).GetValue(reloaded);
            if(!Check(records.bindings.Count==2&&records.bindings[0].anchorUuid==roomA&&records.bindings[1].anchorUuid==roomB,
                "Independent UUIDs for two rooms survive persistence without sharing an origin"))yield break;
            Destroy(reloaded.gameObject);roomFile.bindings.Clear();
            if (!Check(!spatial.CanShow("light.unbound"), "Missing anchor never becomes a visible fallback at startup")) yield break;
            var anchorFixture = new GameObject("Tracked anchor visibility fixture").AddComponent<OVRSpatialAnchor>();
            anchorFixture.enabled = false;
            spatial.Loaded["light.visibility"] = anchorFixture;
            var visible = (System.Collections.Generic.HashSet<string>)typeof(SpatialBindings).GetField("trackedIds", flags).GetValue(spatial);
            visible.Add("light.visibility");
            typeof(SpatialBindings).GetProperty("TrackingReady").SetValue(spatial, true);
            if (!Check(!spatial.CanShow("light.visibility"), "Untracked anchor stays hidden even with cached stable state")) yield break;
            typeof(OVRSpatialAnchor).GetProperty("IsTracked").SetValue(anchorFixture, true);
            if (!Check(spatial.CanShow("light.visibility"), "Current tracked anchor and stable session permit display")) yield break;
            spatial.InvalidateTracking();
            if (!Check(!spatial.CanShow("light.visibility") && !spatial.TrackingReady, "Recenter immediately invalidates cached anchor visibility")) yield break;
            spatial.Loaded.Remove("light.visibility"); Destroy(anchorFixture.gameObject);
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
            var leftFixture=new GameObject("Left preference hand").AddComponent<OVRHand>();leftFixture.enabled=false;
            var rightFixture=new GameObject("Right preference hand").AddComponent<OVRHand>();rightFixture.enabled=false;
            ui.LeftHand=leftFixture;ui.RightHand=rightFixture;
            bool priorLeft=ExperienceSettings.LeftHanded;
            ExperienceSettings.LeftHanded=false;
            bool rightDefault=typeof(QuestHome).GetProperty("ControlHand",flags).GetValue(ui)==rightFixture&&typeof(QuestHome).GetProperty("MenuHand",flags).GetValue(ui)==leftFixture;
            ExperienceSettings.LeftHanded=true;
            bool leftMirrored=typeof(QuestHome).GetProperty("ControlHand",flags).GetValue(ui)==leftFixture&&typeof(QuestHome).GetProperty("MenuHand",flags).GetValue(ui)==rightFixture;
            ExperienceSettings.LeftHanded=priorLeft;ui.LeftHand=ui.RightHand=null;Destroy(leftFixture.gameObject);Destroy(rightFixture.gameObject);
            if(!Check(rightDefault&&leftMirrored,"Handedness mirrors control hand and watch hand together"))yield break;
            testHead.position=new Vector3(30,0,0);
            var gazeA=WorldCard.Create(null,new Vector3(30,0,3),new Vector2(90,90),"gaze A");gazeA.MakeCompactMarker();gazeA.EntityId="sensor.a";
            var gazeB=WorldCard.Create(null,new Vector3(30.07f,0,3),new Vector2(90,90),"gaze B");gazeB.MakeCompactMarker();gazeB.EntityId="sensor.b";
            var gazeMarkers=(System.Collections.Generic.Dictionary<string,WorldCard>)Get("markers");gazeMarkers.Add(gazeA.EntityId,gazeA);gazeMarkers.Add(gazeB.EntityId,gazeB);
            yield return null;Physics.SyncTransforms();
            if(!Check((WorldCard)typeof(QuestHome).GetMethod("GazeTarget",flags).Invoke(ui,null)==gazeA,"Head-center targeting selects the centered entity independently of hand aim"))yield break;
            gazeA.gameObject.SetActive(false);
            if(!Check((WorldCard)typeof(QuestHome).GetMethod("GazeTarget",flags).Invoke(ui,null)==gazeB,"Untracked hidden entity cannot remain the gaze target"))yield break;
            gazeB.transform.position=new Vector3(31,0,3);Physics.SyncTransforms();
            if(!Check(typeof(QuestHome).GetMethod("GazeTarget",flags).Invoke(ui,null)==null,"Looking away cancels gaze target"))yield break;
            gazeMarkers.Clear();Destroy(gazeA.gameObject);Destroy(gazeB.gameObject);testHead.position=Vector3.zero;
            Invoke("BeginPlacement", "light.cancel_on_recenter", 1f);
            Invoke("FreezePlacement", new Vector3(0.2f, 0, 1));
            Invoke("RestoreHandInput");
            if (!Check(Get("placing") == null && !((GameObject)Get("menu")).GetComponentsInChildren<WorldCard>().Any(c => c.Label.text.StartsWith("Position speichern")),
                "Recenter discards the frozen preview and its stale save action")) yield break;
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
            Invoke("ToggleMenu");
            if (!Check((GameObject)Get("menu") == null, "Second left-menu action closes the open submenu")) yield break;
            Invoke("ToggleMenu");
            if (!Check((GameObject)Get("menu") != null, "Next left-menu action opens the main menu again")) yield break;
            Invoke("CloseMenu"); Destroy(testHead.gameObject);
            Destroy(bindingHost);
            bool completed = false, accepted = false;
            ha.Connect("http://127.0.0.1:18123", "test-token", true, ok => { completed = true; accepted = ok; });
            yield return Wait(() => completed, 12, "Connection callback");
            if (!Check(accepted && ha.Fresh && !ha.Demo, "Authenticated HTTP connection")) yield break;
            if (!Check(!ha.Switch("sensor.temperature"), "Magic dispatch rejects read-only sensor commands")) yield break;
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
            if(!Check(ha.Entities["light.test"].CanDim&&!ha.Entities["light.simple"].CanDim&&ha.Entities["media_player.speaker"].CanVolume,"Only advertised dimming and volume capabilities enable controls"))yield break;
            if(!Check(!ha.SetLevel("sensor.temperature",.5f)&&!ha.SetLevel("light.simple",.5f)&&!ha.SetLevel("light.test",float.NaN),"Unsupported or invalid level commands are rejected"))yield break;
            if(!Check(ha.SetLevel("light.test",.4f),"Brightness gesture accepts valid value"))yield break;
            yield return Wait(()=>!ha.Busy&&ha.Fresh&&Mathf.Abs(ha.Entities["light.test"].Level-.4f)<.01f,12,"Brightness payload round trip");
            if(!Check(ha.SetLevel("media_player.speaker",.3f),"Volume gesture accepts valid value"))yield break;
            yield return Wait(()=>!ha.Busy&&ha.Fresh&&Mathf.Abs(ha.Entities["media_player.speaker"].Level-.3f)<.01f,12,"Volume payload round trip");
            var sliderHost=new GameObject("Slider gesture fixture");var sliderUi=sliderHost.AddComponent<QuestHome>();sliderUi.enabled=false;
            var sliderHead=new GameObject("Slider head").transform;
            typeof(QuestHome).GetField("head",flags).SetValue(sliderUi,sliderHead);typeof(QuestHome).GetField("ha",flags).SetValue(sliderUi,ha);
            typeof(QuestHome).GetMethod("OpenLevelControls",flags).Invoke(sliderUi,new object[]{ha.Entities["light.test"]});
            var sliderCard=(WorldCard)typeof(QuestHome).GetField("levelControl",flags).GetValue(sliderUi);
            typeof(QuestHome).GetField("focused",flags).SetValue(sliderUi,sliderCard);
            var sliderMenu=(GameObject)typeof(QuestHome).GetField("menu",flags).GetValue(sliderUi);
            var sliderRay=new Ray(Vector3.zero,sliderMenu.transform.TransformPoint(new Vector3(.15f,.06f,0)).normalized);
            var dragMethod=typeof(QuestHome).GetMethod("UpdateAlternativeInput",flags);
            dragMethod.Invoke(sliderUi,new object[]{true,true,true,sliderRay});
            if(!Check(!ha.Busy&&Mathf.Abs((float)typeof(QuestHome).GetField("draftLevel",flags).GetValue(sliderUi)-.75f)<.01f,"Pinch drag previews slider value without issuing intermediate commands"))yield break;
            dragMethod.Invoke(sliderUi,new object[]{true,true,false,sliderRay});
            yield return Wait(()=>!ha.Busy&&ha.Fresh&&Mathf.Abs(ha.Entities["light.test"].Level-.75f)<.01f,12,"Releasing a dragged slider sends the selected brightness");
            typeof(QuestHome).GetMethod("CloseMenu",flags).Invoke(sliderUi,null);Destroy(sliderHost);Destroy(sliderHead.gameObject);
            if(!Check(ha.Entities["media_player.speaker"].AreaId=="bedroom"&&ha.Entities["camera.front"].AreaId=="hall","Four HA rooms retain independent assignments after commands"))yield break;
            foreach(string mediaId in new[]{"camera.front","image.picture"})
            {
                using(var mediaRequest=ha.MediaRequest(mediaId))
                {
                    if(!Check(mediaRequest!=null&&mediaRequest.redirectLimit==0&&mediaRequest.GetRequestHeader("Authorization")=="Bearer test-token","Media request is authenticated and never redirects"))yield break;
                    yield return mediaRequest.SendWebRequest();
                    var decodedFrame=new Texture2D(2,2);
                    bool ok=mediaRequest.result==UnityWebRequest.Result.Success&&decodedFrame.LoadImage(mediaRequest.downloadHandler.data);
                    Destroy(decodedFrame);if(!Check(ok,"Camera/image proxy provides a decodable frame"))yield break;
                }
            }
            if(!Check(ha.MediaRequest("camera.front/../../other")==null&&ha.MediaRequest("sensor.temperature")==null,"Media cannot escape the selected entity endpoint"))yield break;
            var listHost=new GameObject("Scroll list fixture");
            var list=listHost.AddComponent<EntityScrollList>();
            var many=Enumerable.Range(0,30).Select(i=>new HaEntity{Id="sensor.row_"+i,Name="Row "+i,State=i.ToString(),Unit="W",AreaName=i%2==0?"Kitchen":"Office"}).ToArray();
            list.Initialize(many,ha,_=>{});yield return null;
            if(!Check(list.GetComponentsInChildren<WorldCard>().Count(c=>c.GetComponent<Collider>().enabled)==8,"Scrollable list exposes eight complete entity targets"))yield break;
            list.ScrollBy(99);yield return null;
            if(!Check(list.Count==30&&list.Offset>1&&list.GetComponentsInChildren<WorldCard>().Any(c=>c.Label.text.Contains("29 W")),"Virtualized scrolling reaches last entity and shows live values"))yield break;
            Destroy(listHost);
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
            WatchPlacementProbe.Run((ok,name)=>Check(ok,name));
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

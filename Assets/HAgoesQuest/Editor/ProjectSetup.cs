using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Meta.XR.MRUtilityKit;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;

namespace HAgoesQuest.Editor
{
    public static class ProjectSetup
    {
        public const string ScenePath = "Assets/HAgoesQuest/Scenes/Home.unity";

        [MenuItem("HA goes Quest/1 - Projekt konfigurieren")]
        public static void Configure()
        {
            PlayerSettings.companyName = "HAgoesQuest";
            PlayerSettings.productName = "HA goes Quest";
            PlayerSettings.bundleVersion = "0.3.1";
            PlayerSettings.Android.bundleVersionCode = 4;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.alexrosbach.hagoesquest");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32;
            PlayerSettings.Android.applicationEntry = AndroidApplicationEntry.GameActivity;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.forceInternetPermission = true;
            PlayerSettings.insecureHttpOption = InsecureHttpOption.AlwaysAllowed;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
            QualitySettings.antiAliasing = 4;
            foreach (string iconName in new[] { "lamp", "map", "sensor", "switch" })
            {
                string iconPath = "Assets/HAgoesQuest/Resources/Icons/" + iconName + ".png";
                var importer = AssetImporter.GetAtPath(iconPath) as TextureImporter;
                if (!importer) continue;
                importer.textureType = TextureImporterType.Default;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.isReadable = true;
                importer.maxTextureSize = 512;
                importer.SaveAndReimport();
            }
            var appIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/HAgoesQuest/Resources/Icons/lamp.png");
            if (appIcon) PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new[] { appIcon }, IconKind.Any);
            var player = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var input = player.FindProperty("activeInputHandler");
            if (input != null) { input.intValue = 1; player.ApplyModifiedProperties(); }

            var config = OVRProjectConfig.CachedProjectConfig;
            config.handTrackingSupport = OVRProjectConfig.HandTrackingSupport.ControllersAndHands;
            config.anchorSupport = OVRProjectConfig.AnchorSupport.Enabled;
            config.sceneSupport = OVRProjectConfig.FeatureSupport.Required;
            config.insightPassthroughSupport = OVRProjectConfig.FeatureSupport.Required;
            config.systemLoadingScreenBackground = OVRProjectConfig.SystemLoadingScreenBackground.ContextualPassthrough;
            config.isPassthroughCameraAccessEnabled = true;
            OVRProjectConfig.CommitProjectConfig(config);

            if (!EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget all))
            {
                Directory.CreateDirectory("Assets/XR");
                all = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                AssetDatabase.CreateAsset(all, "Assets/XR/XRGeneralSettingsPerBuildTarget.asset");
                EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, all, true);
            }
            if (!all.HasManagerSettingsForBuildTarget(BuildTargetGroup.Android)) all.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);
            var settings = all.SettingsForBuildTarget(BuildTargetGroup.Android);
            settings.InitManagerOnStart = true;
            if (!XRPackageMetadataStore.AssignLoader(settings.Manager, "UnityEngine.XR.OpenXR.OpenXRLoader", BuildTargetGroup.Android))
                throw new BuildFailedException("OpenXR Loader konnte nicht eingerichtet werden.");
            var set = OpenXRFeatureSetManager.GetFeatureSetWithId(BuildTargetGroup.Android, "com.meta.openxr.featureset.metaxr");
            if (set == null) throw new BuildFailedException("Meta XR Feature Set fehlt. Paketimport abwarten.");
            set.isEnabled = true;
            OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(BuildTargetGroup.Android);
            var xr = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            var touch = xr.GetFeature<OculusTouchControllerProfile>();
            if (touch == null) throw new BuildFailedException("Oculus Touch Controller Profile fehlt.");
            touch.enabled = true;
            var composition = xr.GetFeature<UnityEngine.XR.OpenXR.Features.CompositionLayers.OpenXRCompositionLayersFeature>();
            if (composition == null) throw new BuildFailedException("OpenXR Composition Layers Support fehlt.");
            composition.enabled = true;
            EditorUtility.SetDirty(composition);
            EditorUtility.SetDirty(touch);
            EditorUtility.SetDirty(xr);
            EditorUtility.SetDirty(all);
            EditorUtility.SetDirty(settings);
            EditorUtility.SetDirty(settings.Manager);
            AssetDatabase.SaveAssets();
            Debug.Log("HA goes Quest konfiguriert. Android aktivieren, anschließend Meta XR Project Setup / OpenXR Project Validation prüfen.");
        }

        [MenuItem("HA goes Quest/2 - Startszene erstellen")]
        public static void CreateScene()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath))
            {
                EditorSceneManager.OpenScene(ScenePath);
                UpgradeScene();
                return;
            }
            Directory.CreateDirectory("Assets/HAgoesQuest/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.meta.xr.sdk.core/Prefabs/OVRCameraRig.prefab");
            if (!prefab) throw new BuildFailedException("Meta XR Camera Rig fehlt. Paketimport abwarten.");
            var rigObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            var rig = rigObject.GetComponent<OVRCameraRig>();
            rig.EnsureGameObjectIntegrity();
            var manager = rigObject.GetComponent<OVRManager>();
            manager.isInsightPassthroughEnabled = true;
            manager.trackingOriginType = OVRManager.TrackingOrigin.FloorLevel;
            rigObject.AddComponent<OVRPassthroughLayer>();
            foreach (var camera in rigObject.GetComponentsInChildren<Camera>(true))
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.clear;
                camera.nearClipPlane = 0.1f;
            }
            var mruk = new GameObject("Room understanding").AddComponent<MRUK>();
            mruk.SceneSettings = new MRUK.MRUKSettings { LoadSceneOnStartup = false };
            var app = new GameObject("HA goes Quest");
            app.AddComponent<HomeAssistantConnection>();
            app.AddComponent<SpatialBindings>();
            var home = app.AddComponent<QuestHome>();
            home.Rig = rig;
            home.LeftHand = CreateHand(rig.leftHandAnchor, 0);
            home.RightHand = CreateHand(rig.rightHandAnchor, 1);
            UpgradeScene();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Selection.activeGameObject = app;
            AssetDatabase.SaveAssets();
        }

        static OVRHand CreateHand(Transform parent, int side)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.meta.xr.sdk.core/Prefabs/OVRHandPrefab.prefab");
            if (!prefab) throw new BuildFailedException("Meta XR Hand Prefab fehlt.");
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            foreach (var component in go.GetComponentsInChildren<Component>(true))
            {
                var serialized = new SerializedObject(component);
                foreach (string field in new[] { "HandType", "_skeletonType", "_meshType" })
                {
                    var property = serialized.FindProperty(field);
                    if (property != null) property.intValue = side;
                }
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            return go.GetComponent<OVRHand>();
        }

        static void UpgradeScene()
        {
            // Keep only invisible tracking; the user's real hands remain visible in passthrough.
            foreach (var hand in UnityEngine.Object.FindObjectsByType<OVRHand>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                foreach (var mesh in hand.GetComponentsInChildren<OVRMeshRenderer>(true)) mesh.enabled = false;
                foreach (var renderer in hand.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
            }
            var home = UnityEngine.Object.FindFirstObjectByType<QuestHome>();
            if (home && !home.GetComponent<QrTokenScanner>()) home.gameObject.AddComponent<QrTokenScanner>();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
        }

        [MenuItem("HA goes Quest/3 - Entwicklungs-APK bauen")]
        public static void Build()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                throw new BuildFailedException("Zuerst in Build Profiles auf Android wechseln und die Neukompilierung abwarten.");
            if (!File.Exists(ScenePath)) throw new BuildFailedException("Zuerst die Startszene erstellen.");
            Directory.CreateDirectory("Builds");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { ScenePath }, locationPathName = "Builds/HAgoesQuest.apk",
                target = BuildTarget.Android, options = BuildOptions.Development });
            if (report.summary.result != BuildResult.Succeeded) throw new BuildFailedException("APK-Build fehlgeschlagen. Unity-Konsole prüfen.");
        }

        public static void PrepareBatch()
        {
            Configure();
            CreateScene();
        }
    }

    // Runtime still requires allowInsecureHttp=true for local HTTP installations.
    public sealed class AndroidManifest : IPostGenerateGradleAndroidProject
    {
        // Meta's manifest callback runs at 99999 and installs an HTTPS-only policy.
        public int callbackOrder => 100000;
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            string manifest = Path.Combine(path, "src/main/AndroidManifest.xml");
            var xml = XDocument.Load(manifest);
            XNamespace android = "http://schemas.android.com/apk/res/android";
            var application = xml.Root.Element("application");
            application.SetAttributeValue(android + "usesCleartextTraffic", "true");
            application.SetAttributeValue(android + "allowBackup", "false");
            application.SetAttributeValue(android + "networkSecurityConfig", "@xml/hagoesquest_network_config");
            string resources = Path.Combine(path, "src/main/res/xml");
            Directory.CreateDirectory(resources);
            new XDocument(new XElement("network-security-config",
                new XElement("base-config", new XAttribute("cleartextTrafficPermitted", "true"))))
                .Save(Path.Combine(resources, "hagoesquest_network_config.xml"));
            foreach (string permission in new[] { "android.permission.INTERNET", "com.oculus.permission.USE_SCENE", "com.oculus.permission.USE_ANCHOR_API", "horizonos.permission.HEADSET_CAMERA" })
            {
                if (!xml.Root.Elements("uses-permission").Any(e => (string)e.Attribute(android + "name") == permission))
                    xml.Root.Add(new XElement("uses-permission", new XAttribute(android + "name", permission)));
            }
            if (!xml.Root.Elements("uses-feature").Any(e => (string)e.Attribute(android + "name") == "com.oculus.feature.BOUNDARYLESS_APP"))
                xml.Root.Add(new XElement("uses-feature", new XAttribute(android + "name", "com.oculus.feature.BOUNDARYLESS_APP"), new XAttribute(android + "required", "true")));
            xml.Save(manifest);
        }
    }
}

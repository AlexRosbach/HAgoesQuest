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
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.alexrosbach.hagoesquest");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.forceInternetPermission = true;
            PlayerSettings.insecureHttpOption = InsecureHttpOption.AlwaysAllowed;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
            QualitySettings.antiAliasing = 4;
            var player = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var input = player.FindProperty("activeInputHandler");
            if (input != null) { input.intValue = 2; player.ApplyModifiedProperties(); }

            var config = OVRProjectConfig.CachedProjectConfig;
            config.handTrackingSupport = OVRProjectConfig.HandTrackingSupport.ControllersAndHands;
            config.anchorSupport = OVRProjectConfig.AnchorSupport.Enabled;
            config.sceneSupport = OVRProjectConfig.FeatureSupport.Required;
            config.insightPassthroughSupport = OVRProjectConfig.FeatureSupport.Required;
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
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath))
            {
                EditorSceneManager.OpenScene(ScenePath);
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
    }

    // Runtime still requires allowInsecureHttp=true for local HTTP installations.
    public sealed class AndroidManifest : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 10000;
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            string manifest = Path.Combine(path, "src/main/AndroidManifest.xml");
            var xml = XDocument.Load(manifest);
            XNamespace android = "http://schemas.android.com/apk/res/android";
            var application = xml.Root.Element("application");
            application.SetAttributeValue(android + "usesCleartextTraffic", "true");
            application.SetAttributeValue(android + "allowBackup", "false");
            foreach (string permission in new[] { "android.permission.INTERNET", "com.oculus.permission.USE_SCENE", "com.oculus.permission.USE_ANCHOR_API" })
            {
                if (!xml.Root.Elements("uses-permission").Any(e => (string)e.Attribute(android + "name") == permission))
                    xml.Root.Add(new XElement("uses-permission", new XAttribute(android + "name", permission)));
            }
            xml.Save(manifest);
        }
    }
}

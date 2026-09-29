using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HAgoesQuest.Editor
{
    // The SDK injects local editor connection credentials in its order-1 callback.
    // Move the whole settings asset to an Editor-only location after that callback.
    // Never inspect, log or copy its contents into a distributable build.
    public sealed class PrivateBuildSettings : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        const string Source="Assets/Resources/DevAgentSettings.asset";
        const string Staged="Assets/HAgoesQuest/Editor/DevAgentSettings.asset";
        public int callbackOrder=>int.MaxValue;
        public void OnPreprocessBuild(BuildReport report)
        {
            if(File.Exists(Staged))throw new BuildFailedException("Private SDK settings are already staged; restore them before building.");
            if(File.Exists(Source))Move(Source,Staged);
        }
        public void OnPostprocessBuild(BuildReport report)
        {
            try
            {
                foreach(var packed in report.packedAssets)foreach(var item in packed.contents)
                    if(item.sourceAssetPath.EndsWith("/DevAgentSettings.asset",StringComparison.OrdinalIgnoreCase))
                        throw new BuildFailedException("Local SDK connection settings unexpectedly entered the player build.");
                Debug.Log("HA goes Quest: private SDK settings excluded from player build (packed asset report checked).");
            }
            finally{Restore();}
        }
        public static void Restore()
        {
            if(!File.Exists(Staged))return;
            if(File.Exists(Source))throw new BuildFailedException("Both private settings locations exist; refusing to overwrite either file.");
            Move(Staged,Source);
        }
        static void Move(string from,string to)
        {
            string error=AssetDatabase.MoveAsset(from,to);
            if(!string.IsNullOrEmpty(error))throw new BuildFailedException("Cannot isolate local SDK settings: "+error);
            AssetDatabase.SaveAssets();
        }
    }
}

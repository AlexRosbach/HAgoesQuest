#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HAgoesQuest.Testing
{
    public sealed class UiPreview : MonoBehaviour
    {
        public static void Run()
        {
            if(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HA_QUEST_UI_PREVIEW")))throw new InvalidOperationException("Preview output path required.");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.isPlaying=true;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if(!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HA_QUEST_UI_PREVIEW")))new GameObject("UI render probe").AddComponent<UiPreview>();
        }
        IEnumerator Start()
        {
            var cameraObject=new GameObject("Preview camera",typeof(Camera));var camera=cameraObject.GetComponent<Camera>();
            camera.transform.position=new Vector3(0,0,0);camera.orthographic=true;camera.orthographicSize=.6f;camera.backgroundColor=new Color(.024f,.026f,.032f);camera.clearFlags=CameraClearFlags.SolidColor;
            var host=new GameObject("Preview app");var ha=host.AddComponent<HomeAssistantConnection>();ha.enabled=false;ha.EnterDemo();
            var bindings=host.AddComponent<SpatialBindings>();bindings.enabled=false;
            var home=host.AddComponent<QuestHome>();home.enabled=false;
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            typeof(QuestHome).GetField("head",flags).SetValue(home,camera.transform);
            typeof(QuestHome).GetField("ha",flags).SetValue(home,ha);typeof(QuestHome).GetField("bindings",flags).SetValue(home,bindings);
            ha.Entities.Clear();
            string[] domains={"light","sensor","switch","media_player","camera","climate","sensor","image","cover","vacuum","lock","fan"};
            string[] names={"Deckenlicht","Raumtemperatur","Steckdose Schreibtisch","Wohnzimmer Audio","Eingangskamera","Heizung","Luftfeuchtigkeit","Bilderrahmen","Rollladen","Saugroboter","Haustür","Ventilator"};
            for(int i=0;i<domains.Length;i++)ha.Entities[domains[i]+".preview"+i]=new HaEntity{Id=domains[i]+".preview"+i,Name=names[i],State=i==1?"21.5":i==6?"48":"on",Unit=i==1?"°C":i==6?"%":"",DeviceClass=i==1?"temperature":i==6?"humidity":"",AreaName=i%2==0?"Wohnzimmer":"Arbeitszimmer"};
            typeof(QuestHome).GetMethod("EntityMenu",flags).Invoke(home,null);
            yield return new WaitForSeconds(.7f);
            var target=new RenderTexture(1280,960,24);camera.targetTexture=target;camera.Render();RenderTexture.active=target;
            var image=new Texture2D(1280,960,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,960),0,0);image.Apply();
            File.WriteAllBytes(Environment.GetEnvironmentVariable("HA_QUEST_UI_PREVIEW"),image.EncodeToPNG());
            RenderTexture.active=null;camera.targetTexture=null;Destroy(target);Destroy(image);EditorApplication.Exit(0);
        }
    }
}
#endif

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
            string page=Environment.GetEnvironmentVariable("HA_QUEST_PREVIEW_PAGE")??"Home";
            typeof(QuestHome).GetField("watchDraft",flags).SetValue(home,new WatchPlacement());
            typeof(QuestHome).GetMethod(page,flags).Invoke(home,null);
            var status=(WorldCard)typeof(QuestHome).GetField("status",flags).GetValue(home);
            status.Label.text="Home Assistant · verbunden\n"+(string)typeof(QuestHome).GetField("notice",flags).GetValue(home);
            if(page=="Home")
            {
                var watch=WorldCard.Create(null,new Vector3(.65f,-.32f,1.3f),new Vector2(90,90),"12:48",()=>{});watch.MakeCompactMarker();watch.IsMarker=false;
                watch.Label.rectTransform.anchorMin=watch.Label.rectTransform.anchorMax=Vector2.one*.5f;watch.Label.rectTransform.pivot=Vector2.one*.5f;watch.Label.rectTransform.sizeDelta=new Vector2(85,75);watch.Label.color=Color.white;
                var dial=watch.gameObject.AddComponent<WristDial>();dial.Initialize(watch);dial.SetConnection(true);watch.UnitScale=.002f;
            }
            yield return new WaitForSeconds(.7f);
            var target=new RenderTexture(1280,960,24);camera.targetTexture=target;camera.Render();RenderTexture.active=target;
            var image=new Texture2D(1280,960,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,960),0,0);image.Apply();
            File.WriteAllBytes(Environment.GetEnvironmentVariable("HA_QUEST_UI_PREVIEW"),image.EncodeToPNG());
            RenderTexture.active=null;camera.targetTexture=null;Destroy(target);Destroy(image);
            if(page=="Home")
            {
                typeof(QuestHome).GetMethod("WatchPositionMenu",flags).Invoke(home,null);
                status=(WorldCard)typeof(QuestHome).GetField("status",flags).GetValue(home);
                status.Label.text=(string)typeof(QuestHome).GetField("notice",flags).GetValue(home);
                yield return new WaitForSeconds(.7f);
                target=new RenderTexture(1280,960,24);camera.targetTexture=target;camera.Render();RenderTexture.active=target;
                image=new Texture2D(1280,960,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,960),0,0);image.Apply();
                File.WriteAllBytes(Environment.GetEnvironmentVariable("HA_QUEST_UI_PREVIEW").Replace("-menu.png","-watch.png"),image.EncodeToPNG());
                RenderTexture.active=null;camera.targetTexture=null;Destroy(target);Destroy(image);
            }
            foreach(string nextPage in new[]{"EntityMenu","Filters","FeedbackSettings"})
            {
                camera.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
                typeof(QuestHome).GetMethod(nextPage,flags).Invoke(home,null);
                status=(WorldCard)typeof(QuestHome).GetField("status",flags).GetValue(home);
                status.Label.text=(string)typeof(QuestHome).GetField("notice",flags).GetValue(home);
                yield return new WaitForSeconds(.7f);
                foreach(float angle in new[]{0f,-35f,35f})
                {
                    Vector3 center=new Vector3(0,-.025f,1.4f);
                    camera.transform.position=center-Quaternion.Euler(10,angle,0)*Vector3.forward*1.4f;camera.transform.LookAt(center);
                    Canvas.ForceUpdateCanvases();yield return null;
                    target=new RenderTexture(1280,960,24);camera.targetTexture=target;camera.Render();RenderTexture.active=target;
                    image=new Texture2D(1280,960,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,960),0,0);image.Apply();
                    File.WriteAllBytes(Environment.GetEnvironmentVariable("HA_QUEST_UI_PREVIEW").Replace("-menu.png","-"+nextPage+"-"+angle+".png"),image.EncodeToPNG());
                    RenderTexture.active=null;camera.targetTexture=null;Destroy(target);Destroy(image);
                }
            }
            EditorApplication.Exit(0);
        }
    }
}
#endif

using UnityEngine;
using UnityEngine.UI;

namespace HAgoesQuest
{
    public static class MenuDesign
    {
        public static void Frame(Transform parent)
        {
            var frame=WorldCard.Create(parent,new Vector3(0,-.025f,.025f),new Vector2(900,1040),"");
            frame.Interactable=false;frame.GetComponent<Collider>().enabled=false;frame.SetSurface(WorldCard.Surface.Panel);
            var shadow=frame.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.35f);shadow.effectDistance=new Vector2(8,-14);
        }
        public static void Tile(WorldCard card,string sculpture)
        {
            card.Label.fontSize=23;card.Label.rectTransform.offsetMin=new Vector2(105,12);card.Label.rectTransform.offsetMax=new Vector2(-18,-12);card.Label.alignment=TextAnchor.MiddleLeft;
            var display=new GameObject("Sculpted button icon");display.transform.SetParent(card.transform,false);display.transform.localPosition=new Vector3(-130,0,-18);display.transform.localScale=Vector3.one*.85f;
            var sculpt=display.AddComponent<ButtonSculpture>();sculpt.Initialize(card,sculpture);
        }
    }
    public sealed class ButtonSculpture:MonoBehaviour
    {
        WorldCard card;Material material;
        public void Initialize(WorldCard owner,string kind)
        {
            card=owner;material=new Material(Resources.Load<Shader>("SpatialButton"));material.color=new Color(.77f,.72f,1);
            if(kind=="lamp") {Part(PrimitiveType.Sphere,new Vector3(0,12,0),new Vector3(54,54,54));Part(PrimitiveType.Cylinder,new Vector3(0,-22,0),new Vector3(28,13,28));}
            else if(kind=="map") {Part(PrimitiveType.Cube,new Vector3(-20,0,0),new Vector3(26,65,15));Part(PrimitiveType.Cube,new Vector3(10,8,8),new Vector3(26,65,15));Part(PrimitiveType.Cube,new Vector3(35,0,0),new Vector3(20,65,15));}
            else if(kind=="settings") {Part(PrimitiveType.Cylinder,Vector3.zero,new Vector3(63,9,63),Quaternion.Euler(90,0,0));Part(PrimitiveType.Sphere,new Vector3(0,0,-12),new Vector3(27,27,15));}
            else {Part(PrimitiveType.Cube,Vector3.zero,new Vector3(47,47,26),Quaternion.Euler(10,22,0));Part(PrimitiveType.Sphere,new Vector3(18,18,-18),Vector3.one*21);}
        }
        void Part(PrimitiveType type,Vector3 position,Vector3 scale,Quaternion? rotation=null)
        {
            var part=GameObject.CreatePrimitive(type);Destroy(part.GetComponent<Collider>());part.transform.SetParent(transform,false);
            part.transform.localPosition=position;part.transform.localScale=scale;part.transform.localRotation=rotation??Quaternion.identity;part.GetComponent<Renderer>().sharedMaterial=material;
        }
        void Update(){if(card)transform.localRotation=Quaternion.Lerp(transform.localRotation,Quaternion.Euler(card.IsFocused?-16:-8,card.IsFocused?35:22,0),1-Mathf.Exp(-10*Time.unscaledDeltaTime));}
        void OnDestroy(){if(material)Destroy(material);}
    }
    public sealed class WristDial:MonoBehaviour
    {
        Text date;Image dot;
        public void Initialize(WorldCard card)
        {
            card.UnitScale=.00085f;card.Label.fontSize=24;card.Label.fontStyle=FontStyle.Bold;card.Label.alignment=TextAnchor.MiddleCenter;
            card.Label.rectTransform.anchoredPosition=new Vector2(0,2);
            var title=TextAt("HA HOME",new Vector2(0,23),10);title.color=new Color(.72f,.67f,1);
            date=TextAt("",new Vector2(0,-18),10);
            for(int i=0;i<24;i++)
            {
                float a=i*Mathf.PI/12;var tick=new GameObject("Dial tick",typeof(RectTransform),typeof(Image));tick.transform.SetParent(transform,false);
                var img=tick.GetComponent<Image>();img.raycastTarget=false;img.color=i<6?new Color(.67f,.53f,1):new Color(.35f,.37f,.47f);
                img.rectTransform.anchoredPosition=new Vector2(Mathf.Sin(a),Mathf.Cos(a))*39;img.rectTransform.sizeDelta=new Vector2(i%6==0?2:1,i%6==0?7:4);img.rectTransform.localRotation=Quaternion.Euler(0,0,-i*15);
            }
            var status=new GameObject("Connection indicator",typeof(RectTransform),typeof(Image));status.transform.SetParent(transform,false);dot=status.GetComponent<Image>();dot.raycastTarget=false;dot.rectTransform.sizeDelta=Vector2.one*4;dot.rectTransform.anchoredPosition=new Vector2(0,-29);
        }
        Text TextAt(string value,Vector2 position,int size)
        {
            var go=new GameObject("Dial label",typeof(RectTransform),typeof(Text));go.transform.SetParent(transform,false);var text=go.GetComponent<Text>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=size;text.text=value;text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;text.rectTransform.sizeDelta=new Vector2(80,20);text.rectTransform.anchoredPosition=position;return text;
        }
        public void SetConnection(bool fresh){dot.color=fresh?new Color(.4f,1,.65f):new Color(1,.65f,.25f);date.text=System.DateTime.Now.ToString("dd.MM.");}
    }
    public sealed partial class QuestHome
    {
        void WatchSettings()
        {
            NewMenu(Words("Deine HA-Uhr","Your HA watch"));
            Button(0,ExperienceSettings.Wrist?Words("Uhr: an","Watch: on"):Words("Uhr: aus","Watch: off"),()=>{ExperienceSettings.Wrist=!ExperienceSettings.Wrist;WatchSettings();},"settings");
            Button(1,ExperienceSettings.LeftHanded?Words("Linkshänder · Uhr rechts","Left handed · watch on right"):Words("Rechtshänder · Uhr links","Right handed · watch on left"),()=>{ExperienceSettings.LeftHanded=!ExperienceSettings.LeftHanded;ResetGaze();menuGate.Reset();WatchSettings();});
            Button(4,L10n.T("Zurück"),ExperienceMenu);
            notice=Words("Mit der anderen Hand auf das Zifferblatt tippen. Die Bedienhand übernimmt Pinch, Faustdrehen und Magie; die Uhrenhand öffnet das Menü auch per Pinch.","Tap the dial with the opposite hand. The dominant hand controls pinch, fist rotation and magic; the watch hand can also pinch to open the menu.");
        }
    }
}

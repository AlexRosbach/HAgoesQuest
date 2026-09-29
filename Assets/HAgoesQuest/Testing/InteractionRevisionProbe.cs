#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HAgoesQuest.Testing
{
    public static class InteractionRevisionProbe
    {
        public static void Run(HomeAssistantConnection ha,Action<bool,string> check)
        {
            var host=new GameObject("Room and menu fixture");var head=new GameObject("Head fixture");head.transform.position=new Vector3(1,1,1);
            var bindings=host.AddComponent<SpatialBindings>();bindings.enabled=false;var home=host.AddComponent<QuestHome>();home.enabled=false;
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            Action<string,object> set=(name,value)=>typeof(QuestHome).GetField(name,flags).SetValue(home,value);
            Func<string,object> get=name=>typeof(QuestHome).GetField(name,flags).GetValue(home);
            Action<string> call=name=>typeof(QuestHome).GetMethod(name,flags).Invoke(home,null);
            set("head",head.transform);set("ha",ha);set("bindings",bindings);
            bool oldHints=ExperienceSettings.GestureHints,oldGlow=ExperienceSettings.HandGlow,oldEnglish=ExperienceSettings.English;
            ExperienceSettings.English=false;
            var markerA=WorldCard.Create(null,new Vector3(2,1,1),Vector2.one*90,"A");
            var markerB=WorldCard.Create(null,new Vector3(6,1,1),Vector2.one*90,"B");
            var glowHost=new GameObject("Glow fixture");var glow=glowHost.AddComponent<HandReadinessGlow>();glow.Initialize();
            try
            {
                call("EntityMenu");var menu=(GameObject)get("menu");var root=menu.GetComponent<Canvas>();
                check(root&&root.renderMode==RenderMode.WorldSpace,"Menu has one shared world-space canvas");
                check(menu.GetComponentsInChildren<Canvas>().All(c=>c.rootCanvas==root&&!c.overrideSorting),"Toolbar, rows and panel share a fixed rendering hierarchy");
                check(!menu.transform.GetChild(0).GetComponent<WorldCard>().Interactable,"Menu background is always the first sibling");
                var filter=menu.GetComponentsInChildren<WorldCard>().Single(c=>c.Label.text=="Suchen & Filtern");
                check(filter.Press!=null&&filter.GetComponent<Collider>().enabled&&!filter.GetComponentInParent<UnityEngine.UI.RectMask2D>(),"Filter button remains outside the scrolling clip rectangle");
                filter.Activate();menu=(GameObject)get("menu");
                check(menu.GetComponentsInChildren<WorldCard>().Any(c=>c.Label.text.StartsWith("Art: "))&&menu.GetComponentsInChildren<WorldCard>().Any(c=>c.Label.text.StartsWith("Raum: "))&&menu.GetComponentsInChildren<WorldCard>().Any(c=>c.Label.text.StartsWith("Suchen: ")),"Filter button opens type, room and search controls");
                var scope=(RoomScope)get("roomScope");
                Func<Vector3,bool> a=p=>p.x>=0&&p.x<4&&p.y>=0&&p.y<3&&p.z>=0&&p.z<4;
                Func<Vector3,bool> b=p=>p.x>=4&&p.x<8&&p.y>=0&&p.y<3&&p.z>=0&&p.z<4;
                check(scope.Update("room-a",head.transform.position,a)&&scope.Contains(markerA.transform.position)&&!scope.Contains(markerB.transform.position),"Only the headset room admits entities");
                var markers=(Dictionary<string,WorldCard>)get("markers");markers["light.a"]=markerA;markers["light.b"]=markerB;
                Func<string,bool> allowed=id=>(bool)typeof(QuestHome).GetMethod("EntityAllowedHere",flags).Invoke(home,new object[]{id});
                check(allowed("light.a")&&!allowed("light.b"),"Command gate rejects another room even when its card exists");
                head.transform.position=new Vector3(5,1,1);scope.Update("room-b",head.transform.position,b);
                check(!allowed("light.a")&&allowed("light.b"),"Crossing to the next room reverses command eligibility");
                check(!scope.Contains(new Vector3(5,4,1)),"Another storey is outside the current room");
                check(scope.Update("room-b",new Vector3(9,1,1),b)&&!scope.HasRoom&&!scope.Contains(markerB.transform.position),"A stale current-room result cannot reveal entities outside room bounds");
                check(!scope.Update(null,Vector3.zero,null)&&!scope.Contains(Vector3.zero),"Unknown room fails closed");
                glow.Show(head.transform,markerA.transform,markerB.transform,head.transform,true,true);
                check(glow.gameObject.activeSelf&&glow.GetComponent<SpriteRenderer>()&&!glow.GetComponent<Collider>(),"Readiness uses a non-interactive glow instead of a hand model");
                glow.Show(null,null,null,head.transform,true,true);check(!glow.gameObject.activeSelf,"Missing tracked bones immediately hide readiness glow");
                call("FeedbackSettings");menu=(GameObject)get("menu");
                menu.GetComponentsInChildren<WorldCard>().Single(c=>c.Label.text.StartsWith("Gesten-Untertitel:")).Activate();
                check(ExperienceSettings.GestureHints!=oldHints,"Gesture captions can be switched independently");
                menu=(GameObject)get("menu");menu.GetComponentsInChildren<WorldCard>().Single(c=>c.Label.text.StartsWith("Grüner Hand-Effekt:")).Activate();
                check(ExperienceSettings.HandGlow!=oldGlow,"Hand glow preference is independently persisted");
            }
            finally
            {
                ExperienceSettings.GestureHints=oldHints;ExperienceSettings.HandGlow=oldGlow;ExperienceSettings.English=oldEnglish;
                call("CloseMenu");UnityEngine.Object.Destroy(glowHost);UnityEngine.Object.Destroy(markerA.gameObject);UnityEngine.Object.Destroy(markerB.gameObject);UnityEngine.Object.Destroy(host);UnityEngine.Object.Destroy(head);
            }
        }
    }
}
#endif

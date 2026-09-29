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
                menu.GetComponentsInChildren<WorldCard>().Single(c=>c.Label.text.StartsWith("Raum: ")).Activate();menu=(GameObject)get("menu");
                check(ha.AreaNames.ContainsKey("empty"),"Area catalog includes rooms without supported entities");
                var living=menu.GetComponentsInChildren<WorldCard>().FirstOrDefault(c=>c.Label.text=="Gästezimmer");
                check(living!=null,"Area selection displays empty HA area");
                living.Activate();menu=(GameObject)get("menu");
                check(menu.GetComponentsInChildren<WorldCard>().Any(c=>c.Label.text=="Raum: Gästezimmer"),"Selecting an area persists the filter");
                check(Mathf.Abs(HandPose.RollVector(Vector3.right,Quaternion.identity))<.001f,"Captured roll reference stays constant independently of current head pose");
                var gesture=(HAgoesQuest.Core.GazeGestureGate)typeof(QuestHome).GetProperty("grip",flags).GetValue(home);
                gesture.Step("light.a",true,false,true,false,true,.5f,0,0);
                gesture.Step("light.a",true,false,true,false,true,.5f,0,.2);
                gesture.Step("light.a",true,true,false,false,true,.5f,0,.22);
                set("capturedTarget",markerA);head.transform.rotation=Quaternion.Euler(0,180,0);
                check((WorldCard)typeof(QuestHome).GetMethod("GestureTarget",flags).Invoke(home,null)==markerA,"Closed fist retains entity after looking away");
                gesture.Step("light.a",true,true,false,false,true,.5f,40,.4);
                gesture.Step("light.a",true,false,true,false,true,.5f,40,.5);
                gesture.Step("light.a",true,false,true,false,true,.5f,40,.6);
                check(gesture.Commit&&gesture.Value>.7f,"Captured adjustment commits on open after head turns");
                call("ResetGaze");check(get("capturedTarget")==null,"Menu or room reset clears captured target");head.transform.rotation=Quaternion.identity;
                var listHost=new GameObject("Toolbar overlap regression");var list=listHost.AddComponent<EntityScrollList>();int selected=0;
                list.Initialize(new[]{new HaEntity{Id="light.fixture",Name="Fixture",State="on"}},ha,_=>selected++);
                var toolbarRay=new Ray(new Vector3(-.206f,.29f,-1),Vector3.forward);
                list.Handle(toolbarRay,true,true,new Vector3(-.206f,.156f,0));
                list.Handle(toolbarRay,false,false,Vector3.zero);
                check(selected==0,"Ray click on toolbar cannot select the row underneath a nearby hand");
                var rowRay=new Ray(new Vector3(-.206f,.156f,-1),Vector3.forward);Physics.SyncTransforms();
                list.Handle(rowRay,true,false,Vector3.zero);list.CancelDrag();list.Handle(rowRay,false,false,Vector3.zero);
                check(selected==0,"Navigation cancels a pending list selection");UnityEngine.Object.Destroy(listHost);
                call("EntityMenu");menu=(GameObject)get("menu");
                var back=menu.GetComponentsInChildren<WorldCard>().Single(c=>c.Label.text=="Zurück");
                var bridge=host.AddComponent<MetaInputBridge>();set("metaInput",bridge);
                typeof(MetaInputBridge).GetProperty("PokeSelected").SetValue(bridge,back);
                var center=menu.transform.TransformPoint(new Vector3(0,.15f,0));
                typeof(QuestHome).GetMethod("UpdateAlternativeInput",flags).Invoke(home,new object[]{true,true,false,new Ray(center-menu.transform.forward,menu.transform.forward)});
                check(get("entityList")==null,"Direct Back poke takes priority even when the ray overlaps the list");
                call("Filters");menu=(GameObject)get("menu");
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
                check(glow.gameObject.activeSelf&&glow.GetComponentsInChildren<LineRenderer>().Length==6&&!glow.GetComponent<Collider>(),"Readiness uses a non-interactive glow instead of a hand model");
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

#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEngine;

namespace HAgoesQuest.Testing
{
    public static class WatchPlacementProbe
    {
        public static void Run(Action<bool,string> check)
        {
            const string left="HAgoesQuest.experience.watchPose.left",right="HAgoesQuest.experience.watchPose.right";
            string oldLeft=PlayerPrefs.HasKey(left)?PlayerPrefs.GetString(left):null,oldRight=PlayerPrefs.HasKey(right)?PlayerPrefs.GetString(right):null;
            var wrist=new GameObject("Tracked wrist fixture");var watch=WorldCard.Create(null,Vector3.zero,new Vector2(90,90),"12:48");
            var host=new GameObject("Watch editing fixture");var home=host.AddComponent<QuestHome>();home.enabled=false;
            try
            {
                wrist.transform.SetPositionAndRotation(new Vector3(2,1,-3),Quaternion.Euler(15,80,-20));
                Vector3 offset=new Vector3(.01f,.04f,.03f);Quaternion rotation=Quaternion.Euler(20,-15,35);
                watch.transform.SetPositionAndRotation(wrist.transform.TransformPoint(offset),wrist.transform.rotation*rotation);
                var pose=WatchPlacement.Capture(wrist.transform,watch.transform,1.2f);
                check(Vector3.Distance(pose.position,offset)<.00001f&&Quaternion.Angle(pose.rotation,rotation)<.01f,"Watch captures wrist-local position and rotation");
                pose.Save(false);var restored=WatchPlacement.Load(false);
                wrist.transform.SetPositionAndRotation(new Vector3(-4,2,6),Quaternion.Euler(-45,10,70));restored.Apply(wrist.transform,watch);
                check(Vector3.Distance(watch.transform.position,wrist.transform.TransformPoint(offset))<.00001f&&Quaternion.Angle(watch.transform.rotation,wrist.transform.rotation*rotation)<.01f,"Saved watch follows translated and rotated wrist");
                check(Mathf.Abs(watch.UnitScale-.00102f)<.000001f,"Watch size survives persistence");
                var other=new WatchPlacement{position=new Vector3(.02f,0,0),scale=.8f};other.Save(true);
                check(Vector3.Distance(WatchPlacement.Load(false).position,offset)<.00001f&&WatchPlacement.Load(true).scale==.8f,"Left and right wrist placements remain independent");
                restored.Move(Vector3.right*.005f);restored.Rotate(Vector3.up*5);restored.Resize(.05f);
                check(Vector3.Distance(WatchPlacement.Load(false).position,offset)<.00001f&&WatchPlacement.Load(false).scale==1.2f,"Unsaved preview cannot overwrite persisted placement");
                check(Vector3.Distance(restored.position,offset+rotation*Vector3.right*.005f)<.00001f,"Position buttons move along the dial axes");
                restored.Move(Vector3.one*50);restored.Resize(100);
                check(restored.position.magnitude<=.20001f&&restored.scale==1.6f,"Watch offset and size have comfortable bounds");
                WatchPlacement.Reset(false);check(WatchPlacement.Load(false)==null&&WatchPlacement.Load(true)!=null,"Reset affects only the selected wrist");
                PlayerPrefs.SetString(left,"{\"scale\":99}");check(WatchPlacement.Load(false)==null,"Invalid persisted pose falls back safely");
                var flags=BindingFlags.Instance|BindingFlags.NonPublic;
                typeof(QuestHome).GetField("watchEditing",flags).SetValue(home,true);
                typeof(QuestHome).GetMethod("CloseMenu",flags).Invoke(home,null);
                check(!(bool)typeof(QuestHome).GetField("watchEditing",flags).GetValue(home),"Closing the menu cancels watch preview");
                foreach(WorldCard.Surface surface in new[]{WorldCard.Surface.Secondary,WorldCard.Surface.Primary,WorldCard.Surface.Caption})
                {
                    watch.SetSurface(surface);var bg=watch.GetComponent<UnityEngine.UI.Image>().color;
                    check((Luminance(watch.Label.color)+.05f)/(Luminance(bg)+.05f)>=4.5f,"Menu text meets 4.5:1 contrast: "+surface);
                }
            }
            finally
            {
                if(oldLeft==null)PlayerPrefs.DeleteKey(left);else PlayerPrefs.SetString(left,oldLeft);
                if(oldRight==null)PlayerPrefs.DeleteKey(right);else PlayerPrefs.SetString(right,oldRight);
                PlayerPrefs.Save();UnityEngine.Object.Destroy(wrist);UnityEngine.Object.Destroy(watch.gameObject);UnityEngine.Object.Destroy(host);
            }
        }
        static float Linear(float x)=>x<=.04045f?x/12.92f:Mathf.Pow((x+.055f)/1.055f,2.4f);
        static float Luminance(Color c)=>.2126f*Linear(c.r)+.7152f*Linear(c.g)+.0722f*Linear(c.b);
    }
}
#endif

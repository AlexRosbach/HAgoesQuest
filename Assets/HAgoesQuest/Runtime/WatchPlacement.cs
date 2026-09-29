using System;
using UnityEngine;

namespace HAgoesQuest
{
    [Serializable]
    public sealed class WatchPlacement
    {
        public Vector3 position;
        public Quaternion rotation=Quaternion.identity;
        public float scale=1;
        public static int Revision { get; private set; }
        static string Key(bool rightWrist)=>"HAgoesQuest.experience.watchPose."+(rightWrist?"right":"left");
        public static WatchPlacement Capture(Transform wrist,Transform watch,float scale)
            =>new WatchPlacement{position=wrist.InverseTransformPoint(watch.position),rotation=Quaternion.Inverse(wrist.rotation)*watch.rotation,scale=scale};
        public void Apply(Transform wrist,WorldCard watch)
        {
            watch.transform.SetPositionAndRotation(wrist.TransformPoint(position),wrist.rotation*rotation);
            watch.UnitScale=.00085f*scale;
        }
        public void Move(Vector3 dialDirection){position=Vector3.ClampMagnitude(position+rotation*dialDirection,.20f);}
        public void Rotate(Vector3 degrees){rotation=rotation*Quaternion.Euler(degrees);}
        public void Resize(float delta){scale=Mathf.Clamp(scale+delta,.65f,1.6f);}
        public void Save(bool rightWrist){PlayerPrefs.SetString(Key(rightWrist),JsonUtility.ToJson(this));PlayerPrefs.Save();Revision++;}
        public static WatchPlacement Load(bool rightWrist)
        {
            if(!PlayerPrefs.HasKey(Key(rightWrist)))return null;
            try
            {
                var p=JsonUtility.FromJson<WatchPlacement>(PlayerPrefs.GetString(Key(rightWrist)));
                if(p==null||!Finite(p.position.x)||!Finite(p.position.y)||!Finite(p.position.z)||!Finite(p.scale)||p.position.magnitude>.201f||p.scale<.65f||p.scale>1.6f)return null;
                float norm=p.rotation.x*p.rotation.x+p.rotation.y*p.rotation.y+p.rotation.z*p.rotation.z+p.rotation.w*p.rotation.w;
                if(!Finite(norm)||norm<.9f||norm>1.1f)return null;
                p.rotation=Quaternion.Normalize(p.rotation);return p;
            }catch(ArgumentException){return null;}
        }
        static bool Finite(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value);
        public static void Reset(bool rightWrist){PlayerPrefs.DeleteKey(Key(rightWrist));PlayerPrefs.Save();Revision++;}
    }

    public sealed partial class QuestHome
    {
        bool watchEditing;
        WatchPlacement savedWatch;
        bool savedWatchRight;
        int savedWatchRevision=-1;
        WatchPlacement SavedWatch()
        {
            if(savedWatchRevision!=WatchPlacement.Revision||savedWatchRight!=ExperienceSettings.LeftHanded)
            {
                savedWatchRight=ExperienceSettings.LeftHanded;savedWatchRevision=WatchPlacement.Revision;
                savedWatch=WatchPlacement.Load(savedWatchRight);
            }
            return savedWatch;
        }
        WatchPlacement watchDraft;
        WorldCard watchSizeLabel;
        void BeginWatchPosition()
        {
            var wrist=MagicCasting.Bone(WatchSkeleton,OVRSkeleton.BoneId.Hand_WristRoot,OVRSkeleton.BoneId.XRHand_Wrist);
            if(!wrist||!HandReady(MenuHand)||!wristWatch){notice=Words("Bitte die Uhrenhand sichtbar vor dich halten und erneut öffnen.","Hold your watch hand in view, then open this again.");return;}
            // Capture an actual tracked pose, never an untracked/default transform.
            if(!wristWatch.gameObject.activeSelf)DefaultWatchPose(wrist);
            watchDraft=WatchPlacement.Load(ExperienceSettings.LeftHanded)??WatchPlacement.Capture(wrist,wristWatch.transform,wristWatch.UnitScale/.00085f);
            WatchPositionMenu();
        }
        void WatchPositionMenu()
        {
            NewMenu(Words("Uhr anpassen · Live-Vorschau","Adjust watch · live preview"));watchEditing=true;
            WatchAdjustmentRow(0,Words("Seitlich · 5 mm","Sideways · 5 mm"),()=>watchDraft.Move(Vector3.left*.005f),()=>watchDraft.Move(Vector3.right*.005f));
            WatchAdjustmentRow(1,Words("Hoch / runter · 5 mm","Up / down · 5 mm"),()=>watchDraft.Move(Vector3.down*.005f),()=>watchDraft.Move(Vector3.up*.005f));
            WatchAdjustmentRow(2,Words("Abstand zur Haut · 5 mm","Off the skin · 5 mm"),()=>watchDraft.Move(Vector3.forward*.005f),()=>watchDraft.Move(Vector3.back*.005f));
            WatchAdjustmentRow(3,Words("Neigung · 5°","Tilt · 5°"),()=>watchDraft.Rotate(Vector3.right*-5),()=>watchDraft.Rotate(Vector3.right*5));
            WatchAdjustmentRow(4,Words("Seitlich kippen · 5°","Side tilt · 5°"),()=>watchDraft.Rotate(Vector3.up*-5),()=>watchDraft.Rotate(Vector3.up*5));
            WatchAdjustmentRow(5,Words("Drehung · 5°","Rotation · 5°"),()=>watchDraft.Rotate(Vector3.forward*-5),()=>watchDraft.Rotate(Vector3.forward*5));
            watchSizeLabel=WatchAdjustmentRow(6,Words("Größe","Size"),()=>watchDraft.Resize(-.05f),()=>watchDraft.Resize(.05f));
            var save=WorldCard.Create(menu.transform,new Vector3(-.19f,-.33f,0),new Vector2(365,72),Words("Position speichern","Save position"),()=>
            {
                if(!HandReady(MenuHand)){notice=Words("Zum Speichern die Uhrenhand ins Sichtfeld halten.","Bring the watch hand into view to save.");return;}
                watchDraft.Save(ExperienceSettings.LeftHanded);ExperienceSettings.Wrist=true;watchEditing=false;WatchSettings();
            });save.SetSurface(WorldCard.Surface.Primary);
            WorldCard.Create(menu.transform,new Vector3(.19f,-.33f,0),new Vector2(365,72),Words("Abbrechen","Cancel"),()=>{watchEditing=false;WatchSettings();});
            notice=Words("Mit − / + anpassen. Die Uhr bleibt an deinem Handgelenk. Erst Speichern übernimmt die Änderung; Menü schließen verwirft sie.","Adjust with − / +. The watch stays attached to your wrist. Save applies changes; closing the menu discards them.");
        }
        WorldCard WatchAdjustmentRow(int row,string text,Action minus,Action plus)
        {
            float y=.29f-row*.087f;
            var label=WorldCard.Create(menu.transform,new Vector3(0,y,0),new Vector2(470,78),text);label.Interactable=false;label.GetComponent<Collider>().enabled=false;label.Label.fontSize=22;
            WorldCard.Create(menu.transform,new Vector3(-.315f,y,0),new Vector2(110,78),"−",minus);
            WorldCard.Create(menu.transform,new Vector3(.315f,y,0),new Vector2(110,78),"+",plus);
            return label;
        }
        void DefaultWatchPose(Transform wrist)
        {
            var palm=MagicCasting.Bone(WatchSkeleton,OVRSkeleton.BoneId.Hand_Middle1,OVRSkeleton.BoneId.XRHand_MiddleProximal);
            Vector3 towardArm=palm?(wrist.position-palm.position).normalized:Vector3.zero;
            wristWatch.transform.position=wrist.position+towardArm*.055f+(head.position-wrist.position).normalized*.035f;
            wristWatch.Face(head);wristWatch.UnitScale=.00085f;
        }
    }
}

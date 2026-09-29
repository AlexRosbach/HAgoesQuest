using HAgoesQuest.Core;
using UnityEngine;

namespace HAgoesQuest
{
    public sealed partial class QuestHome
    {
        readonly GazePinchGate gazePinch = new GazePinchGate();
        readonly GazeGestureGate grip = new GazeGestureGate();
        WorldCard sensorGaze;
        WorldCard gestureHint;
        HandReadinessGlow readinessGlow;
        OVRHand ControlHand => ExperienceSettings.LeftHanded?LeftHand:RightHand;
        OVRHand MenuHand => ExperienceSettings.LeftHanded?RightHand:LeftHand;
        OVRSkeleton ControlSkeleton => ExperienceSettings.LeftHanded?leftSkeleton:rightSkeleton;
        OVRSkeleton WatchSkeleton => ExperienceSettings.LeftHanded?rightSkeleton:leftSkeleton;
        OVRInput.Controller ControlController => ExperienceSettings.LeftHanded?OVRInput.Controller.LTouch:OVRInput.Controller.RTouch;
        Transform ControllerAnchor => ExperienceSettings.LeftHanded?Rig.leftControllerAnchor:Rig.rightControllerAnchor;
        string Words(string de,string en)=>ExperienceSettings.English?en:de;

        WorldCard GazeTarget()
        {
            WorldCard best=null;float angle=1.7f,distance=float.MaxValue;
            foreach(var card in markers.Values)
                Consider(card,ref best,ref angle,ref distance);
            if(magic?.PracticeTarget)Consider(magic.PracticeTarget,ref best,ref angle,ref distance);
            return best;
        }
        void Consider(WorldCard card,ref WorldCard best,ref float angle,ref float distance)
        {
            if(!card||!card.gameObject.activeInHierarchy||!card.GetComponent<Collider>().enabled)return;
            Vector3 offset=card.transform.position-head.position;float d=offset.magnitude;
            if(d<.15f||d>8)return;
            float a=Vector3.Angle(head.forward,offset);
            if(a>angle || (Mathf.Abs(a-angle)<.01f&&d>=distance))return;
            if(Physics.Raycast(head.position,offset.normalized,out var hit,d+.02f)&&hit.collider.GetComponent<WorldCard>()!=card)return;
            best=card;angle=a;distance=d;
        }
        void ResetGaze()
        {
            gazePinch.Reset();grip.Reset();
            readinessGlow?.Hide();InteractionFeedback.Instance?.ResetAdjustment();
            if(focused){focused.SetMagicFocus(false);focused.Focus(false);}
            if(gestureHint)gestureHint.gameObject.SetActive(false);
        }
        void TickGaze(bool allowed,bool tracked,bool down)
        {
            pointer.enabled=false;if(beamGlow)beamGlow.enabled=false;
            if(focused){focused.Focus(false);focused.SetMagicFocus(false);}
            focused=allowed?GazeTarget():null;
            if(focused)focused.Focus(true);
            reticle.gameObject.SetActive(allowed);
            float distance=focused?Vector3.Distance(head.position,focused.transform.position):1.4f;
            reticle.transform.position=head.position+head.forward*distance;reticle.Face(head);
            reticle.UnitScale=.00024f*distance;reticle.Focus(focused);
            var tip=MagicCasting.Bone(ControlSkeleton,OVRSkeleton.BoneId.Hand_IndexTip,OVRSkeleton.BoneId.XRHand_IndexTip);
            metaInput?.Step(new Ray(head.position,head.forward),false,allowed&&tracked,HandReady(ControlHand)&&tip,tip?tip.position:Vector3.zero);
            if(metaInput&&metaInput.PokeSelected==wristWatch&&wristWatch){wristWatch.Activate();ResetGaze();return;}
            HaEntity entity=null;
            bool practice=focused&&magic!=null&&focused==magic.PracticeTarget;
            if(focused&&!practice)ha.Entities.TryGetValue(focused.EntityId,out entity);
            bool usable=focused&&(practice || (EntityAllowedHere(focused.EntityId)&&focused.Interactable&&!focused.ReadOnly&&ha.Fresh&&!ha.Busy));
            bool adjustable=usable&&entity!=null&&(entity.CanDim||entity.CanVolume);
            bool canCast=usable&&magic!=null&&magic.Enabled&&(practice||(entity!=null&&EntityPolicy.CanSwitch(entity.Id,entity.State)));
            bool poseValid=HandReady(ControlHand)&&ControlHand.HandConfidence==OVRHand.TrackingConfidence.High&&ControlSkeleton&&ControlSkeleton.IsDataValid;
            bool fist=poseValid&&HandPose.IsFist(ControlSkeleton);
            bool open=poseValid&&MagicCasting.OpenPalm(ControlSkeleton,out _,out _);
            bool wasClosed=grip.Closed;
            grip.Step(usable?focused.EntityId:null,allowed&&poseValid,fist,open,canCast,adjustable,entity?.Level??0,
                poseValid?HandPose.Roll(ControlSkeleton,head):0,Time.unscaledTime);
            InteractionFeedback.Instance?.PreviewLevel(entity?.Id,grip.Adjusting,grip.Value);
            if(!readinessGlow){readinessGlow=new GameObject("Real hand readiness glow").AddComponent<HandReadinessGlow>();readinessGlow.Initialize();}
            readinessGlow.Show(
                MagicCasting.Bone(ControlSkeleton,OVRSkeleton.BoneId.Hand_WristRoot,OVRSkeleton.BoneId.XRHand_Wrist),
                MagicCasting.Bone(ControlSkeleton,OVRSkeleton.BoneId.Hand_Index1,OVRSkeleton.BoneId.XRHand_IndexProximal),
                MagicCasting.Bone(ControlSkeleton,OVRSkeleton.BoneId.Hand_Pinky1,OVRSkeleton.BoneId.XRHand_LittleProximal),head,
                ExperienceSettings.HandGlow&&allowed&&poseValid&&(adjustable||canCast)&&(grip.Ready||grip.Closed),grip.Closed);
            if(focused&&canCast)focused.SetMagicFocus(true,grip.Ready||grip.Closed);
            bool consumed=wasClosed||grip.Closed||fist||grip.Commit||grip.Cast;
            if(grip.Commit&&entity!=null&&EntityAllowedHere(entity.Id))ha.SetLevel(entity.Id,grip.Value);
            if(grip.Cast&&focused)
            {
                var wrist=MagicCasting.Bone(ControlSkeleton,OVRSkeleton.BoneId.Hand_WristRoot,OVRSkeleton.BoneId.XRHand_Wrist);
                if(wrist&&MagicCasting.OpenPalm(ControlSkeleton,out var palm,out _))magic.CastAt(palm,focused,head,ha);
            }
            if(consumed)gazePinch.Reset();
            else if(gazePinch.Step(usable?focused.Key:null,down,tracked&&allowed&&(!HandReady(ControlHand)||poseValid),!HandReady(ControlHand)||HandPose.PinchIntent(ControlSkeleton),Time.unscaledTime)&&!practice)focused.Activate();
            if(!gestureHint){gestureHint=WorldCard.Create(null,Vector3.zero,new Vector2(600,84),"");gestureHint.Interactable=false;gestureHint.GetComponent<Collider>().enabled=false;gestureHint.Label.fontSize=24;}
            gestureHint.gameObject.SetActive(ExperienceSettings.GestureHints&&allowed&&focused&&(adjustable||canCast));
            if(gestureHint.gameObject.activeSelf)
            {
                gestureHint.transform.position=head.position+head.forward*1.15f-head.up*.25f;gestureHint.Face(head);
                gestureHint.Label.text=grip.Adjusting?(entity.CanDim?Words("Helligkeit ","Brightness "):Words("Lautstärke ","Volume "))+Mathf.RoundToInt(grip.Value*100)+" %\n"+Words("Hand öffnen: übernehmen","Open hand to apply"):
                    grip.Closed?Words("Faust drehen: regeln · Öffnen: ","Twist to adjust · Open to ")+(canCast?Words("Feuerball","cast"):Words("beenden","finish")):
                    grip.Ready?Words("Bereit · Faust schließen","Ready · close your fist"):Words("Ziel ansehen · Hand kurz öffnen","Look at target · briefly open hand");
            }
        }
    }

    public static class HandPose
    {
        static bool Straight(Transform root,Transform joint,Transform tip)=>root&&joint&&tip&&Vector3.Dot((joint.position-root.position).normalized,(tip.position-joint.position).normalized)>.4f;
        public static bool PinchIntent(OVRSkeleton skeleton)=>
            Straight(MagicCasting.Bone(skeleton,OVRSkeleton.BoneId.Hand_Middle1,OVRSkeleton.BoneId.XRHand_MiddleProximal),MagicCasting.Bone(skeleton,OVRSkeleton.BoneId.Hand_Middle2,OVRSkeleton.BoneId.XRHand_MiddleIntermediate),MagicCasting.Bone(skeleton,OVRSkeleton.BoneId.Hand_MiddleTip,OVRSkeleton.BoneId.XRHand_MiddleTip))&&
            Straight(MagicCasting.Bone(skeleton,OVRSkeleton.BoneId.Hand_Ring1,OVRSkeleton.BoneId.XRHand_RingProximal),MagicCasting.Bone(skeleton,OVRSkeleton.BoneId.Hand_Ring2,OVRSkeleton.BoneId.XRHand_RingIntermediate),MagicCasting.Bone(skeleton,OVRSkeleton.BoneId.Hand_RingTip,OVRSkeleton.BoneId.XRHand_RingTip));
        static bool Curled(Transform root,Transform joint,Transform tip)
        {
            if(!root||!joint||!tip)return false;
            return Vector3.Dot((joint.position-root.position).normalized,(tip.position-joint.position).normalized)<.25f;
        }
        public static bool IsFist(OVRSkeleton skeleton)
        {
            return Curled(MagicCasting.Bone(skeleton,OVRSkeleton.BoneId.Hand_Index1,OVRSkeleton.BoneId.XRHand_IndexProximal),MagicCasting.Bone(skeleton,OVRSkeleton.BoneId.Hand_Index2,OVRSkeleton.BoneId.XRHand_IndexIntermediate),MagicCasting.Bone(skeleton,OVRSkeleton.BoneId.Hand_IndexTip,OVRSkeleton.BoneId.XRHand_IndexTip))&&
                Curled(MagicCasting.Bone(skeleton,OVRSkeleton.BoneId.Hand_Middle1,OVRSkeleton.BoneId.XRHand_MiddleProximal),MagicCasting.Bone(skeleton,OVRSkeleton.BoneId.Hand_Middle2,OVRSkeleton.BoneId.XRHand_MiddleIntermediate),MagicCasting.Bone(skeleton,OVRSkeleton.BoneId.Hand_MiddleTip,OVRSkeleton.BoneId.XRHand_MiddleTip))&&
                Curled(MagicCasting.Bone(skeleton,OVRSkeleton.BoneId.Hand_Ring1,OVRSkeleton.BoneId.XRHand_RingProximal),MagicCasting.Bone(skeleton,OVRSkeleton.BoneId.Hand_Ring2,OVRSkeleton.BoneId.XRHand_RingIntermediate),MagicCasting.Bone(skeleton,OVRSkeleton.BoneId.Hand_RingTip,OVRSkeleton.BoneId.XRHand_RingTip));
        }
        public static float Roll(OVRSkeleton skeleton,Transform head)
        {
            var a=MagicCasting.Bone(skeleton,OVRSkeleton.BoneId.Hand_Index1,OVRSkeleton.BoneId.XRHand_IndexProximal);
            var b=MagicCasting.Bone(skeleton,OVRSkeleton.BoneId.Hand_Pinky1,OVRSkeleton.BoneId.XRHand_LittleProximal);
            if(!a||!b)return float.NaN;
            var across=a.position-b.position;
            if(Vector3.ProjectOnPlane(across,head.forward).magnitude<.008f)return float.NaN;
            return Mathf.Atan2(-Vector3.Dot(across,head.up),Vector3.Dot(across,head.right))*Mathf.Rad2Deg;
        }
    }
}

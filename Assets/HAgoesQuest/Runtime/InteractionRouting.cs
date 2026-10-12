using UnityEngine;

namespace HAgoesQuest
{
    public sealed partial class QuestHome
    {
        int areaMenuPage=-1, displayedAreaRevision=-1;
        bool HandOverMenuControl()
        {
            if(NearMenu(ControlSkeleton))return true;
            if(!ControlHand||!ControlHand.IsPointerPoseValid||!ControlHand.PointerPose)return false;
            if(!Physics.Raycast(ControlHand.PointerPose.position,ControlHand.PointerPose.forward,out var hit,8))return false;
            var card=hit.collider.GetComponent<WorldCard>();return card&&card.Interactable&&menu&&card.transform.IsChildOf(menu.transform);
        }
        bool Pinching(OVRHand hand)=>HandReady(hand)&&hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        bool NearMenu(OVRSkeleton skeleton)
        {
            var tip=MagicCasting.Bone(skeleton,OVRSkeleton.BoneId.Hand_IndexTip,OVRSkeleton.BoneId.XRHand_IndexTip);
            if(!tip||!menu)return false;
            var p=menu.transform.InverseTransformPoint(tip.position);
            return Mathf.Abs(p.x)<.46f&&Mathf.Abs(p.y)<.53f&&Mathf.Abs(p.z)<.06f;
        }
        bool HandIntent(OVRHand hand,OVRSkeleton skeleton)=>HandReady(hand)&&
            (Pinching(hand)||NearMenu(skeleton)||(skeleton&&skeleton.IsDataValid&&(hand==LeftHand?leftGrip.Ready||leftGrip.Closed:rightGrip.Ready||rightGrip.Closed)&&HandPose.IsFist(skeleton))||NearWatch(skeleton));
        bool NearWatch(OVRSkeleton skeleton)
        {
            var tip=MagicCasting.Bone(skeleton,OVRSkeleton.BoneId.Hand_IndexTip,OVRSkeleton.BoneId.XRHand_IndexTip);
            return wristWatch&&tip&&Vector3.Distance(tip.position,wristWatch.transform.position)<.065f;
        }
        /// <summary>Selects an exclusive input owner while retaining that hand's armed gesture.</summary>
        void RouteHands()
        {
            int before=ownership.Owner;
            ownership.Step(HandIntent(LeftHand,leftSkeleton),HandIntent(RightHand,rightSkeleton),leftGrip.Closed||rightGrip.Closed||levelDragging,ExperienceSettings.LeftHanded,Time.unscaledTime);
            if(before!=ownership.Owner)
            {
                metaInput?.Cancel(before<0);entityList?.CancelDrag();rayReady=false;previousPinch=false;
                // Preserve an already armed open palm when that hand starts a fist.
                if(ownership.Owner==0)rightGrip.Reset();else if(ownership.Owner==1)leftGrip.Reset();
            }
        }
        /// <summary>Invalidates pending selections before menu colliders change.</summary>
        void BlockNavigation()
        {
            navigation.Block(Time.unscaledTime);metaInput?.Cancel();entityList?.CancelDrag();areaMenuPage=-1;
        }
        bool NavigationAllowed()=>navigation.Step(Pinching(LeftHand)||Pinching(RightHand)||NearMenu(leftSkeleton)||NearMenu(rightSkeleton)||
            OVRInput.Get(OVRInput.RawButton.LIndexTrigger)||OVRInput.Get(OVRInput.RawButton.RIndexTrigger),Time.unscaledTime);
    }
}

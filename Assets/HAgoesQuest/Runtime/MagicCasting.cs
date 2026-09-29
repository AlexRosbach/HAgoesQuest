using HAgoesQuest.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HAgoesQuest
{
    public sealed class MagicCasting
    {
        const string Preference = "HAgoesQuest.magic.enabled.v1";
        readonly MagicPushGate gate = new MagicPushGate();
        WorldCard highlighted;
        readonly List<MagicFireball> effects = new List<MagicFireball>();
        Vector3 armedPalm;
        public bool Enabled { get; private set; }
        public bool Flying => effects.Exists(effect => effect);
        public WorldCard PracticeTarget;
        public string Hint { get; private set; } = L10n.T("Magie · rechte Hand öffnen");
        public MagicCasting() { Enabled = PlayerPrefs.GetInt(Preference, 0) == 1; }
        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
            PlayerPrefs.SetInt(Preference, enabled ? 1 : 0); PlayerPrefs.Save(); Cancel();
        }
        public void Cancel()
        {
            gate.Reset();
            if (highlighted) highlighted.SetMagicFocus(false);
            highlighted = null;
            foreach (var effect in effects) if (effect) Object.Destroy(effect.gameObject);
            effects.Clear();
            if (PracticeTarget) Object.Destroy(PracticeTarget.gameObject);
            PracticeTarget = null;
        }
        public bool CastAt(Vector3 origin,WorldCard target,Transform head,HomeAssistantConnection ha)
        {
            effects.RemoveAll(effect=>!effect);
            if(!Enabled||!target||!target.gameObject.activeInHierarchy||effects.Count>=4)return false;
            if(target!=PracticeTarget&&(!target.Interactable||target.ReadOnly||!ha.Switch(target.EntityId)))return false;
            effects.Add(MagicFireball.Launch(origin,target,head));target.Pulse();return true;
        }
        public void Tick(OVRHand hand, OVRSkeleton skeleton, Transform head,
            IEnumerable<WorldCard> markers, HomeAssistantConnection ha, bool allowed)
        {
            if (highlighted) highlighted.SetMagicFocus(false);
            highlighted = null;
            effects.RemoveAll(effect => !effect);
            if (!Enabled || !allowed || !hand || !hand.IsTracked ||
                hand.IsSystemGestureInProgress || hand.HandConfidence != OVRHand.TrackingConfidence.High || !skeleton || !skeleton.IsDataValid)
            { gate.Reset(); Hint = L10n.T("Magie · rechte Hand gut sichtbar ins Bild halten"); return; }
            if (!OpenPalm(skeleton, out var palm, out var normal))
            {
                Hint = L10n.T("Hand öffnen · Finger ausstrecken, Handfläche zum Ziel");
                gate.Step(null, true, false, default, default, Time.unscaledTime); return;
            }
            float best = Mathf.Cos(35*Mathf.Deg2Rad);
            WorldCard target = null;
            foreach (var card in PracticeTarget ? markers.Concat(new[] { PracticeTarget }) : markers)
            {
                if (effects.Count >= 4 || !card || !card.gameObject.activeInHierarchy || !card.Interactable || card.ReadOnly) continue;
                if (card != PracticeTarget && (!ha.Fresh || ha.Busy || !ha.Entities.TryGetValue(card.EntityId, out var entity) || !EntityPolicy.CanSwitch(entity.Id, entity.State))) continue;
                var to = card.transform.position-palm;
                if (to.magnitude < 0.2f || to.magnitude > 5 || Vector3.Dot(head.forward, (card.transform.position-head.position).normalized) < 0.65f) continue;
                float alignment = Vector3.Dot(normal, to.normalized);
                if (alignment > best) { best = alignment; target = card; }
            }
            var relative = palm-head.position;
            var toward = target ? (target.transform.position-palm).normalized : normal;
            bool wasArmed = gate.Armed;
            bool cast = gate.Step(target ? target.EntityId : null, true, true,
                new PushVector(relative.x, relative.y, relative.z), new PushVector(toward.x, toward.y, toward.z), Time.unscaledTime);
            if (!wasArmed && gate.Armed) armedPalm = palm;
            if (target) { target.SetMagicFocus(true, gate.Armed); highlighted = target; }
            Hint = !target ? L10n.T("Handfläche auf ein Lampen-/Schalter-Icon richten") : gate.Armed ? L10n.T("Bereit · jetzt etwa 12 cm nach vorne stoßen") : L10n.T("Ziel erkannt · Hand kurz ruhig halten");
            if (cast && target && Vector3.Dot(palm-armedPalm, toward) >= 0.10f && (target == PracticeTarget || ha.Switch(target.EntityId)))
            {
                effects.Add(MagicFireball.Launch(palm + toward*0.04f, target, head));
                Hint = L10n.T("Ausgelöst · Hand zurücknehmen oder kurz schließen");
            }
        }

        public static Transform Bone(OVRSkeleton skeleton, OVRSkeleton.BoneId legacy, OVRSkeleton.BoneId xr)
        {
            if (!skeleton || skeleton.Bones == null) return null;
            var type = skeleton.GetSkeletonType();
            // OVR and OpenXR bone enums overlap numerically; never match both at once.
            var id = type == OVRSkeleton.SkeletonType.XRHandRight || type == OVRSkeleton.SkeletonType.XRHandLeft ? xr : legacy;
            foreach (var bone in skeleton.Bones) if (bone.Id == id) return bone.Transform;
            return null;
        }
        static bool Extended(Transform root, Transform joint, Transform tip)
        {
            return root && joint && tip && Vector3.Distance(root.position, tip.position) > 0.035f &&
                Vector3.Dot((joint.position-root.position).normalized, (tip.position-joint.position).normalized) > 0.75f;
        }
        public static bool IndexExtended(OVRSkeleton skeleton) => skeleton && skeleton.IsDataValid && Extended(
            Bone(skeleton, OVRSkeleton.BoneId.Hand_Index1, OVRSkeleton.BoneId.XRHand_IndexProximal),
            Bone(skeleton, OVRSkeleton.BoneId.Hand_Index2, OVRSkeleton.BoneId.XRHand_IndexIntermediate),
            Bone(skeleton, OVRSkeleton.BoneId.Hand_IndexTip, OVRSkeleton.BoneId.XRHand_IndexTip));
        public static bool OpenPalm(OVRSkeleton skeleton, out Vector3 palm, out Vector3 normal)
        {
            palm = normal = Vector3.zero;
            if (!skeleton || !skeleton.IsDataValid || skeleton.Bones == null) return false;
            var wrist = Bone(skeleton, OVRSkeleton.BoneId.Hand_WristRoot, OVRSkeleton.BoneId.XRHand_Wrist);
            var index = Bone(skeleton, OVRSkeleton.BoneId.Hand_Index1, OVRSkeleton.BoneId.XRHand_IndexProximal);
            var middle = Bone(skeleton, OVRSkeleton.BoneId.Hand_Middle1, OVRSkeleton.BoneId.XRHand_MiddleProximal);
            var ring = Bone(skeleton, OVRSkeleton.BoneId.Hand_Ring1, OVRSkeleton.BoneId.XRHand_RingProximal);
            var pinky = Bone(skeleton, OVRSkeleton.BoneId.Hand_Pinky1, OVRSkeleton.BoneId.XRHand_LittleProximal);
            if (!wrist || !index || !middle || !pinky) return false;
            if (!Extended(index, Bone(skeleton, OVRSkeleton.BoneId.Hand_Index2, OVRSkeleton.BoneId.XRHand_IndexIntermediate), Bone(skeleton, OVRSkeleton.BoneId.Hand_IndexTip, OVRSkeleton.BoneId.XRHand_IndexTip)) ||
                !Extended(middle, Bone(skeleton, OVRSkeleton.BoneId.Hand_Middle2, OVRSkeleton.BoneId.XRHand_MiddleIntermediate), Bone(skeleton, OVRSkeleton.BoneId.Hand_MiddleTip, OVRSkeleton.BoneId.XRHand_MiddleTip)) ||
                !Extended(ring, Bone(skeleton, OVRSkeleton.BoneId.Hand_Ring2, OVRSkeleton.BoneId.XRHand_RingIntermediate), Bone(skeleton, OVRSkeleton.BoneId.Hand_RingTip, OVRSkeleton.BoneId.XRHand_RingTip))) return false;
            palm = (wrist.position+middle.position)*0.5f;
            normal = Vector3.Cross(pinky.position-index.position, middle.position-wrist.position).normalized;
            return normal.sqrMagnitude > 0.9f;
        }
    }
}

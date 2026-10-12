using System;
using Meta.XR.MRUtilityKit;
using UnityEngine;

namespace HAgoesQuest
{
    /// <summary>Admits positions only inside a validated current physical room.</summary>
    /// <remarks>HA area IDs are not physical room IDs and are never used as geometry.</remarks>
    public sealed class RoomScope
    {
        Func<Vector3,bool> contains;
        public string Id { get; private set; }
        public bool HasRoom=>!string.IsNullOrEmpty(Id);
        /// <summary>Validates containment of the head and returns whether the accepted room changed.</summary>
        public bool Update(string id,Vector3 head,Func<Vector3,bool> test)
        {
            string next=!string.IsNullOrEmpty(id)&&test!=null&&test(head)?id:null;
            bool changed=Id!=next;Id=next;contains=next==null?null:test;return changed;
        }
        public bool Contains(Vector3 position)=>HasRoom&&contains!=null&&contains(position);
    }

    public sealed partial class QuestHome
    {
        readonly RoomScope roomScope=new RoomScope();
        MRUKRoom scopedRoom;
        Func<Vector3,bool> scopedContains;
        bool ScopeBypassed=>ha!=null&&(ha.Demo||(Application.isEditor&&!roomScope.HasRoom));
        void RefreshRoomScope()
        {
            MRUKRoom room=null;
            if(MRUK.Instance)
            {
                var reported=MRUK.Instance.GetCurrentRoom();
                // Do not trust a stale/nearest-room fallback when the headset left its bounds.
                if(reported&&reported.IsPositionInRoom(head.position))room=reported;
                else foreach(var candidate in MRUK.Instance.Rooms)
                    if(candidate&&candidate.IsPositionInRoom(head.position)){room=candidate;break;}
            }
            if(scopedRoom!=room){scopedRoom=room;scopedContains=room?(Func<Vector3,bool>)(p=>room.IsPositionInRoom(p)):null;}
            if(roomScope.Update(room?room.Anchor.Uuid.ToString():null,head.position,scopedContains))
            {
                ResetGaze();sensorGaze=null;levelDragging=false;entityList?.CancelDrag();
                magic?.Cancel();
                bool wasPlacing=placing!=null;
                if(wasPlacing)CancelPlacement();
                if(wasPlacing||(levelControl&&menu)){Home();notice=Words("Raum gewechselt · Aktion abgebrochen.","Room changed · action cancelled.");}
            }
        }
        bool PointInCurrentRoom(Vector3 position)=>ScopeBypassed||roomScope.Contains(position);
        bool MarkerVisibleHere(string id,WorldCard card)
            =>card&&!scanBusy&&(Application.isEditor||bindings.CanShow(id))&&PointInCurrentRoom(card.transform.position);
        bool EntityAllowedHere(string id)
        {
            if(ScopeBypassed)return true;
            return !string.IsNullOrEmpty(id)&&markers.TryGetValue(id,out var card)&&MarkerVisibleHere(id,card)&&roomScope.Contains(head.position);
        }
    }
}

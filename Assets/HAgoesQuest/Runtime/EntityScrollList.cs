using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HAgoesQuest
{
    /// <summary>Recycles entity rows while separating scroll gestures from intentional selection.</summary>
    public sealed class EntityScrollList : MonoBehaviour
    {
        HaEntity[] entities;
        HomeAssistantConnection connection;
        Action<HaEntity> select;
        readonly List<WorldCard> rows = new List<WorldCard>();
        float scroll, startScroll, startY;
        bool wasDown, dragging, touching;
        WorldCard pressed;
        string pressedId;
        Transform viewport;
        public int Count => entities?.Length ?? 0;
        public float Offset => scroll;
        public void Initialize(HaEntity[] values, HomeAssistantConnection ha, Action<HaEntity> onSelect)
        {
            entities=values; connection=ha; select=onSelect;
            var host = new GameObject("Scrollable entities",typeof(RectTransform),typeof(Canvas),typeof(RectMask2D));
            viewport=host.transform; viewport.SetParent(transform,false);viewport.localPosition=new Vector3(0,-0.03f,0);
            viewport.localScale=Vector3.one*0.001f;
            host.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
            host.GetComponent<RectTransform>().sizeDelta=new Vector2(820,470);
            // Ten recycled rows: eight fully visible, with clipped content hinting at scrolling.
            for(int i=0;i<10;i++)
            {
                var card=WorldCard.Create(viewport,Vector3.zero,new Vector2(392,94),"");
                card.IsEntityRow=true; card.UnitScale=1; card.Label.fontSize=19; rows.Add(card);
            }
            RefreshRows();
        }
        public void ScrollBy(float meters) { scroll=Mathf.Clamp(scroll+meters,0,Mathf.Max(0,Mathf.Ceil(Count/2f)*0.106f-0.47f)); RefreshRows(); }
        void Update() { RefreshRows(); }
        void RefreshRows()
        {
            if (entities == null) return;
            int first=Mathf.FloorToInt(scroll/0.106f);
            for(int i=0;i<rows.Count;i++)
            {
                var card=rows[i]; int index=first*2+i;
                bool exists=index<Count; card.gameObject.SetActive(exists);if(!exists)continue;
                var entity=entities[index]; if(connection.Entities.TryGetValue(entity.Id,out var fresh))entity=fresh;
                float y=0.186f-(i/2)*0.106f+(scroll-first*0.106f);
                card.transform.localPosition=new Vector3(i%2==0?-206:206,y*1000,-1);
                // WorldCard animates in meters; the viewport uses pixel coordinates.
                card.transform.localScale=Vector3.one;
                card.GetComponent<Collider>().enabled=Mathf.Abs(y)<0.188f;
                card.EntityId=entity.Id; var captured=entity; card.Press=()=>select(captured);
                card.SetIcon(IconLibrary.ForEntity(entity));
                card.Label.text=Trim(entity.Name,24)+"\n"+Trim(entity.DisplayValue,23)+" · "+Trim(entity.AreaName,16);
                card.Interactable=true;
            }
        }
        static string Trim(string text,int length) => text == null ? "" : text.Length>length?text.Substring(0,length-1)+"…":text;
        /// <summary>Updates scrolling and returns whether the list consumes the current input.</summary>
        /// <remarks>Toolbar poke handling must run first. A ray pinch takes priority over nearby touch.</remarks>
        public bool Handle(Ray ray,bool down,bool fingerValid,Vector3 finger)
        {
            var plane=new Plane(transform.forward,transform.position);
            bool hit=plane.Raycast(ray,out float distance) && distance>0 && distance<8;
            Vector3 local=hit?transform.InverseTransformPoint(ray.GetPoint(distance)):Vector3.one*100;
            Vector3 fingertip=transform.InverseTransformPoint(finger);
            bool near=fingerValid && !down && Mathf.Abs(fingertip.x)<0.41f && fingertip.y>-.265f && fingertip.y<.205f && fingertip.z>-.035f && fingertip.z<.012f;
            bool active=down || near;
            if(near){local=fingertip;touching=true;}
            bool inside=Mathf.Abs(local.x)<0.41f && local.y>-.265f && local.y<.205f;
            if(active&&!wasDown&&inside)
            {
                dragging=true;startY=local.y;startScroll=scroll;pressed=null;
                foreach(var card in rows) if(card.gameObject.activeInHierarchy && card.GetComponent<Collider>().enabled && card.GetComponent<Collider>().bounds.Contains(ray.GetPoint(distance))) {pressed=card;break;}
                if(near) foreach(var card in rows) if(card.gameObject.activeInHierarchy && card.GetComponent<Collider>().enabled && Vector3.Distance(card.transform.position,finger)<.065f) {pressed=card;break;}
                pressedId=pressed?pressed.EntityId:null;
            }
            bool consume=inside||dragging;
            if(dragging&&active&&(hit||near)) {scroll=startScroll;ScrollBy(local.y-startY);}
            if(dragging&&!active)
            {
                // Recycling may have replaced the pressed row's entity while the finger was held.
                dragging=false;if(Mathf.Abs(scroll-startScroll)<.012f&&pressed&&pressed.EntityId==pressedId)pressed.Activate();pressed=null;touching=false;
            }
            wasDown=active;
#if UNITY_EDITOR
            if(inside&&UnityEngine.InputSystem.Mouse.current!=null) ScrollBy(UnityEngine.InputSystem.Mouse.current.scroll.ReadValue().y*-.0004f);
#endif
            return consume;
        }
        public void CancelDrag(){dragging=wasDown=touching=false;pressed=null;}
    }
}

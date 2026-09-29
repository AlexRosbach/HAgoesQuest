using System;
using Oculus.Interaction;
using UnityEngine;

namespace HAgoesQuest
{
    // Root driver for the official ISDK ray and poke interactors. The Meta runtime
    // supplies aim/pinch tracking; the app only routes the resulting selection.
    public sealed class MetaInputBridge : MonoBehaviour, ISelector, IUpdateDriver
    {
        public event Action WhenSelected = delegate {};
        public event Action WhenUnselected = delegate {};
        public bool IsRootDriver { get; set; } = true;
        RayInteractor ray;
        PokeInteractor poke;
        Transform rayOrigin, tip;
        bool pressed, released, rayAllowed, pokeAllowed, delivered;
        int createdFrame;
        public WorldCard Hovered { get; private set; }
        public WorldCard RaySelected { get; private set; }
        public WorldCard PokeSelected { get; private set; }
        public Vector3 HitPoint { get; private set; }
        void Awake()
        {
            createdFrame=Time.frameCount;
            rayOrigin=new GameObject("ISDK ray origin").transform;rayOrigin.SetParent(transform,false);
            tip=new GameObject("ISDK index fingertip").transform;tip.SetParent(transform,false);
            ray=gameObject.AddComponent<RayInteractor>();ray.InjectAllRayInteractor(this,rayOrigin);ray.MaxRayLength=8;ray.IsRootDriver=false;
            poke=gameObject.AddComponent<PokeInteractor>();poke.InjectAllPokeInteractor(tip);poke.IsRootDriver=false;
        }
        public void Cancel(bool ready=false)
        {
            Hovered=RaySelected=PokeSelected=null;released=delivered=rayAllowed=pokeAllowed=false;
            released=ready;
            if(pressed){pressed=false;WhenUnselected();}
            if(ray)ray.Disable();if(poke)poke.Disable();
        }
        public void Step(Ray aim,bool down,bool allowed,bool fingerValid,Vector3 fingerPosition)
        {
            Hovered=RaySelected=PokeSelected=null;
            if(Time.frameCount<=createdFrame)return;
            rayAllowed=allowed;pokeAllowed=allowed&&fingerValid;
            rayOrigin.SetPositionAndRotation(aim.origin,Quaternion.LookRotation(aim.direction));tip.position=fingerPosition;
            if(!allowed)released=false;
            else if(!down){released=true;delivered=false;}
            bool select=allowed&&released&&down;
            if(select!=pressed){pressed=select;if(pressed)WhenSelected();else WhenUnselected();}
            Drive();
        }
        public void Drive()
        {
            var oldRay=ray.SelectedInteractable;var oldPoke=poke.SelectedInteractable;
            if(rayAllowed)ray.Drive();else ray.Disable();
            if(pokeAllowed)poke.Drive();else poke.Disable();
            Hovered=ray.Interactable?ray.Interactable.GetComponent<WorldCard>():null;
            HitPoint=ray.CollisionInfo?.Point??ray.End;
            if(ray.SelectedInteractable&&ray.SelectedInteractable!=oldRay&&!delivered){RaySelected=ray.SelectedInteractable.GetComponent<WorldCard>();delivered=true;}
            if(poke.SelectedInteractable&&poke.SelectedInteractable!=oldPoke)PokeSelected=poke.SelectedInteractable.GetComponent<WorldCard>();
        }
    }
}

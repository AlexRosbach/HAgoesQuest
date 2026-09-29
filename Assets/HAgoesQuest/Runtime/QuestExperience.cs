using System.Collections;
using System.Linq;
using HAgoesQuest.Core;
using UnityEngine;

namespace HAgoesQuest
{
    public sealed partial class QuestHome
    {
        LineRenderer beamGlow;
        EntityScrollList entityList;
        WorldCard wristWatch, magicHint, levelControl;
        OVRSkeleton leftSkeleton;
        Ray smoothedRay;
        bool rayReady, previousPinch, levelDragging, levelTouch, touchArmed;
        float pinchStarted, touchNext;
        WorldCard touchingCard;
        string levelId;
        float draftLevel;
        UnityEngine.UI.Image levelKnob;
        MetaInputBridge metaInput;
        Texture2D beamTexture;

        void InitializeExperience()
        {
            if (!GetComponent<InteractionFeedback>()) gameObject.AddComponent<InteractionFeedback>();
            metaInput=gameObject.AddComponent<MetaInputBridge>();
            leftSkeleton=LeftHand?LeftHand.GetComponent<OVRSkeleton>():null;
            beamGlow=new GameObject("Soft ray glow").AddComponent<LineRenderer>();
            beamGlow.material=new Material(Shader.Find("Sprites/Default"));beamGlow.useWorldSpace=true;beamGlow.positionCount=2;beamGlow.enabled=false;
            beamTexture=new Texture2D(1,64,TextureFormat.RGBA32,false);beamTexture.wrapMode=TextureWrapMode.Clamp;
            for(int y=0;y<64;y++){float edge=(y/63f-.5f)*2;beamTexture.SetPixel(0,y,new Color(1,1,1,Mathf.Exp(-edge*edge*7)));}
            beamTexture.Apply(false,true);beamGlow.material.mainTexture=beamTexture;
            wristWatch=WorldCard.Create(null,Vector3.zero,new Vector2(90,90),"",ToggleMenu);
            wristWatch.MakeCompactMarker();wristWatch.UnitScale=.0007f;wristWatch.IsMarker=false;
            wristWatch.Label.rectTransform.anchoredPosition=Vector2.zero;
            wristWatch.Label.rectTransform.anchorMin=wristWatch.Label.rectTransform.anchorMax=Vector2.one*.5f;
            wristWatch.Label.rectTransform.pivot=Vector2.one*.5f;wristWatch.Label.rectTransform.sizeDelta=new Vector2(85,75);wristWatch.Label.fontSize=22;wristWatch.Label.color=Color.white;
            wristWatch.gameObject.AddComponent<WristDial>().Initialize(wristWatch);
            wristWatch.gameObject.SetActive(false);
            StartCoroutine(StartupAnimation());
        }
        IEnumerator StartupAnimation()
        {
            var splash=WorldCard.Create(null,head.position+head.forward*.85f,new Vector2(400,120),"HA goes Quest");
            splash.Interactable=false;splash.GetComponent<Collider>().enabled=false;splash.SetIcon("automation");
            var halo=new GameObject("Startup halo").AddComponent<LineRenderer>();halo.material=new Material(Shader.Find("Sprites/Default"));halo.useWorldSpace=true;halo.positionCount=65;halo.startWidth=halo.endWidth=.002f;
            for(float elapsed=0;elapsed<1.1f;elapsed+=Time.unscaledDeltaTime)
            {
                float progress=Mathf.Clamp01(elapsed/1.1f);splash.Face(head);splash.transform.position=head.position+head.forward*.85f+head.up*(.1f+progress*.05f);
                halo.startColor=halo.endColor=new Color(.73f,.64f,1,Mathf.Sin(progress*Mathf.PI)*.65f);
                for(int i=0;i<65;i++){float angle=i*Mathf.PI/32+progress;halo.SetPosition(i,splash.transform.position+(head.right*Mathf.Cos(angle)+head.up*Mathf.Sin(angle))*Mathf.Lerp(.04f,.27f,progress));}
                yield return null;
            }
            Destroy(halo.material);Destroy(halo.gameObject);
            Destroy(splash.gameObject);
        }
        void UpdateExperience()
        {
            if(wristWatch)
            {
                var wrist=MagicCasting.Bone(WatchSkeleton,OVRSkeleton.BoneId.Hand_WristRoot,OVRSkeleton.BoneId.XRHand_Wrist);
                bool visible=(ExperienceSettings.Wrist||watchEditing)&&HandReady(MenuHand)&&wrist;
                wristWatch.Interactable=!watchEditing;
                wristWatch.gameObject.SetActive(visible);
                if(visible)
                {
                    var saved=watchEditing?watchDraft:SavedWatch();
                    if(saved!=null)saved.Apply(wrist,wristWatch);else DefaultWatchPose(wrist);
                    wristWatch.Label.text=System.DateTime.Now.ToString("HH:mm");
                    if(watchEditing&&watchSizeLabel)watchSizeLabel.Label.text=Words("Größe · ","Size · ")+Mathf.RoundToInt(watchDraft.scale*100)+" %";
                    wristWatch.GetComponent<WristDial>()?.SetConnection(ha.Fresh);
                }
            }
        }
        bool LookingAt(WorldCard card)
        {
            // Quest 3 has no eye tracking. The center of the head view is the gaze proxy.
            return card==sensorGaze;
        }
        Ray SmoothAim(Ray source,bool pinching)
        {
            if(!rayReady){smoothedRay=source;rayReady=true;}
            if(pinching&&!previousPinch)pinchStarted=Time.unscaledTime;
            if(!pinching||Time.unscaledTime-pinchStarted>.10f)
            {
                float amount=1-Mathf.Exp(-24*Time.unscaledDeltaTime);
                smoothedRay=new Ray(Vector3.Lerp(smoothedRay.origin,source.origin,amount),Vector3.Slerp(smoothedRay.direction,source.direction,amount));
            }
            previousPinch=pinching;return smoothedRay;
        }
        void UpdateBeam(Vector3 start,Vector3 end,bool entityHit)
        {
            if(!beamGlow)return;
            var color=entityHit?new Color(.84f,.74f,1,1):new Color(.75f,.78f,1,.6f);
            pointer.startColor=pointer.endColor=color;pointer.startWidth=entityHit?.0032f:.0018f;pointer.endWidth=pointer.startWidth*.65f;
            beamGlow.enabled=true;beamGlow.SetPosition(0,start);beamGlow.SetPosition(1,end);
            beamGlow.startWidth=entityHit?.018f:.009f;beamGlow.endWidth=beamGlow.startWidth*.6f;
            color.a=entityHit?.15f:.055f;beamGlow.startColor=beamGlow.endColor=color;
        }
        bool UpdateAlternativeInput(bool allowed,bool tracked,bool down,Ray ray)
        {
            var tip=MagicCasting.Bone(ControlSkeleton,OVRSkeleton.BoneId.Hand_IndexTip,OVRSkeleton.BoneId.XRHand_IndexTip);
            bool finger=allowed&&HandReady(ControlHand)&&tip;
            if(!allowed||!tracked){entityList?.CancelDrag();touchingCard=null;levelDragging=false;rayReady=false;touchArmed=false;return false;}
            if(levelControl&&menu)
            {
                var nearBounds=levelControl.GetComponent<Collider>().bounds;nearBounds.Expand(.015f);
                bool near=finger&&nearBounds.Contains(tip.position);
                if(!levelDragging&&((down&&focused==levelControl)||near)){levelDragging=true;levelTouch=near;}
                if(levelDragging)
                {
                    if(levelTouch&&!finger){levelDragging=false;return true;}
                    var plane=new Plane(menu.transform.forward,levelControl.transform.position);
                    if(levelTouch || plane.Raycast(ray,out _))
                    {
                        plane.Raycast(ray,out float distance);
                        float x=menu.transform.InverseTransformPoint(levelTouch?tip.position:ray.GetPoint(distance)).x;
                        draftLevel=Mathf.InverseLerp(-.3f,.3f,x);UpdateLevelLabel();
                    }
                    if(levelTouch?!near:!down){levelDragging=false;ha.SetLevel(levelId,draftLevel);}
                    return true;
                }
            }
            if(entityList&&entityList.Handle(ray,down,finger,finger?tip.position:Vector3.zero))return true;
            var candidate=metaInput?metaInput.PokeSelected:null;
            if(candidate&&!candidate.IsEntityRow&&candidate.Interactable&&Time.unscaledTime>=touchNext)
            {touchNext=Time.unscaledTime+.4f;candidate.Activate();return true;}
            return false;
        }
        void ExperienceMenu()
        {
            NewMenu(L10n.T("Einstellungen"));
            Button(0,L10n.T("Magie · Anleitung & Übungsziel"),MagicSettings,"automation");
            Button(1,Words("Uhr & Bedienhand","Watch & handedness"),WatchSettings,"settings");
            Button(2,ExperienceSettings.English?L10n.T("Language: English · Deutsch"):L10n.T("Sprache: Deutsch · English"),()=>{ExperienceSettings.English=!ExperienceSettings.English;ExperienceMenu();});
            Button(3,ExperienceSettings.Sound?L10n.T("Schaltgeräusche: an"):L10n.T("Schaltgeräusche: aus"),()=>{ExperienceSettings.Sound=!ExperienceSettings.Sound;ExperienceMenu();},"speaker");
            Button(4,ExperienceSettings.English?"About · privacy & licenses":"Über · Datenschutz & Lizenzen",AboutMenu,"sensor");
            Button(5,L10n.T("Zurück"),Home);
            notice=Words("Entität ansehen + pinchen. Faust drehen: regeln. Menüs: Zeigen oder antippen. Die Uhr öffnet ein ortsfestes Panel.","Look at entity + pinch. Twist fist to adjust. Menus: point or tap. The watch opens a stationary panel.");
        }
        void StartMagicPractice()
        {
            magic.SetEnabled(true);CloseMenu();
            if(magic.PracticeTarget)Destroy(magic.PracticeTarget.gameObject);
            var target=WorldCard.Create(null,head.position+head.forward*1.3f,new Vector2(100,100),L10n.T("Übungsziel"));
            target.EntityId="practice.target";target.MakeCompactMarker();target.SetIcon("automation");target.Face(head);target.SetNearby(true);
            magic.PracticeTarget=target;
        }
        void OpenLevelControls(HaEntity entity)
        {
            NewMenu(entity.Name);levelId=entity.Id;draftLevel=entity.Level;levelDragging=false;
            levelControl=WorldCard.Create(menu.transform,new Vector3(0,.06f,0),new Vector2(700,105),"");
            levelControl.Press=()=>{};levelControl.SetIcon(entity.CanDim?"sun":"speaker");UpdateLevelLabel();
            var track=new GameObject("Slider track",typeof(RectTransform),typeof(UnityEngine.UI.Image));track.transform.SetParent(levelControl.transform,false);
            var trackImage=track.GetComponent<UnityEngine.UI.Image>();trackImage.raycastTarget=false;trackImage.color=new Color(.45f,.39f,.65f);
            trackImage.rectTransform.sizeDelta=new Vector2(600,6);trackImage.rectTransform.anchoredPosition=new Vector2(0,-30);
            var knob=new GameObject("Slider knob",typeof(RectTransform),typeof(UnityEngine.UI.Image));knob.transform.SetParent(levelControl.transform,false);
            levelKnob=knob.GetComponent<UnityEngine.UI.Image>();levelKnob.raycastTarget=false;levelKnob.rectTransform.sizeDelta=new Vector2(20,28);UpdateLevelLabel();
            Button(4,L10n.T("Zurück"),Home);Button(5,L10n.T("Menü schließen"),CloseMenu);
            notice=L10n.T("Auf den Regler zeigen, pinchen und halten. Seitlich ziehen; beim Loslassen wird der Wert gesendet.");
        }
        void UpdateLevelLabel()
        {
            if(levelControl)levelControl.Label.text=(ha.Entities.TryGetValue(levelId,out var entity)&&entity.CanDim?L10n.T("Helligkeit"):L10n.T("Lautstärke"))+"  "+Mathf.RoundToInt(draftLevel*100)+" %";
            if(levelKnob)levelKnob.rectTransform.anchoredPosition=new Vector2(Mathf.Lerp(-300,300,draftLevel),-30);
        }
        void AboutMenu()
        {
            NewMenu("HA goes Quest · 0.6.1");
            var info=WorldCard.Create(menu.transform,new Vector3(0,.12f,0),new Vector2(720,300),ExperienceSettings.English?
                "Independent development app. Not affiliated with Meta or Home Assistant.\n\nCredentials are encrypted on this Quest. Device commands and media go to your configured HA server. QR camera frames are processed locally. Spatial tracking uses Meta platform services.\n\nThird-party licenses are included below.":
                "Unabhängige Entwicklungs-App. Keine offizielle App von Meta oder Home Assistant.\n\nAnmeldung verschlüsselt auf dieser Quest. Befehle und Medien gehen an deinen HA-Server. QR-Kamerabilder werden lokal verarbeitet. Raumtracking nutzt Meta-Plattformdienste.\n\nDrittanbieter-Lizenzen sind unten enthalten.");
            info.Label.fontSize=20;info.Interactable=false;info.GetComponent<Collider>().enabled=false;
            Button(4,ExperienceSettings.English?"Third-party licenses":"Drittanbieter-Lizenzen",()=>LicensePage(0));
            Button(5,L10n.T("Zurück"),ExperienceMenu);
        }
        void LicensePage(int pageIndex)
        {
            string text=Resources.Load<TextAsset>("Legal/ThirdPartyNotices")?.text??"License file unavailable";
            const int pageSize=660;int total=Mathf.Max(1,Mathf.CeilToInt(text.Length/(float)pageSize));pageIndex=Mathf.Clamp(pageIndex,0,total-1);
            NewMenu((ExperienceSettings.English?"Licenses ":"Lizenzen ")+(pageIndex+1)+" / "+total);
            var body=WorldCard.Create(menu.transform,new Vector3(0,.08f,0),new Vector2(720,400),text.Substring(pageIndex*pageSize,Mathf.Min(pageSize,text.Length-pageIndex*pageSize)));
            body.Label.fontSize=17;body.Label.alignment=TextAnchor.UpperLeft;body.Interactable=false;body.GetComponent<Collider>().enabled=false;
            Pager(4,()=>LicensePage(pageIndex-1),()=>LicensePage(pageIndex+1));Button(5,L10n.T("Zurück"),AboutMenu);
        }
    }
}


using System;
using UnityEngine;
using UnityEngine.UI;
using Oculus.Interaction;
using Oculus.Interaction.Surfaces;

namespace HAgoesQuest
{
    public sealed class WorldCard : MonoBehaviour
    {
        public Action Press;
        public string EntityId;
        public bool IsMarker;
        public bool IsMedia;
        public bool IsEntityRow;
        public float UnitScale = 0.001f;
        public WorldCard Adjustment;
        public bool IsFocused => focused;
        float lastFocused = -100;
        RayInteractable sdkRay;
        PokeInteractable sdkPoke;
        BoundsClipper sdkBounds;
        public bool ReadOnly;
        public bool Interactable = true;
        public Text Label;
        Image background;
        Image icon;
        CanvasGroup group;
        bool focused;
        float shownAt, pulseAt = -100;
        float detailAlpha;
        bool showDetails;
        float proximity;
        Image ring;
        bool magicFocus, magicArmed;
        public void SetMagicFocus(bool active, bool armed = false) { magicFocus = active; magicArmed = armed; }
        static Sprite circle, circleRing;
        string lastState;
        static Sprite rounded;
        static readonly System.Collections.Generic.Dictionary<string, Sprite> icons = new System.Collections.Generic.Dictionary<string, Sprite>();
        Color idle = new Color(.20f,.25f,.34f,1);
        public enum Surface { Secondary, Primary, Header, Caption, Panel }
        public void SetSurface(Surface style)
        {
            idle=style==Surface.Primary?new Color(.30f,.24f,.67f,1):style==Surface.Panel?new Color(.105f,.14f,.205f,1):style==Surface.Header||style==Surface.Caption?new Color(.145f,.185f,.26f,1):new Color(.20f,.25f,.34f,1);
            if(style==Surface.Header){Label.fontSize=30;Label.fontStyle=FontStyle.Bold;Label.alignment=TextAnchor.MiddleLeft;background.enabled=false;GetComponent<Outline>().enabled=false;}
            if(style==Surface.Caption)Label.color=new Color(.94f,.96f,1);
            background.color=idle;
        }
        public void MakeCursor(){background.sprite=Circle(false);background.type=Image.Type.Simple;GetComponent<Outline>().enabled=false;idle=Color.white;}
        public string Key => GetInstanceID().ToString();

        public static WorldCard Create(Transform parent, Vector3 localPosition, Vector2 size, string label, Action press = null)
        {
            var go = new GameObject("Card", typeof(RectTransform), typeof(Canvas), typeof(Image), typeof(BoxCollider), typeof(CanvasGroup), typeof(WorldCard));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = Vector3.one * 0.001f;
            go.GetComponent<RectTransform>().sizeDelta = size;
            go.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            go.GetComponent<BoxCollider>().size = new Vector3(size.x, size.y, 8);
            var card = go.GetComponent<WorldCard>();
            card.Press = press;
            card.background = go.GetComponent<Image>();
            card.background.color = card.idle;
            card.background.sprite = Rounded();
            card.background.type = Image.Type.Sliced;
            card.group = go.GetComponent<CanvasGroup>();
            card.shownAt = Time.unscaledTime;
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0.62f, 0.64f, 0.8f, 0.42f);
            outline.effectDistance = new Vector2(1, -1);

            var text = new GameObject("Text", typeof(RectTransform), typeof(Text));
            text.transform.SetParent(go.transform, false);
            var rect = text.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(16, 8);
            rect.offsetMax = new Vector2(-16, -8);
            card.Label = text.GetComponent<Text>();
            card.Label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            card.Label.fontSize = 24;
            card.Label.alignment = TextAnchor.MiddleCenter;
            card.Label.color = Color.white;
            card.Label.supportRichText = false;
            card.Label.raycastTarget = false;
            card.Label.text = label;
            var surface=go.AddComponent<ColliderSurface>();surface.InjectAllColliderSurface(go.GetComponent<Collider>());
            card.sdkRay=go.AddComponent<RayInteractable>();card.sdkRay.InjectAllRayInteractable(surface);
            var plane=go.AddComponent<PlaneSurface>();
            card.sdkBounds=go.AddComponent<BoundsClipper>();card.sdkBounds.Size=new Vector3(size.x,size.y,1);
            var patch=go.AddComponent<ClippedPlaneSurface>();patch.InjectAllClippedPlaneSurface(plane,new[]{card.sdkBounds});
            card.sdkPoke=go.AddComponent<PokeInteractable>();card.sdkPoke.InjectAllPokeInteractable(patch);
            return card;
        }

        public void Focus(bool focused)
        {
            this.focused = focused && (Interactable || IsMarker);
            if (this.focused) lastFocused=Time.unscaledTime;
        }

        public void SetNearby(bool nearby) { showDetails = nearby; }
        public void SetDistance(float distance)
        {
            showDetails = distance <= 0.30f;
            proximity = 1 - Mathf.InverseLerp(0.3f, 1.2f, distance);
        }
        public void ShowSensorValue(bool gaze, float distance = 1)
        {
            if (gaze) showDetails = true;
            // Constant angular text size, independent of the icon's proximity animation.
            float parentScale=Mathf.Max(.0001f,transform.lossyScale.x);
            float target=Mathf.Clamp(distance,.45f,8)*.00115f/parentScale;
            Label.rectTransform.localScale=Vector3.one*target;
            Label.fontSize=28;
            Label.rectTransform.sizeDelta=new Vector2(420,150);
            Label.alignment=TextAnchor.LowerCenter;
            Label.rectTransform.pivot = new Vector2(0.5f, 0);
            Label.rectTransform.anchorMin = Label.rectTransform.anchorMax = new Vector2(0.5f, 1);
            Label.rectTransform.anchoredPosition = new Vector2(0, 16*target);
        }

        public void MakeCompactMarker()
        {
            IsMarker = true;
            GetComponent<RectTransform>().sizeDelta = new Vector2(90, 90);
            GetComponent<BoxCollider>().size = new Vector3(90, 90, 18);
            background.enabled = true;
            background.sprite = Circle(false);
            background.type = Image.Type.Simple;
            GetComponent<Outline>().enabled = false;
            var border = new GameObject("Menu style ring", typeof(RectTransform), typeof(Image));
            border.transform.SetParent(transform, false);
            ring = border.GetComponent<Image>();
            ring.raycastTarget = false;
            ring.sprite = Circle(true);
            ring.rectTransform.sizeDelta = new Vector2(90, 90);
            ring.color = new Color(.68f,.58f,1,.9f);
            if (icon)
            {
                icon.rectTransform.anchorMin = icon.rectTransform.anchorMax = Vector2.one * 0.5f;
                icon.rectTransform.anchoredPosition = Vector2.zero;
                icon.rectTransform.sizeDelta = new Vector2(68, 68);
            }
            Label.rectTransform.anchorMin = Label.rectTransform.anchorMax = new Vector2(0.5f, 0);
            Label.rectTransform.pivot = new Vector2(0.5f, 1);
            Label.rectTransform.sizeDelta = new Vector2(270, 85);
            Label.rectTransform.anchoredPosition = new Vector2(0, -8);
            Label.fontSize = 18;
            Label.alignment = TextAnchor.UpperCenter;
            Label.color = Color.clear;
        }

        public void Activate()
        {
            if (!Interactable) return;
            Pulse();
            if(!IsMarker&&Press!=null)InteractionFeedback.Instance?.PlayUI();
            Press?.Invoke();
        }

        public void Pulse() { pulseAt = Time.unscaledTime; }

        public void SetState(string state)
        {
            if (lastState != null && lastState != state) Pulse();
            lastState = state;
            if (icon) icon.color = state == "off" ? new Color(0.65f, 0.72f, 0.78f, 0.75f) : Color.white;
        }

        public void SetIcon(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            if (!icons.TryGetValue(name, out var sprite))
            {
                sprite = IconLibrary.Get(name);
                icons[name] = sprite;
            }
            if(icon && icon.sprite==sprite)return;
            if (!icon)
            {
                var go = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);
                icon = go.GetComponent<Image>();
                icon.raycastTarget = false;
                icon.preserveAspect = true;
            }
            icon.sprite = sprite;
            if(IsMarker&&!IsMedia)
            {
                icon.rectTransform.anchorMin=icon.rectTransform.anchorMax=Vector2.one*.5f;
                icon.rectTransform.anchoredPosition=Vector2.zero;icon.rectTransform.sizeDelta=new Vector2(68,68);
                return;
            }
            var ownRect = GetComponent<RectTransform>();
            float side = Mathf.Min(ownRect.sizeDelta.y - 14, 90);
            icon.rectTransform.anchorMin = icon.rectTransform.anchorMax = new Vector2(0, 0.5f);
            icon.rectTransform.anchoredPosition = new Vector2(side * 0.5f + 14, 0);
            icon.rectTransform.sizeDelta = Vector2.one * side;
            Label.rectTransform.offsetMin = new Vector2(side + 28, 8);
            Label.alignment = TextAnchor.MiddleLeft;
        }

        void Update()
        {
            if (!background || !group) return;
            bool canHit=GetComponent<Collider>().enabled;
            if(sdkRay)sdkRay.enabled=canHit&&(IsMarker||Press!=null);
            if(sdkPoke)sdkPoke.enabled=canHit&&Interactable&&Press!=null;
            if(sdkBounds){var rect=GetComponent<RectTransform>().sizeDelta;sdkBounds.Size=new Vector3(rect.x,rect.y,1);}
            if (Adjustment) Adjustment.gameObject.SetActive(Interactable && (focused || Adjustment.IsFocused || Time.unscaledTime-lastFocused<1.5f));
            float appear = Mathf.Clamp01((Time.unscaledTime - shownAt) / 0.24f);
            float ease = 1 - Mathf.Pow(1 - appear, 3);
            float pulse = Mathf.Clamp01(1 - (Time.unscaledTime - pulseAt) / 0.4f);
            float size = Mathf.Lerp(0.9f, 1, ease) * (focused ? 1.025f : 1) * (1 + Mathf.Sin(pulse * Mathf.PI) * 0.035f);
            if (IsMarker && !IsMedia) size *= Mathf.Lerp(1, 1.8f, proximity);
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * (size * UnitScale), 1 - Mathf.Exp(-18 * Time.unscaledDeltaTime));
            group.alpha = ease;
            if (IsMarker)
            {
                detailAlpha = Mathf.MoveTowards(detailAlpha, showDetails || IsMedia ? 1 : 0, Time.unscaledDeltaTime * 8);
                Label.color = new Color(1, 1, 1, detailAlpha);
                if (ring) ring.color = magicFocus ? new Color(0.12f, 1, 0.3f, magicArmed ? 1 : 0.5f) : focused ? new Color(0.78f, 0.72f, 1, 1) : new Color(0.55f, 0.54f, 0.75f, 0.85f);
                if(ring)ring.transform.localScale=Vector3.one*(magicArmed?1.04f+.035f*Mathf.Sin(Time.unscaledTime*5):1);
            }
            Color target = focused ? Color.Lerp(idle,new Color(.54f,.48f,.9f,1),.55f) : idle;
            target = Color.Lerp(target, new Color(0.43f, 0.32f, 0.7f, 1), pulse * 0.6f);
            background.color = Color.Lerp(background.color, target, 1 - Mathf.Exp(-14 * Time.unscaledDeltaTime));
        }

        static Sprite Circle(bool rim)
        {
            if (rim && circleRing) return circleRing;
            if (!rim && circle) return circle;
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float radius = new Vector2(x - 63.5f, y - 63.5f).magnitude;
                float alpha = Mathf.Clamp01(63.5f - radius);
                if (rim) alpha *= Mathf.Clamp01(radius - 59.5f);
                pixels[y * size + x] = new Color(1, 1, 1, alpha);
            }
            texture.SetPixels(pixels); texture.Apply(false, true);
            var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, 100);
            if (rim) circleRing = sprite; else circle = sprite;
            return sprite;
        }

        static Sprite Rounded()
        {
            if (rounded) return rounded;
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                var q = new Vector2(Mathf.Abs(x - 31.5f) - 15.5f, Mathf.Abs(y - 31.5f) - 15.5f);
                float distance = new Vector2(Mathf.Max(q.x, 0), Mathf.Max(q.y, 0)).magnitude - 16;
                pixels[y * size + x] = new Color(1, 1, 1, Mathf.Clamp01(0.5f - distance));
            }
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            rounded = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, 100, 0, SpriteMeshType.FullRect, Vector4.one * 20);
            return rounded;
        }

        public void Face(Transform head)
        {
            var direction = transform.position - head.position;
            if (direction.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
    }
}


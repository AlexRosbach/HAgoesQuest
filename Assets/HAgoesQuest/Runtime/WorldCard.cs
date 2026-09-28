using System;
using UnityEngine;
using UnityEngine.UI;

namespace HAgoesQuest
{
    public sealed class WorldCard : MonoBehaviour
    {
        public Action Press;
        public string EntityId;
        public bool IsMarker;
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
        readonly Color idle = new Color(0.025f, 0.045f, 0.075f, 0.94f);
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
            outline.effectColor = new Color(0.14f, 0.6f, 0.65f, 0.3f);
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
            return card;
        }

        public void Focus(bool focused)
        {
            this.focused = focused && (Interactable || IsMarker);
        }

        public void SetNearby(bool nearby) { showDetails = nearby; }
        public void SetDistance(float distance)
        {
            showDetails = distance <= 0.30f;
            proximity = 1 - Mathf.InverseLerp(0.3f, 1.2f, distance);
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
            ring.color = new Color(0.14f, 0.6f, 0.65f, 0.85f);
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
                var texture = Resources.Load<Texture2D>("Icons/" + name);
                if (!texture) return;
                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f, 100);
                icons[name] = sprite;
            }
            if (!icon)
            {
                var go = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);
                icon = go.GetComponent<Image>();
                icon.raycastTarget = false;
                icon.preserveAspect = true;
            }
            icon.sprite = sprite;
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
            float appear = Mathf.Clamp01((Time.unscaledTime - shownAt) / 0.24f);
            float ease = 1 - Mathf.Pow(1 - appear, 3);
            float pulse = Mathf.Clamp01(1 - (Time.unscaledTime - pulseAt) / 0.4f);
            float size = Mathf.Lerp(0.9f, 1, ease) * (focused ? 1.025f : 1) * (1 + Mathf.Sin(pulse * Mathf.PI) * 0.035f);
            if (IsMarker) size *= Mathf.Lerp(1, 1.8f, proximity);
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * (size * 0.001f), 1 - Mathf.Exp(-18 * Time.unscaledDeltaTime));
            group.alpha = ease;
            if (IsMarker)
            {
                detailAlpha = Mathf.MoveTowards(detailAlpha, showDetails ? 1 : 0, Time.unscaledDeltaTime * 8);
                Label.color = new Color(1, 1, 1, detailAlpha);
                if (ring) ring.color = magicFocus ? new Color(0.12f, 1, 0.3f, magicArmed ? 1 : 0.5f) : focused ? new Color(0.3f, 1, 0.9f, 1) : new Color(0.14f, 0.6f, 0.65f, 0.85f);
            }
            Color target = focused ? new Color(0.025f, 0.24f, 0.27f, 0.98f) : idle;
            target = Color.Lerp(target, new Color(0.1f, 0.58f, 0.48f, 1), pulse * 0.6f);
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

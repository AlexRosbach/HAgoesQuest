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
        public bool Interactable = true;
        public Text Label;
        Image background;
        readonly Color idle = new Color(0.035f, 0.075f, 0.105f, 0.96f);
        public string Key => GetInstanceID().ToString();

        public static WorldCard Create(Transform parent, Vector3 localPosition, Vector2 size, string label, Action press = null)
        {
            var go = new GameObject("Card", typeof(RectTransform), typeof(Canvas), typeof(Image), typeof(BoxCollider), typeof(WorldCard));
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
            background.color = focused && Interactable ? new Color(0.03f, 0.48f, 0.43f, 1) : idle;
        }

        public void Face(Transform head)
        {
            var direction = transform.position - head.position;
            if (direction.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
    }
}

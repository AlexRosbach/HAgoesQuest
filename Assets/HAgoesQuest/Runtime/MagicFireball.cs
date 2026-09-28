using UnityEngine;

namespace HAgoesQuest
{
    // Short-lived visual feedback only. Home Assistant remains the source of truth.
    public sealed class MagicFireball : MonoBehaviour
    {
        Transform target, head;
        Vector3 source, destination;
        SpriteRenderer[] flames;
        LineRenderer ring;
        Material material;
        float age, flight;
        bool impacted;
        static Sprite sprite;

        public static MagicFireball Launch(Vector3 source, WorldCard target, Transform head)
        {
            if (!sprite)
            {
                var texture = Resources.Load<Texture2D>("Effects/green-fireball");
                if (!texture) return null;
                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f, texture.width);
            }
            var effect = new GameObject("Emerald magic fireball").AddComponent<MagicFireball>();
            effect.source = source; effect.target = target.transform; effect.head = head;
            effect.destination = target.transform.position;
            effect.flight = Mathf.Clamp(Vector3.Distance(source, effect.destination)/4, 0.25f, 0.65f);
            effect.material = new Material(Shader.Find("Sprites/Default"));
            effect.flames = new SpriteRenderer[16];
            for (int i = 0; i < effect.flames.Length; i++)
            {
                var renderer = new GameObject("Emerald flame").AddComponent<SpriteRenderer>();
                renderer.transform.SetParent(effect.transform, false);
                renderer.sprite = sprite; renderer.sharedMaterial = effect.material;
                renderer.color = Color.clear; effect.flames[i] = renderer;
            }
            effect.ring = new GameObject("Magic impact ring").AddComponent<LineRenderer>();
            effect.ring.transform.SetParent(effect.transform, false);
            effect.ring.sharedMaterial = effect.material; effect.ring.useWorldSpace = true;
            effect.ring.loop = true; effect.ring.positionCount = 48;
            effect.ring.startWidth = effect.ring.endWidth = 0.005f;
            effect.ring.enabled = false;
            return effect;
        }

        void Update()
        {
            if (!head || !target || !target.gameObject.activeInHierarchy) { Destroy(gameObject); return; }
            age += Time.unscaledDeltaTime;
            destination = target.position;
            if (age < flight)
            {
                for (int i = 0; i < flames.Length; i++)
                {
                    float t = Mathf.Clamp01((age-i*0.012f)/flight);
                    var position = Vector3.Lerp(source, destination, t) + Vector3.up * (Mathf.Sin(t*Mathf.PI)*0.045f);
                    float taper = 1-i/(float)flames.Length;
                    Show(flames[i], position, (0.055f+0.07f*taper)*(1+0.1f*Mathf.Sin(age*30+i)), taper*0.65f, age*160+i*41);
                }
                return;
            }
            if (!impacted) { impacted = true; target.GetComponent<WorldCard>()?.Pulse(); }
            float burst = (age-flight)/0.35f;
            if (burst >= 1) { Destroy(gameObject); return; }
            Vector3 normal = (head.position-destination).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, normal).normalized;
            if (right.sqrMagnitude < 0.01f) right = head.right;
            Vector3 up = Vector3.Cross(normal, right);
            ring.enabled = true;
            ring.startColor = ring.endColor = new Color(0.15f, 1, 0.45f, 1-burst);
            for (int i = 0; i < 48; i++)
            {
                float angle = i*Mathf.PI*2/48;
                ring.SetPosition(i, destination + (right*Mathf.Cos(angle)+up*Mathf.Sin(angle))*(0.035f+burst*0.13f));
            }
            for (int i = 0; i < flames.Length; i++)
            {
                float angle = i*2.39996f;
                var position = destination + (right*Mathf.Cos(angle)+up*Mathf.Sin(angle))*burst*(0.08f+i*0.004f);
                Show(flames[i], position, Mathf.Lerp(0.065f, 0.008f, burst), (1-burst)*0.8f, age*180+i*37);
            }
        }

        void Show(SpriteRenderer renderer, Vector3 position, float size, float alpha, float spin)
        {
            renderer.transform.position = position;
            Vector3 facing = position-head.position;
            if (facing.sqrMagnitude < 0.0001f) facing = head.forward;
            renderer.transform.rotation = Quaternion.LookRotation(facing, head.up)*Quaternion.Euler(0, 0, spin);
            renderer.transform.localScale = Vector3.one*size;
            renderer.color = new Color(1, 1, 1, alpha);
        }
        void OnDestroy() { if (material) Destroy(material); }
    }
}

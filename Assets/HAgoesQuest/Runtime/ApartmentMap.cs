using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.UI;

namespace HAgoesQuest
{
    public sealed class ApartmentMap : MaskableGraphic
    {
        public Transform Head;
        public SpatialBindings Bindings;
        public IReadOnlyList<Vector3> Trail;
        float nextDraw;
        readonly List<List<Vector2>> outlines = new List<List<Vector2>>();
        Vector2 center;
        float scale;

        void Update()
        {
            if (Time.unscaledTime < nextDraw) return;
            nextDraw = Time.unscaledTime + 0.15f;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (!Head || MRUK.Instance == null) return;
            outlines.Clear();
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue), max = new Vector2(float.MinValue, float.MinValue);
            foreach (var room in MRUK.Instance.Rooms)
            foreach (var floor in room.FloorAnchors)
            {
                var points = new List<Vector2>();
                if (floor.PlaneBoundary2D == null) continue;
                foreach (var point in floor.PlaneBoundary2D)
                {
                    Vector3 world = floor.transform.TransformPoint(new Vector3(point.x, point.y, 0));
                    var xz = new Vector2(world.x, world.z);
                    points.Add(xz);
                    min = Vector2.Min(min, xz); max = Vector2.Max(max, xz);
                }
                if (points.Count > 2) outlines.Add(points);
            }
            if (outlines.Count == 0) return;
            Vector2 head = new Vector2(Head.position.x, Head.position.z);
            min = Vector2.Min(min, head); max = Vector2.Max(max, head);
            center = (min + max) * 0.5f;
            Vector2 span = max - min;
            Rect rect = rectTransform.rect;
            scale = Mathf.Min((rect.width - 40) / Mathf.Max(span.x, 1), (rect.height - 45) / Mathf.Max(span.y, 1));
            foreach (var points in outlines)
                for (int i = 0; i < points.Count; i++) Line(mesh, Project(points[i]), Project(points[(i + 1) % points.Count]), 3, new Color(0.2f, 0.9f, 0.78f));
            if (Trail != null)
                for (int i = 1; i < Trail.Count; i++)
                {
                    var a = new Vector2(Trail[i - 1].x, Trail[i - 1].z);
                    var b = new Vector2(Trail[i].x, Trail[i].z);
                    // Ignore points outside this scene's bounds (e.g. after recentering).
                    if (a.x < min.x || a.x > max.x || a.y < min.y || a.y > max.y || b.x < min.x || b.x > max.x || b.y < min.y || b.y > max.y) continue;
                    Line(mesh, Project(a), Project(b), 1.5f, new Color(0.3f, 0.6f, 0.8f, 0.55f));
                }
            if (Bindings)
                foreach (var anchor in Bindings.Loaded.Values)
                    if (anchor && anchor.Localized) Dot(mesh, Project(new Vector2(anchor.transform.position.x, anchor.transform.position.z)), 5, new Color(1, 0.8f, 0.35f));
            Vector2 position = Project(head);
            Dot(mesh, position, 6, Color.white);
            Vector2 direction = new Vector2(Head.forward.x, Head.forward.z).normalized;
            Line(mesh, position, position + direction * 19, 3, Color.white);
        }

        Vector2 Project(Vector2 point) => (point - center) * scale + rectTransform.rect.center;

        static void Dot(VertexHelper mesh, Vector2 point, float radius, Color tint)
        {
            const int steps = 16;
            for (int i = 0; i < steps; i++)
            {
                float a = i * Mathf.PI * 2 / steps, b = (i + 1) * Mathf.PI * 2 / steps;
                int start = mesh.currentVertCount;
                mesh.AddVert(point, tint, Vector2.zero);
                mesh.AddVert(point + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius, tint, Vector2.zero);
                mesh.AddVert(point + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * radius, tint, Vector2.zero);
                mesh.AddTriangle(start, start + 1, start + 2);
            }
        }

        static void Line(VertexHelper mesh, Vector2 a, Vector2 b, float width, Color tint)
        {
            if ((a - b).sqrMagnitude < 0.001f) return;
            Vector2 delta = (b - a).normalized;
            Vector2 normal = new Vector2(-delta.y, delta.x) * width * 0.5f;
            int start = mesh.currentVertCount;
            mesh.AddVert(a - normal, tint, Vector2.zero);
            mesh.AddVert(a + normal, tint, Vector2.zero);
            mesh.AddVert(b + normal, tint, Vector2.zero);
            mesh.AddVert(b - normal, tint, Vector2.zero);
            mesh.AddTriangle(start, start + 1, start + 2);
            mesh.AddTriangle(start, start + 2, start + 3);
        }
    }
}

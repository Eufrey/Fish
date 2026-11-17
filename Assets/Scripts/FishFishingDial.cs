using UnityEngine;
using UnityEngine.UI;

namespace Fish
{
    // Draws the existing normalized timing position. Owns no input or catch rules.
    public sealed class FishFishingDial : MaskableGraphic
    {
        private float position;

        public void ShowPosition(float value)
        {
            position = value;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Vector2 center = rectTransform.rect.center;
            float radius = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * .46f;
            Ring(mesh, center + Vector2.down * 7, radius, 0, 0, 1, new Color(0, .05f, .03f, .4f));
            Ring(mesh, center, radius, 0, 0, 1, new Color32(140, 92, 46, 255));
            Ring(mesh, center + Vector2.up * 3, radius * .96f, 0, 0, 1, new Color32(221, 172, 109, 255));
            Ring(mesh, center, radius * .87f, 0, 0, 1, new Color32(255, 245, 218, 255));
            Ring(mesh, center, radius * 1.03f, radius * .87f,
                FishTimingAttempt.TargetStart, FishTimingAttempt.TargetEnd, new Color32(190, 240, 120, 255));
            Vector2 direction = Direction(position);
            Vector2 side = new Vector2(-direction.y, direction.x) * 7;
            int index = mesh.currentVertCount;
            mesh.AddVert(center + side, FishUITheme.Ink, Vector2.zero);
            mesh.AddVert(center - side, FishUITheme.Ink, Vector2.zero);
            mesh.AddVert(center + direction * radius * .83f, FishUITheme.Ink, Vector2.zero);
            mesh.AddTriangle(index, index + 1, index + 2);
            Ring(mesh, center, 13, 0, 0, 1, new Color32(140, 92, 46, 255));
        }

        private static Vector2 Direction(float progress)
        {
            float angle = progress * Mathf.PI * 2;
            return new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
        }

        private static void Ring(VertexHelper mesh, Vector2 center, float outer, float inner,
            float start, float end, Color tint)
        {
            int segments = Mathf.CeilToInt((end - start) * 120);
            for (int i = 0; i < segments; i++)
            {
                Vector2 a = Direction(Mathf.Lerp(start, end, (float)i / segments));
                Vector2 b = Direction(Mathf.Lerp(start, end, (float)(i + 1) / segments));
                int index = mesh.currentVertCount;
                mesh.AddVert(center + a * inner, tint, Vector2.zero);
                mesh.AddVert(center + a * outer, tint, Vector2.zero);
                mesh.AddVert(center + b * outer, tint, Vector2.zero);
                mesh.AddVert(center + b * inner, tint, Vector2.zero);
                mesh.AddTriangle(index, index + 1, index + 2);
                mesh.AddTriangle(index, index + 2, index + 3);
            }
        }
    }
}

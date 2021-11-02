using UnityEngine;
using UnityEngine.UI;

namespace Pigeon
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class PolygonOutline : OutlineGraphic
    {
        [Space(10)]
        [Min(3)] public int sides = 8;

        public void SetSides(int value)
        {
            if (value < 3)
            {
                value = 3;
            }
            sides = value;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            float width = rectTransform.rect.width / 2f;
            float height = rectTransform.rect.height / 2f;
            Vector2 center = rectTransform.rect.center;

            int vertexCount = sides * 2;
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;

            //Create vertices
            for (int i = 0; i < sides; i++)
            {
                float t = (1f - i / (float)sides) * Mathf.PI * 2f;
                float xCirclePoint = Mathf.Cos(t);
                float yCirclePoint = Mathf.Sin(t);

                vertex.position = new Vector2(center.x + width * xCirclePoint, center.y + height * yCirclePoint);
                vh.AddVert(vertex);

                vertex.position = new Vector2(center.x + (width - thickness) * xCirclePoint, center.y + (height - thickness) * yCirclePoint);
                vh.AddVert(vertex);
            }

            //Create triangles
            for (int i = 0; i < vertexCount - 2; i += 2)
            {
                vh.AddTriangle(i, i + 2, i + 3);
                vh.AddTriangle(i + 3, i + 1, i);
            }

            vh.AddTriangle(vertexCount - 2, 0, 1);
            vh.AddTriangle(1, vertexCount - 1, vertexCount - 2);
        }
    }
}
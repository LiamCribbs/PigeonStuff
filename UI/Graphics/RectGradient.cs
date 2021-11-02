using UnityEngine;
using UnityEngine.UI;

namespace Pigeon
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class RectGradient : Graphic
    {
        public Color endColor = Color.black;
        public enum GradientMode { Horizontal, Vertical }
        public GradientMode gradientMode;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            float minX = rectTransform.rect.xMin;
            float maxX = rectTransform.rect.xMax;
            float minY = rectTransform.rect.yMin;
            float maxY = rectTransform.rect.yMax;

            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;

            if (gradientMode == GradientMode.Horizontal)
            {
                vertex.position = new Vector3(minX, minY);
                vertex.color = color;
                vh.AddVert(vertex);

                vertex.position = new Vector3(minX, maxY);
                vertex.color = color;
                vh.AddVert(vertex);

                vertex.position = new Vector3(maxX, maxY);
                vertex.color = endColor;
                vh.AddVert(vertex);
            }
            else
            {
                vertex.position = new Vector3(minX, minY);
                vertex.color = endColor;
                vh.AddVert(vertex);

                vertex.position = new Vector3(minX, maxY);
                vertex.color = color;
                vh.AddVert(vertex);

                vertex.position = new Vector3(maxX, maxY);
                vertex.color = color;
                vh.AddVert(vertex);
            }

            vertex.position = new Vector3(maxX, minY);
            vertex.color = endColor;
            vh.AddVert(vertex);

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }
    }
}
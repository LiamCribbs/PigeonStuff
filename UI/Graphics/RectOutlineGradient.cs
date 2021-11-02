using UnityEngine;
using UnityEngine.UI;

namespace Pigeon
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class RectOutlineGradient : OutlineGraphic
    {
        public Color endColor = Color.black;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            float minX = rectTransform.rect.xMin;
            float maxX = rectTransform.rect.xMax;
            float minY = rectTransform.rect.yMin;
            float maxY = rectTransform.rect.yMax;

            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = endColor;

            vertex.position = new Vector3(minX, minY);
            vh.AddVert(vertex);

            vertex.position = new Vector3(maxX, minY);
            vh.AddVert(vertex);

            vertex.position = new Vector3(minX + thickness, minY + thickness);
            vh.AddVert(vertex);

            vertex.position = new Vector3(maxX - thickness, minY + thickness);
            vh.AddVert(vertex);

            vertex.color = color;

            vertex.position = new Vector3(minX, maxY);
            vh.AddVert(vertex);

            vertex.position = new Vector3(maxX, maxY);
            vh.AddVert(vertex);

            vertex.position = new Vector3(minX + thickness, maxY - thickness);
            vh.AddVert(vertex);

            vertex.position = new Vector3(maxX - thickness, maxY - thickness);
            vh.AddVert(vertex);

            //Left edge
            vh.AddTriangle(0, 4, 6);
            vh.AddTriangle(6, 2, 0);

            //Top edge
            vh.AddTriangle(4, 5, 7);
            vh.AddTriangle(7, 6, 4);

            //Right edge
            vh.AddTriangle(5, 1, 3);
            vh.AddTriangle(3, 7, 5);

            //Bottom edge
            vh.AddTriangle(1, 0, 2);
            vh.AddTriangle(2, 3, 1);
        }
    }
}
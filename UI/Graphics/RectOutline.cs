using UnityEngine;
using UnityEngine.UI;

namespace Pigeon
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class RectOutline : OutlineGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            float minX = rectTransform.rect.xMin;
            float maxX = rectTransform.rect.xMax;
            float minY = rectTransform.rect.yMin;
            float maxY = rectTransform.rect.yMax;

            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;

            vertex.position = new Vector3(minX, minY);
            vh.AddVert(vertex);

            vertex.position = new Vector3(minX, maxY);
            vh.AddVert(vertex);

            vertex.position = new Vector3(maxX, maxY);
            vh.AddVert(vertex);

            vertex.position = new Vector3(maxX, minY);
            vh.AddVert(vertex);

            vertex.position = new Vector3(minX + thickness, minY + thickness);
            vh.AddVert(vertex);

            vertex.position = new Vector3(minX + thickness, maxY - thickness);
            vh.AddVert(vertex);

            vertex.position = new Vector3(maxX - thickness, maxY - thickness);
            vh.AddVert(vertex);

            vertex.position = new Vector3(maxX - thickness, minY + thickness);
            vh.AddVert(vertex);

            //Left edge
            vh.AddTriangle(0, 1, 5);
            vh.AddTriangle(5, 4, 0);

            //Top edge
            vh.AddTriangle(1, 2, 6);
            vh.AddTriangle(6, 5, 1);

            //Right edge
            vh.AddTriangle(2, 3, 7);
            vh.AddTriangle(7, 6, 2);

            //Bottom edge
            vh.AddTriangle(3, 0, 4);
            vh.AddTriangle(4, 7, 3);
        }
    }
}
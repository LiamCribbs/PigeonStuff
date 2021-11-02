using UnityEngine;
using UnityEngine.UI;

namespace Pigeon
{
    public class Rect : Graphic
    {
        //static Vector2 zero = new Vector2();

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            float minX = rectTransform.rect.xMin;
            float maxX = rectTransform.rect.xMax;
            float minY = rectTransform.rect.yMin;
            float maxY = rectTransform.rect.yMax;

            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;
            //zero.x = 0f;
            //zero.y = 0f;

            vertex.position = new Vector3(minX, minY);
            //vertex.uv0 = zero;
            vh.AddVert(vertex);

            vertex.position = new Vector3(minX, maxY);
            //zero.y = 1f;
            //vertex.uv0 = zero;
            vh.AddVert(vertex);

            vertex.position = new Vector3(maxX, maxY);
            //zero.x = maxX / maxY;
            //vertex.uv0 = zero;
            vh.AddVert(vertex);

            vertex.position = new Vector3(maxX, minY);
            //zero.y = 0f;
            //vertex.uv0 = zero;
            vh.AddVert(vertex);

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }
    }
}
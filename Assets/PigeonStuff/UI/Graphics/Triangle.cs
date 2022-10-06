using UnityEngine;
using UnityEngine.UI;

namespace Pigeon.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class Triangle : Graphic
    {
        [Space(10)]
        [SerializeField] bool rightTriangle;
        [SerializeField] bool flip;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;

            var rect = rectTransform.rect;

            if (flip)
            {
                vertex.position = new Vector3(rect.xMax, rect.yMax);
                vh.AddVert(vertex);
                vertex.position = rightTriangle ? new Vector3(rect.xMin, rect.yMin) : new Vector3(rect.center.x, rect.yMin);
                vh.AddVert(vertex);
                vertex.position = new Vector3(rect.xMin, rect.yMax);
                vh.AddVert(vertex);
            }
            else
            {
                vertex.position = new Vector3(rect.xMax, rect.yMin);
                vh.AddVert(vertex);
                vertex.position = rect.min;
                vh.AddVert(vertex);
                vertex.position = rightTriangle ? new Vector3(rect.xMin, rect.yMax) : new Vector3(rect.center.x, rect.yMax);
                vh.AddVert(vertex);
            }

            vh.AddTriangle(0, 1, 2);
        }
    }
}
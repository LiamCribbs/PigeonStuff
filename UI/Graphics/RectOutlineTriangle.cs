using UnityEngine;
using UnityEngine.UI;

namespace Pigeon
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class RectOutlineTriangle : OutlineGraphic
    {
        [Range(0f, 1f)] [SerializeField] float convergence;

        public float Convergence
        {
            get => convergence;
            set
            {
                convergence = value;
                SetVerticesDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            float minX = rectTransform.rect.xMin;
            float maxX = rectTransform.rect.xMax;
            float minY = rectTransform.rect.yMin;
            float maxY = rectTransform.rect.yMax;

            const float halfY = 0f;
            const float angle = 30f * Mathf.Deg2Rad;

            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;

            vertex.position = new Vector3(minX, minY);
            vh.AddVert(vertex);

            vertex.position = new Vector3(minX, maxY);
            vh.AddVert(vertex);

            vertex.position = new Vector3(maxX, Mathf.LerpUnclamped(maxY, halfY, convergence));
            vh.AddVert(vertex);

            vertex.position = new Vector3(maxX, Mathf.LerpUnclamped(minY, halfY, convergence));
            vh.AddVert(vertex);

            vertex.position = new Vector3(minX + Mathf.Sin(angle) * thickness * 2f, minY + Mathf.Cos(angle) * Mathf.Lerp(thickness, thickness * 2f, convergence));
            vh.AddVert(vertex);

            vertex.position = new Vector3(minX + Mathf.Sin(angle) * thickness * 2f, maxY - Mathf.Cos(angle) * Mathf.Lerp(thickness, thickness * 2f, convergence));
            vh.AddVert(vertex);

            vertex.position = new Vector3(maxX - Mathf.Lerp(thickness, thickness * 2f, convergence), Mathf.LerpUnclamped(maxY - thickness, halfY, convergence));
            vh.AddVert(vertex);

            vertex.position = new Vector3(maxX - Mathf.Lerp(thickness, thickness * 2f, convergence), Mathf.LerpUnclamped(minY + thickness, halfY, convergence));
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
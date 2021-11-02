using UnityEngine;
using UnityEngine.UI;

namespace Pigeon
{
    public class RectTriangle : Graphic
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

            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;

            vertex.position = new Vector3(minX, minY);
            vh.AddVert(vertex);

            vertex.position = new Vector3(minX, maxY);
            vh.AddVert(vertex);

            vertex.position = new Vector3(maxX, Mathf.Lerp(maxY, halfY, convergence));
            vh.AddVert(vertex);

            vertex.position = new Vector3(maxX, Mathf.Lerp(minY, halfY, convergence));
            vh.AddVert(vertex);

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }
    }
}
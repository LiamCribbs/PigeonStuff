using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class Polygon : Graphic
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
        }

        vertex.position = Vector2.zero;
        vh.AddVert(vertex);

        //Create triangles
        for (int i = 0; i < sides; i++)
        {
            vh.AddTriangle(i, i + 1, sides);
        }

        vh.AddTriangle(sides - 1, 0, sides);
    }
}
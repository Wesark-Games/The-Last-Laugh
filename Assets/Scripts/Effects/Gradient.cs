using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/Vertical Gradient")]
public class UIVerticalGradient : BaseMeshEffect
{
    public Color topColor = new Color(0f, 0f, 0f, 0.6f);
    public Color bottomColor = new Color(0f, 0f, 0f, 0f);

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive()) return;

        int count = vh.currentVertCount;
        if (count == 0) return;

        UIVertex vertex = new UIVertex();
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        for (int i = 0; i < count; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);
            float y = vertex.position.y;
            if (y < minY) minY = y;
            if (y > maxY) maxY = y;
        }

        float height = maxY - minY;

        for (int i = 0; i < count; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);
            float normalizedY = (vertex.position.y - minY) / height;
            vertex.color = Color.Lerp(bottomColor, topColor, normalizedY);
            vh.SetUIVertex(vertex, i);
        }
    }
}
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CreateAssetMenu()]
public class ColorPalette : ScriptableObject
{
    public Color[] colors;

    public static ColorPalette[] ColorPalettes { get; private set; }
    public static ColorPalette SelectedColorPalette { get; private set; }

    public void Apply()
    {
        if (colors == null || colors.Length == 0)
        {
            return;
        }

        ColorPaletteGraphic[] graphics = FindObjectsOfType<ColorPaletteGraphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            graphics[i].Graphic.color = graphics[i].colorIndex < colors.Length ? colors[graphics[i].colorIndex] : colors[0];
        }

        SelectedColorPalette = this;
    }

    [InitializeOnLoadMethod()]
    static void Initialize()
    {
        ColorPalettes = Resources.FindObjectsOfTypeAll<ColorPalette>();
    }
}

[CustomEditor(typeof(ColorPalette))]
public class ColorPaletteEditor : Editor
{
    const float ColorWidth = 30f;
    const float ColorPadding = 4f;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        ColorPalette instance = (ColorPalette)target;

        var colors = serializedObject.FindProperty("colors");
        int numColors = colors.arraySize;

        // Get width
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        float width = GUILayoutUtility.GetLastRect().width;

        width = (numColors * ColorWidth + (numColors - 1) * ColorPadding) > width ? (width - numColors * ColorPadding) / numColors : ColorWidth;
        GUIStyle styleCentered = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        styleCentered.normal.textColor = Color.grey;

        for (int i = 0; i < numColors; i++)
        {
            Rect rect = new Rect(10 + width * i + ColorPadding * i, 10, width, width);
            EditorGUI.DrawRect(rect, instance.colors[i]);
            rect.y += width - ColorPadding;
            EditorGUI.LabelField(rect, i.ToString(), styleCentered);
        }

        EditorGUILayout.Space(75);

        EditorGUILayout.PropertyField(colors);

        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Apply Palette"))
        {
            instance.Apply();
        }
    }
}
#endif
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class ColorPaletteGraphic : MonoBehaviour
{
    public Graphic Graphic { get => GetComponent<Graphic>(); }
    public int colorIndex;

    void Reset()
    {
        while (UnityEditorInternal.ComponentUtility.MoveComponentUp(this));
    }
}

[CustomEditor(typeof(ColorPaletteGraphic))]
public class ColorPaletteGraphicEditor : Editor
{
    const float ColorStartHeight = 100f;
    const float ColorWidth = 30f;
    const float ColorPadding = 4f;

    static bool colorFoldout;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var colorIndex = serializedObject.FindProperty("colorIndex");
        int maxValue = 0;
        for (int i = 0; i < ColorPalette.ColorPalettes.Length; i++)
        {
            int length = ColorPalette.ColorPalettes[i].colors.Length - 1;
            if (length > maxValue)
            {
                maxValue = length;
            }
        }

        GUI.color = ColorPalette.SelectedColorPalette && colorIndex.intValue < ColorPalette.SelectedColorPalette.colors.Length ? ColorPalette.SelectedColorPalette.colors[colorIndex.intValue] : ColorPalette.ColorPalettes != null && ColorPalette.ColorPalettes.Length > 0 && ColorPalette.ColorPalettes[0] && colorIndex.intValue < ColorPalette.ColorPalettes[0].colors.Length ? ColorPalette.ColorPalettes[0].colors[colorIndex.intValue] : Color.white;
        EditorGUILayout.IntSlider(colorIndex, 0, maxValue);
        GUI.color = Color.white;

        if (colorFoldout = EditorGUILayout.Foldout(colorFoldout, "Palettes", true))
        {
            ColorPaletteGraphic instance = (ColorPaletteGraphic)target;

            float largestHeight = 0f;
            for (int i = 0; i < ColorPalette.ColorPalettes.Length; i++)
            {
                Color[] colors = ColorPalette.ColorPalettes[i].colors;
                float height = colors.Length * ColorWidth + (colors.Length - 1) * ColorPadding;
                if (height > largestHeight)
                {
                    largestHeight = height;
                }

                if (GUI.Button(new Rect(20 + ColorWidth * i + ColorPadding * i, ColorStartHeight - 20 - ColorPadding * 2, ColorWidth, 20), new GUIContent("A", "Apply this palette to all objects")))
                {
                    ColorPalette.ColorPalettes[i].Apply();
                }

                if (GUI.Button(new Rect(20 + ColorWidth * i + ColorPadding * i, ColorStartHeight - 40 - ColorPadding * 3, ColorWidth, 20), new GUIContent("S", "Set this object's color")))
                {
                    instance.Graphic.color = colors[instance.colorIndex];
                }

                if (instance.colorIndex < colors.Length)
                {
                    EditorGUI.DrawRect(new Rect(20 + ColorWidth * i + ColorPadding * i - ColorPadding * 0.5f, ColorStartHeight + ColorWidth * instance.colorIndex + 
                        ColorPadding * instance.colorIndex - ColorPadding * 0.5f, ColorWidth + ColorPadding, ColorWidth + ColorPadding), new Color(1, 1, 1, 0.75f));
                }

                for (int j = 0; j < colors.Length; j++)
                {
                    EditorGUI.DrawRect(new Rect(20 + ColorWidth * i + ColorPadding * i, ColorStartHeight + ColorWidth * j + ColorPadding * j, ColorWidth, ColorWidth), colors[j]);
                }
            }

            GUILayout.Space(ColorStartHeight * 0.5f + largestHeight);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
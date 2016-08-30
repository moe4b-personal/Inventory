using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(RadialMenu))]
public class RadialMenuEditor : Editor {

    SerializedProperty value;
    SerializedProperty items;
    
    SerializedProperty colors;
    SerializedProperty bevel;

    SerializedProperty limitRange;
    SerializedProperty useMouse;
    SerializedProperty scaledCanvas;
    SerializedProperty canvas;

    SerializedProperty OnItemSelected;

    void OnEnable()
    {
        value = serializedObject.FindProperty("value");
        items = serializedObject.FindProperty("items");

        colors = serializedObject.FindProperty("colors");
        bevel = serializedObject.FindProperty("bevel");
        limitRange = serializedObject.FindProperty("limitRange");
        useMouse = serializedObject.FindProperty("useMouse");
        scaledCanvas = serializedObject.FindProperty("scaledCanvas");
        canvas = serializedObject.FindProperty("canvas");

        OnItemSelected = serializedObject.FindProperty("OnItemSelected");
    }

    public override void OnInspectorGUI()
    {
        value.intValue = EditorGUILayout.IntSlider("Value", value.intValue, 0, items.arraySize - 1);

        DrawItems("Items", items);

        DrawColorBlock("Colors", colors);

        bevel.floatValue = EditorGUILayout.Slider("Bevel", bevel.floatValue, 0, 180f/items.arraySize);

        limitRange.boolValue = EditorGUILayout.Toggle("Limit Range", limitRange.boolValue);

        if(limitRange.boolValue)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();

            scaledCanvas.boolValue = EditorGUILayout.Toggle("Scaled Canvas", scaledCanvas.boolValue);

            if(scaledCanvas.boolValue)
            {
                EditorGUILayout.PropertyField(canvas, new GUIContent(""));
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }

        useMouse.boolValue = EditorGUILayout.Toggle("Use Mouse", useMouse.boolValue);

        Space();
        EditorGUILayout.PropertyField(OnItemSelected, new GUIContent("Item Selected"));

        serializedObject.ApplyModifiedProperties();
    }

    void DrawItems(string label, SerializedProperty items)
    {
        items.isExpanded = EditorGUILayout.Foldout(items.isExpanded, label);

        if(items.isExpanded)
        {
            DrawArraySizer(ref items);

            Space();

            EditorGUI.indentLevel++;

            for (int i = 0; i < items.arraySize; i++)
            {
                DrawItem("Item " + i, items.GetArrayElementAtIndex(i));
            }

            EditorGUI.indentLevel--;
        }
    }

    void DrawItem(string label, SerializedProperty item)
    {
            SerializedProperty sprite = item.FindPropertyRelative("sprite");
            EditorGUILayout.PropertyField(sprite, new GUIContent(label));
    }

    void DrawArraySizer(ref SerializedProperty array, int sideDistance = 2, int MidDistance = 0)
    {
        array.arraySize = EditorGUILayout.IntField("Size", array.arraySize);

        EditorGUILayout.BeginHorizontal();
        Space(sideDistance);
        if (GUILayout.Button("-"))
            if (array.arraySize != 0)
                array.arraySize--;

        Space(MidDistance);

        if (GUILayout.Button("+"))
            array.arraySize++;
        Space(sideDistance);
        EditorGUILayout.EndHorizontal();
    }

    public void Space(int spaceCount = 1)
    {
        for (int i = 0; i < spaceCount; i++)
        {
            EditorGUILayout.Space();
        }
    }

    void DrawColorBlock(string label, SerializedProperty block)
    {
        SerializedProperty normalColor = block.FindPropertyRelative("normal");
        SerializedProperty pressedColor = block.FindPropertyRelative("pressed");

        block.isExpanded = EditorGUILayout.Foldout(block.isExpanded, label);

        if(block.isExpanded)
        {
            EditorGUI.indentLevel++;
            normalColor.colorValue = EditorGUILayout.ColorField("Normal", normalColor.colorValue);
            pressedColor.colorValue = EditorGUILayout.ColorField("Pressed", pressedColor.colorValue);
            EditorGUI.indentLevel--;
        }
    }
}

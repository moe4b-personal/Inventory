#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CleanUI : EditorWindow {
    [MenuItem("GameObject/Clean UI/Text", false, 0)]
    static void CreateText()
    {
        CreateUI("Text");
    }

    [MenuItem("GameObject/Clean UI/Image", false, 0)]
    static void CreateImage()
    {
        CreateUI("Image");
    }

    [MenuItem("GameObject/Clean UI/Raw Image", false, 0)]
    static void CreateRawImage()
    {
        CreateUI("Raw Image");
    }

    [MenuItem("GameObject/Clean UI/Button",false,0)]
    static void CreateButton()
    {
        CreateUI("Button");
    }

    [MenuItem("GameObject/Clean UI/Toggle", false, 0)]
    static void CreateToggle()
    {
        CreateUI("Toggle");
    }

    [MenuItem("GameObject/Clean UI/Slider", false, 0)]
    static void CreateSlider()
    {
        CreateUI("Slider");
    }

    [MenuItem("GameObject/Clean UI/Scroll Bar", false, 0)]
    static void CreateScrollBar()
    {
        CreateUI("Scroll Bar");
    }

    [MenuItem("GameObject/Clean UI/Drop Down", false, 0)]
    static void CreateDropDown()
    {
        CreateUI("Drop Down");
    }

    [MenuItem("GameObject/Clean UI/Input Field", false, 0)]
    static void CreateInputField()
    {
        CreateUI("Input Field");
    }

    [MenuItem("GameObject/Clean UI/Scroll Rect", false, 0)]
    static void CreateScrollRect()
    {
        CreateUI("Scroll Rect");
    }

    [MenuItem("GameObject/Clean UI/Vertical Scroll Rect", false, 0)]
    static void CreateVerticalScrollRect()
    {
        CreateUI("Vertical Scroll Rect");
    }

    [MenuItem("GameObject/Clean UI/Panel", false, 0)]
    static void CreatePanel()
    {
        CreateUI("Panel");
    }

    [MenuItem("GameObject/Clean UI/Progress Bar", false, 0)]
    static void CreateProgressBar()
    {
        CreateUI("Progress Bar");
    }

    [MenuItem("GameObject/Clean UI/Circular Progress", false, 0)]
    static void CreateCircularProgress()
    {
        CreateUI("Circular Progress");
    }

    [MenuItem("GameObject/Clean UI/Menu", false, 0)]
    static void CreateMenu()
    {
        CreateUI("Menu");
    }

    [MenuItem("GameObject/Clean UI/Radial Menu", false, 0)]
    static void RadialMenu()
    {
        CreateUI("Radial Menu");
    }

    [MenuItem("GameObject/Clean UI/Canvas", false, 0)]
    static GameObject CreateCanvas()
    {
        GameObject canvas = Instantiate(Resources.Load("Canvas")) as GameObject;

        canvas.name = "Canvas";

        if (Selection.activeGameObject != null)
        {
            if (Selection.activeTransform.GetComponent<Canvas>() || Selection.activeTransform.root.GetComponent<Canvas>())
            {
                canvas.transform.SetParent(Selection.activeTransform);

                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                DestroyImmediate(scaler);

                RectTransform rt = canvas.GetComponent<RectTransform>();

                rt.pivot = new Vector2(0.5f, 0.5f);

                rt.localScale = Vector3.one;

                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;

                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

            }
        }

        GameObject eventSystem;

        if(FindObjectOfType<EventSystem>())
        {
            eventSystem = FindObjectOfType<EventSystem>().gameObject;
        }

        else
        {
            eventSystem = Instantiate(Resources.Load("Event System")) as GameObject;
            eventSystem.name = "Event System";
            Undo.RegisterCreatedObjectUndo(eventSystem, "Removed Canvas");
        }

        Selection.activeGameObject = canvas;

        Undo.RegisterCreatedObjectUndo(canvas, "Removed Canvas");

        return canvas;
    }

    static void CreateUI(string prefabName)
    {
        GameObject UI = Instantiate(Resources.Load(prefabName)) as GameObject;

        Undo.RegisterCreatedObjectUndo(UI, "Removed " + prefabName);

        UI.name = prefabName;

        GameObject canvas = null;

        bool found = false;

        if (Selection.activeGameObject != null )
        {
            if (Selection.activeTransform.GetComponent<Canvas>() || Selection.activeTransform.root.GetComponent<Canvas>())
            {
                canvas = Selection.activeGameObject;
                found = true;
            }
        }

        else if(!found)
        {
            Canvas foundCanvas = FindObjectOfType<Canvas>();

            if (foundCanvas)
                canvas = FindObjectOfType<Canvas>().gameObject;

            if(!canvas)
            {
                canvas = CreateCanvas();
            }
        }

        UI.transform.SetParent(canvas.transform, false);

        RectTransform rt = UI.GetComponent<RectTransform>();

        rt.localScale = Vector3.one;

        if(prefabName == "Menu")
        {
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        rt.anchoredPosition = Vector2.zero;

        Selection.activeGameObject = UI;
    }

}

#endif
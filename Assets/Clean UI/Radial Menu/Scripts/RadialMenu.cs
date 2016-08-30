using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

[ExecuteInEditMode]
public class RadialMenu : MonoBehaviour
{
    [SerializeField]
    int value;
    public int Value { get { return value; } set { SetValue(value); } }

    [SerializeField]
    MenuItem[] items;

    [SerializeField]
    ColorBlock colors;

    [SerializeField]
    float bevel = 5;

    [SerializeField]
    bool limitRange = true;

    [SerializeField]
    bool useMouse = true;

    [SerializeField]
    bool scaledCanvas = false;
    [SerializeField]
    Canvas canvas;

    float degreePerOption;
    float selectionFill;

    Image selection;
    GameObject itemTemplate;
    Transform itemsParent;

    Vector3 selectionDirection;
    float selectionDegree;

    RectTransform rect;

    [SerializeField][HideInInspector]
    GameObject[] itemsObjects;

    [SerializeField]
    public RadialMenuEvent OnItemSelected;

    void Init()
    {
        SetItemsValues();

        selection.fillAmount = selectionFill;

        SetValue(value);
        SetItems();
    }

    void SetItemsValues()
    {
        itemsParent = transform.Find("Items");
        itemTemplate = transform.Find("Item Template").gameObject;
        selection = transform.Find("Selection").GetComponent<Image>();

        rect = GetComponent<RectTransform>();
        degreePerOption = 180f / items.Length;
        selectionFill = 1f / items.Length;
    }

    void Start()
    {
        SetItemsValues();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlaying)
        {
            DirectionSelection();
        }
        else
            Init();
#else
        DirectionSelection();
#endif
    }

    void DirectionSelection()
    {
        if(useMouse)
        {
            selectionDirection = new Vector2(Input.mousePosition.x - rect.position.x, Input.mousePosition.y - rect.position.y);

            selectionDirection.x /= rect.sizeDelta.x / 2;
            selectionDirection.y /= rect.sizeDelta.y / 2;

            if (scaledCanvas)
            {
                selectionDirection /= canvas.scaleFactor;
            }
        }

        if (selectionDirection.magnitude > 1 && limitRange)
            return;

        selectionDegree = GetVector2Angle(selectionDirection);

        for (int i = 0; i < items.Length; i++)
        {
            float minAngle = degreePerOption + (i * degreePerOption * 2);
            float maxAngle = degreePerOption + ((i + 1) * degreePerOption * 2);

            if (IsAngleBetween(selectionDegree, minAngle, maxAngle))
            {
                if (i == items.Length - 1)
                    SetValue(0);
                else
                    SetValue(i + 1);

                if(useMouse)
                {
                    if (Input.GetMouseButtonDown(0))
                        Select();
                    if (Input.GetMouseButtonUp(0))
                        DeSelect();
                }

                return;
            }
        }
    }

    internal void Select()
    {
        selection.color = colors.Pressed;
    }

    internal void DeSelect()
    {
        if (OnItemSelected != null)
            OnItemSelected.Invoke(value);

        selection.color = colors.Normal;
    }

    internal void SetValue(int newIndex)
    {
        value = newIndex;

        selection.rectTransform.localRotation = Quaternion.Euler(0, 0, degreePerOption - (value * degreePerOption * 2));
    }

    void SetItems()
    {
        for (int i = 0; i < itemsObjects.Length; i++)
        {
            if(itemsObjects[i])
                DestroyImmediate(itemsObjects[i]);
        }

        itemsObjects = new GameObject[items.Length];

        for (int i = 0; i < items.Length; i++)
        {
            RectTransform itemRt = Instantiate(itemTemplate).GetComponent<RectTransform>();

            itemsObjects[i] = itemRt.gameObject;

            Image itemBackground = itemRt.Find("Item Background").GetComponent<Image>();
            Image itemImage = itemRt.Find("Item Image").GetComponent<Image>();

            itemImage.rectTransform.eulerAngles = new Vector3(0, 0, degreePerOption * i * 2);

            items[i].background = itemBackground;
            items[i].image = itemImage;
            itemImage.sprite = items[i].Sprite;

            BevelDegreeAndFill(itemBackground, degreePerOption, selectionFill, bevel);

            itemRt.gameObject.SetActive(true);
            itemRt.localRotation = Quaternion.Euler(0, 0, -degreePerOption * 2 * i);
            itemRt.SetParent(itemsParent, false);
        }
    }

    //helper functions
    void BevelDegreeAndFill(Image image, float degree, float fill, float bevelDegree)
    {
        float beveledFill = fill - ((bevelDegree / 360) * 2);

        image.fillAmount = beveledFill;
        image.rectTransform.localRotation = Quaternion.Euler(0, 0, degree - bevelDegree);
    }

    public static bool IsAngleBetween(float angle, float min, float max)
    {
        if(max > 360)
        {
            max -= 360;
            min -= 360;
            if(angle > 180)
                angle -= 360;
        }

        if (angle > min && angle < max)
            return true;

        return false;
    }

    public static float GetVector2Angle(Vector2 vec)
    {
        if (vec.x < 0)
        {
            return 360 - (Mathf.Atan2(vec.x, vec.y) * Mathf.Rad2Deg * -1);
        }
        else
        {
            return Mathf.Atan2(vec.x, vec.y) * Mathf.Rad2Deg;
        }
    }

    [Serializable]
    public class MenuItem
    {
        [SerializeField]
        Sprite sprite;
        public Sprite Sprite { get { return sprite; } }

        [SerializeField]
        internal Image background;
        [SerializeField]
        internal Image image;
    }

    [Serializable]
    public class ColorBlock
    {
        [SerializeField]
        Color normal;
        public Color Normal { get { return normal; } }

        [SerializeField]
        Color pressed;
        public Color Pressed { get { return pressed; } }
    }

    [Serializable]
    public class RadialMenuEvent : UnityEvent<int>
    {

    }
}
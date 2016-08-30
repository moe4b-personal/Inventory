using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;

public class Inventory : MonoBehaviour {
    [SerializeField]
    int rows = 6;
    [SerializeField]
    int columns = 4;

    [SerializeField]
    InternalSlot[] slots;

    [SerializeField]
    SlotItem[] items;

    [SerializeField]
    Padding padding;

    [SerializeField]
    GameObject slotTemplate;

    [SerializeField]
    InventorySlotColor colors;
    public InventorySlotColor Colors { get { return colors; } }

    [Header("Slots")]
    [SerializeField]
    MouseSlot mouseSlot;
    [SerializeField]
    TrashSlot trashSlot;
    [SerializeField]
    DropSlot dropSlot;

    [Header("Buttons")]
    [SerializeField]
    internal PointerEventData.InputButton selectButton = PointerEventData.InputButton.Left;
    [SerializeField]
    internal PointerEventData.InputButton useButton = PointerEventData.InputButton.Right;
    [SerializeField]
    internal PointerEventData.InputButton divideButton = PointerEventData.InputButton.Middle;
    [SerializeField]
    internal KeyCode singleItemDevideAssistKey = KeyCode.LeftShift;

    [SerializeField]
    KeyCode pickUpKey = KeyCode.F;

    [SerializeField]
    ItemDropper dropper = new ItemDropper(200);
    public Transform ThrowPosition { get { return dropper.dropPosition; } internal set { dropper.dropPosition = value; } }

    [SerializeField]
    ItemPicker picker = new ItemPicker(5);
    public Transform PickUpPosition { get { return picker.trans; } internal set { picker.trans = value; } }

    int currentItemIndex = NoIndex;
    public int CurrentItemIndex { get { return currentItemIndex; } internal set { currentItemIndex = value; } }

    int targetItemIndex = NoIndex;
    public int TargetItemIndex { get { return targetItemIndex; } internal set { targetItemIndex = value; } }

    internal bool Changing { get { return currentItemIndex != NoIndex || targetItemIndex != NoIndex; } }

    string path;

    public InternalSlot this[int index]
    {
        get
        {
            return slots[index];
        }
    }

    [SerializeField]
    InventoryItem[] InventoryItems;

    public const int NoIndex = -1;
    public const int Trash = -2;
    public const int Drop = -3;
    public const string NoID = "";

    void Start()
	{
        path = Path.Combine(Application.dataPath, "Inventory.sav");

        SetUI();

        SaveOrLoad();
    }

    void Update()
    {
        if(slots[0].Count > 0)
        {
            mouseSlot.Position = Input.mousePosition;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            StackAll(false);
            ArrangeByStack(true);
        }

        PickUp();
    }

    void SaveOrLoad()
    {
        if (File.Exists(Path.Combine(Application.dataPath, "Inventory.sav")))
            load();
        else
        {
            for (int i = 0; i < items.Length; i++)
            {
                AddInventoryItem(items[i], true, false);
            }

            Save();
        }
    }

    public void PickUp()
    {
        if (picker.DetectItem())
        {
            if (Input.GetKeyDown(pickUpKey))
            {
                InventoryItem item = picker.PickUpItem();

                if (item != null)
                {
                    AddInventoryItem(new SlotItem(item, 1), true);
                }
            }
        }
    }

    void SetUI()
    {
        slots = new InternalSlot[rows* columns + 1];

        slots[0] = new InternalSlot(mouseSlot, new SlotItem(null, 0));
        mouseSlot.inventory = this;
        mouseSlot.index = 0;

        trashSlot.inventory = this;
        dropSlot.inventory = this;

        RectTransform rect = (RectTransform)transform;
        Vector2 fullSize = slotTemplate.GetComponent<RectTransform>().sizeDelta;

        fullSize.x += padding.Right + padding.Left;
        fullSize.x *= rows;
        fullSize.x += padding.Right + padding.Left;

        fullSize.y += padding.Up + padding.Down;
        fullSize.y *= columns;
        fullSize.y += padding.Up + padding.Down;

        rect.sizeDelta = fullSize;

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                GameObject slotO = Instantiate(slotTemplate);

                var slotRect = slotO.GetComponent<RectTransform>();
                slotRect.SetParent(transform, true);
                Vector2 position = new Vector2();

                position.x = y * slotRect.sizeDelta.y;
                position.x += (y + 1) * (padding.Right + padding.Left);

                position.y = -x * slotRect.sizeDelta.x;
                position.y -= (x + 1) * (padding.Up + padding.Down);

                slotRect.anchoredPosition = position;

                var slot = slotO.GetComponent<InventorySlot>();
                slot.ItemImage.enabled = false;
                slot.index = x * rows + y + 1;
                slot.inventory = this;

                slots[x * rows + y + 1].slot = slot;
            }
        }
    }

    internal void StackAll(bool autoSave = true)
    {
        for (int x = 1; x < slots.Length; x++) //look through all elements
        {
            for (int y = x; y < slots.Length; y++)
            {
                if (slots[x].SlotItem.Item == slots[y].SlotItem.Item) //found match
                {
                    if (slots[x].CanTakeMore() > 0)
                        AddToIndex(x, y, false);
                    else
                        break;
                }
            }
        }

        if (autoSave)
            Save();
    }

    internal void ArrangeByStack(bool autoSave = true)
    {
        List<InventoryItem> items = new List<InventoryItem>();
        InventoryItem currentItem;

        for (int x = 1; x < slots.Length; x++)
        {
            if (slots[x].SlotItem.Item != null && !items.Contains(slots[x].SlotItem.Item))
            {
                currentItem = slots[x].SlotItem.Item;
                items.Add(currentItem);

                for (int y = 1; y < slots.Length; y++)
                {
                    if (slots[y].SlotItem.Item == currentItem || slots[y].SlotItem.Item == null)
                    {
                        for (int z = y; z < slots.Length; z++)
                        {
                            if (slots[z].SlotItem.Item == currentItem)
                            {
                                if (slots[z].Count > slots[y].Count)
                                {
                                    SwitchIndex(y, z, false);
                                }
                            }
                        }
                    }
                }
            }
        }

        if(autoSave)
            Save();
    }

    internal bool AddInventoryItem(SlotItem slotItem, bool canAddToCount = false, bool autoSave = true)
    {
        slotItem.ItemID = GetItemID(slotItem.Item);

        for (int x = 1; x < slots.Length; x++)
        {
            if(slots[x].Occupied)
            {
                if (slotItem.Item == slots[x].SlotItem.Item)
                {
                    if (AddToIndex(x, ref slotItem, false))
                    {
                        if (autoSave)
                            Save();

                        return true;
                    }
                }
            }
            else
            {
                slots[x].SlotItem = slotItem;

                if (autoSave)
                    Save();

                return true;
            }
        }

        print("all slots occupied");
        return false;
    }

    internal bool SwitchIndex(bool autoSave = true)
    {
        return SwitchIndex(currentItemIndex, targetItemIndex, autoSave);
    }

    internal bool SwitchIndex(int index1, int index2, bool autoSave = false)
    {
        SlotItem currentItem = slots[index1].SlotItem;

        if (index1 == index2)
            return false;

        if(index2 == Trash) //trash item
        {
            slots[index1].SlotItem = new SlotItem(null, 0);

            if(autoSave)
                Save();

            return true;
        }
        else if(index2 == Drop) //drop item
        {
            DropItem(index1);
            return true;
        }

        if (AddToIndex(index1, index2, autoSave))
        {

        }
        else
        {
            slots[index1].SlotItem = slots[index2].SlotItem;
            slots[index2].SlotItem = currentItem;

            if(autoSave)
                Save();
        }

        if (index1 == 0 || index2 == 0)
        {
            UpdateMouseSlot();
        }

        return true;
    }

    void DropItem(int index)
    {
        for (int i = 0; i < slots[index].Count; i++)
        {
            dropper.Throw(slots[index].SlotItem.Item);
        }

        slots[index].SlotItem = new SlotItem(null, 0);

        Save();
    }

    void UpdateMouseSlot()
    {
        if (slots[0].Count == 0 && mouseSlot.Visible) //no items but still visible
            mouseSlot.Hide();

        else if (slots[0].Count > 0 && !mouseSlot.Visible) //items avalible but invisible
            mouseSlot.Set(slots[0].SlotItem, Input.mousePosition);
    }

    bool AddToIndex(int index, ref SlotItem slotItem, bool autoSave = true)
    {
        if (slots[index].SlotItem.Item != slotItem.Item)
            return false;

        var addable = slots[index].CanTakeMore(); //number of elements that can  be added

        if (addable > 0) //can add more items
        {
            uint toAdd = (uint)Mathf.Clamp(slotItem.Count, 0, addable);
            slots[index].Count += toAdd;
            slotItem.Count -= toAdd;

            if (index == 0)
            {
                UpdateMouseSlot();
            }

            if(autoSave)
                Save();

            return true;
        }

        return false;
    }

    internal bool AddToIndex(int fromIndex, int toIndex, bool autoSave = true)
    {
        if (slots[fromIndex].SlotItem.Item != slots[toIndex].SlotItem.Item)
            return false;

        var addable = slots[toIndex].CanTakeMore(); //number of elements that can  be added

        if(addable > 0) //can add more items
        {
            uint toAdd = (uint)Mathf.Clamp(slots[fromIndex].Count, 0, addable);
            slots[toIndex].Count += toAdd;
            slots[fromIndex].Count -= toAdd;

            if (fromIndex == 0 || toIndex == 0)
            {
                UpdateMouseSlot();
            }

            if(autoSave)
                Save();

            return true;
        }

        return false;
    }

    internal void UseItem(int index)
    {
        print("using item" + index);
    }

    internal bool Divide(int index, bool byHalf = true)
    {
        if (slots[index].Count < 2)
            return false;

        uint devideCount;

        if (byHalf)
            devideCount = slots[index].Count / 2;
        else
            devideCount = 1;

        slots[index].Count = slots[index].Count - devideCount; //divide and add the remaining of the division
        slots[0].SlotItem = new SlotItem(slots[index].SlotItem.Item, devideCount);

        UpdateMouseSlot();
        return true;
    }

    internal string GetItemID(InventoryItem item)
    {
        for (int i = 0; i < InventoryItems.Length; i++)
        {
            if (InventoryItems[i] == item)
                return InventoryItems[i].ItemID;
        }

        return NoID;
    }

    public InventoryItem GetItem(string ID)
    {
        if (ID == NoID)
            return null;

        for (int i = 0; i < InventoryItems.Length; i++)
        {
            if (InventoryItems[i].ItemID == ID)
                return InventoryItems[i];
        }

        return null;
    }

    internal void Save()
    {
        print("Save");
        SlotItem[] saveItems = new SlotItem[slots.Length];

        for (int i = 1; i < slots.Length; i++)
        {
            saveItems[i - 1] = slots[i].SlotItem;
            saveItems[i - 1].ItemID = GetItemID(saveItems[i - 1].Item);
        }

        DataContractSerializer sr = new DataContractSerializer(typeof(SlotItem[]));

        XmlWriterSettings settings = new XmlWriterSettings() { Indent = true };

        using (XmlWriter writer = XmlWriter.Create(path, settings))
        {
            sr.WriteObject(writer, saveItems);
        }
    }

    internal void load()
    {
        print("Load");
        DataContractSerializer sr = new DataContractSerializer(typeof(SlotItem[]));

        SlotItem[] loadedItems = null;

        using (FileStream file = new FileStream(path, FileMode.Open))
        {
            try
            {
                loadedItems = (SlotItem[])sr.ReadObject(file);
            }
            catch (System.Exception)
            {
                Debug.LogError("Error");
            }
        }

        if(loadedItems != null)
        {
            AssignItems(ref loadedItems);

            for (int i = 1; i < slots.Length; i++)
            {
                slots[i].SlotItem = loadedItems[i - 1];
            }
        }
    }

    internal void AssignItems(ref SlotItem[] items)
    {
        for (int i = 0; i < items.Length; i++)
        {
            items[i].Item = GetItem(items[i].ItemID);
        }
    }

    [System.Serializable]
    struct Padding
    {
        [SerializeField]
        float right;
        public float Right { get { return right; } }

        [SerializeField]
        float left;
        public float Left { get { return left; } }

        [SerializeField]
        float up;
        public float Up { get { return up; } }

        [SerializeField]
        float down;
        public float Down { get { return down; } }

        public Padding(float right_, float left_, float up_, float down_)
        {
            right = right_;
            left = left_;
            up = up_;
            down = down_;
        }
    }

    [System.Serializable]
    public struct InventorySlotColor
    {
        [SerializeField]
        Color normal;
        public Color Normal { get { return normal; } }

        [SerializeField]
        Color highlighted;
        public Color Highlighted { get { return highlighted; } }

        [SerializeField]
        Color selected;
        public Color Selected { get { return selected; } }

        public InventorySlotColor(Color newNormal, Color newHightlighted, Color newSelected)
        {
            normal = newNormal;
            highlighted = newHightlighted;
            selected = newSelected;
        }
    }

    [System.Serializable]
    public struct InternalSlot
    {
        [SerializeField]
        internal Slot slot;

        [SerializeField]
        SlotItem slotItem;
        public SlotItem SlotItem { get { return slotItem; } internal set { SetItem(value); } }

        public uint Count {
            get
            {
                return SlotItem.Count;
            }
            internal set
            {
                if (value > slotItem.Item.MaxStack)
                    value = slotItem.Item.MaxStack;

                if (value == 0)
                {
                    SetItem(new SlotItem(null, 0));
                    return;
                }

                slotItem.Count = value;
                slot.UpdateCountText();
            }
        }

        public bool Occupied { get { return slotItem.Count != 0; } }

        public uint CanTakeMore()
        {
            if (SlotItem.Item == null)
                return 0;

            if (slotItem.Item.Stackable)
            {
                return slotItem.Item.MaxStack - Count;
            }

            return 0;
        }

        public void SetItem(SlotItem newItem)
        {
            if (newItem.Count == 0)
            {
                slotItem = new SlotItem(null, 0);

                slot.UpdateSprite();
                slot.UpdateCountText();
            }
            else
            {
                slotItem = newItem;

                slot.UpdateSprite();
                slot.UpdateCountText();
            }
        }

        public InternalSlot(Slot newSlot, SlotItem newItem)
        {
            slot = newSlot;
            slotItem = newItem;
        }
    }

    [System.Serializable]
    public class ItemDropper
    {
        [SerializeField]
        internal Transform dropPosition;

        [SerializeField]
        int power;

        public ItemDropper(int newPower)
        {
            power = newPower;
        }

        public void Throw(InventoryItem item)
        {
            Throw(item, power);
        }

        public void Throw(InventoryItem item, int force)
        {
            GameObject throwItem = (GameObject)Instantiate(item.Model, dropPosition.position, dropPosition.rotation);

            ItemModel iModel = throwItem.GetComponent<ItemModel>();
            iModel.item = item;

            Rigidbody throwRb = throwItem.GetComponent<Rigidbody>();

            throwRb.AddForce(dropPosition.forward * force, ForceMode.Acceleration);
        }
    }

    [System.Serializable]
    public class ItemPicker
    {
        [SerializeField]
        internal Transform trans;

        [SerializeField]
        float range;

        [SerializeField]
        LayerMask mask;

        RaycastHit hit;

        public ItemPicker(float newRange)
        {
            range = newRange;
            mask = Physics.AllLayers;
        }

        internal bool DetectItem(string tag = "Inventory Item")
        {
            if (Physics.Raycast(trans.position, trans.forward * range, out hit, range, mask))
            {
                print(hit.transform.name + " : " + hit.transform.tag);

                if(hit.transform.tag == tag)
                {
                    return true;
                }
            }

            return false;
        }

        internal InventoryItem PickUpItem()
        {
            if (DetectItem())
            {
                if (hit.transform.GetComponent<ItemModel>())
                {
                    InventoryItem item = hit.transform.GetComponent<ItemModel>().item;

                    Destroy(hit.transform.gameObject);
                    return item;
                }

                return null;
            }
            else
                return null;
        }
    }
}

[System.Serializable][DataContract]
public struct SlotItem
{
    [SerializeField]
    InventoryItem item;
    public InventoryItem Item { get { return item; } internal set { item = value; } }


    [DataMember]
    public string ItemID
    {
        get
        {
            return itemID;
        }
        set
        {
            itemID = value;
        }
    }

    string itemID;

    [SerializeField][DataMember]
    uint count;
    public uint Count { get { return count; } internal set { count = value; } }

    public override bool Equals(object obj)
    {
        if (GetType() == obj.GetType())
        {
            SlotItem objItem = (SlotItem)obj;

            if (item == objItem.item && count == objItem.count)
                return true;
        }

        return false;
    }

    public static bool operator ==(SlotItem item1, SlotItem item2)
    {
        return item1.Equals(item2);
    }

    public static bool operator !=(SlotItem item1, SlotItem item2)
    {
        return !item1.Equals(item2);
    }

    public SlotItem(InventoryItem newItem, uint newCount)
    {
        itemID = Inventory.NoID;
        item = newItem;
        count = newCount;
    }
}
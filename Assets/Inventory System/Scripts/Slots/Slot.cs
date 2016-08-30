using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Slot : Selectable {
    [SerializeField]
    protected Image itemImage;
    public Image ItemImage { get { return itemImage; } }

    [SerializeField]
    protected Text itemCountT;
    public Text ItemCountT { get { return itemCountT; } }

    public SlotItem slotItem { get { return inventory[index].SlotItem; } }
    public bool Occupied { get { return inventory[index].Occupied; } }

    [SerializeField]
    protected internal int index;
    protected internal Inventory inventory;

    public virtual void UpdateSprite()
    {
        if (Occupied)
        {
            itemImage.enabled = true;
            itemImage.sprite = slotItem.Item.Sprite;
        }
        else
        {
            itemImage.enabled = false;
            itemImage.sprite = null;
        }
    }

    public virtual void UpdateCountText()
    {
        if (slotItem.Count > 1)
            itemCountT.text = slotItem.Count + "";
        else
            itemCountT.text = "";
    }
}
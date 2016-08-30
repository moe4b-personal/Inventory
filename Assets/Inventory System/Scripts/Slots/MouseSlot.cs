using UnityEngine;
using System.Collections;

public class MouseSlot : Slot {

    public Vector3 Position { get { return itemImage.rectTransform.position; } internal set { itemImage.rectTransform.position = value; } }

    public bool Visible { get { return gameObject.activeInHierarchy; } internal set { gameObject.SetActive(value); } }

    public void Set(SlotItem slotItem, Vector3 newPos)
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        Position = newPos;
        itemImage.sprite = slotItem.Item.Sprite;

        if (slotItem.Count > 1)
            itemCountT.text = slotItem.Count + "";
        else
            itemCountT.text = "";
    }

    public void Hide()
    {
        gameObject.SetActive(true);
    }
}
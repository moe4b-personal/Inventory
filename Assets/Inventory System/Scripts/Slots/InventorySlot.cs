using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public class InventorySlot : Slot, IBeginDragHandler, IDragHandler {
    [SerializeField]
    Image backgroundImage;

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == inventory.selectButton) //select button
        {
            if(!inventory.Changing) //first click
            {
                if(Occupied)
                {
                    inventory.CurrentItemIndex = index;
                    inventory.TargetItemIndex = Inventory.NoIndex;
                }
            }
            else //second click
            {

            }
        }
        else if(eventData.button == inventory.useButton) //use button
        {
            inventory.CurrentItemIndex = index;
        }
        else if(eventData.button == inventory.divideButton)
        {
            if(inventory[0].Count == 0)
            {
                inventory.CurrentItemIndex = index;
            }
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == inventory.selectButton) //select button
        {
            if(inventory.CurrentItemIndex != Inventory.NoIndex)
            {
                if (inventory.TargetItemIndex == Inventory.NoIndex) //no slot was selected
                {
                    inventory.TargetItemIndex = inventory.CurrentItemIndex;
                }
                else //a new slot was highlighted
                {
                    if (inventory.CurrentItemIndex == inventory.TargetItemIndex) //pressed the same slot twice
                    {
                        inventory.CurrentItemIndex = Inventory.NoIndex;
                        inventory.TargetItemIndex = Inventory.NoIndex;
                    }
                    else
                    {
                        if(inventory.SwitchIndex())
                        {
                            if(inventory.CurrentItemIndex == 0 && inventory[0].Count != 0) //switched between mouse and normal slot
                            {

                            }
                            else
                            {
                                inventory.CurrentItemIndex = Inventory.NoIndex;
                                inventory.TargetItemIndex = Inventory.NoIndex;
                            }
                        }
                    }
                }
            }
        }
        else if (eventData.button == inventory.divideButton) //divide button
        {
            if(inventory.Changing)
            {
                if (inventory.CurrentItemIndex != 0)
                {
                    if (inventory.TargetItemIndex == Inventory.NoIndex)
                    {
                        if (inventory.Divide(inventory.CurrentItemIndex, !Input.GetKey(inventory.singleItemDevideAssistKey)))
                            inventory.CurrentItemIndex = 0;
                    }
                }
                else
                {
                    inventory.SwitchIndex();

                    inventory.CurrentItemIndex = Inventory.NoIndex;
                    inventory.TargetItemIndex = Inventory.NoIndex;
                }
            }
        }
        else if(eventData.button == inventory.useButton) //use button
        {
            if(inventory.CurrentItemIndex != Inventory.NoIndex)
            {
                inventory.UseItem(index);

                inventory.CurrentItemIndex = Inventory.NoIndex;
            }
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (inventory.Changing)
        {
            inventory.TargetItemIndex = index;
        }

        else if (Input.GetKey(ButtonToKey(inventory.useButton))) //use button
        {
            inventory.CurrentItemIndex = index;
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (Input.GetKey(ButtonToKey(inventory.useButton))) //use button
        {
            inventory.CurrentItemIndex = Inventory.NoIndex;
        }
        else if(Input.GetKey(ButtonToKey(inventory.divideButton)))
        {
            inventory.TargetItemIndex = Inventory.NoIndex;
        }
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button == inventory.selectButton)
        {
            if (Occupied && inventory[0].Count == 0)
            {
                if (inventory.CurrentItemIndex != index) //dragged a different slot than what we selected
                {

                }

                inventory.SwitchIndex(0, index);
                inventory.CurrentItemIndex = 0;
            }
        }
        else if(eventData.button == inventory.divideButton)
        {
            if (inventory[0].Count == 0)
            {
                if (inventory.Divide(inventory.CurrentItemIndex, !Input.GetKey(inventory.singleItemDevideAssistKey)))
                    inventory.CurrentItemIndex = 0;
            }
        }
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {

    }

    public static KeyCode ButtonToKey(PointerEventData.InputButton button)
    {
        if (button == PointerEventData.InputButton.Left)
            return KeyCode.Mouse0;
        else if (button == PointerEventData.InputButton.Right)
            return KeyCode.Mouse1;
        else
            return KeyCode.Mouse2;
    }
}
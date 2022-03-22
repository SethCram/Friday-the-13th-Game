using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiOptions : MonoBehaviour
{
    //slot this options menu linked to:
    public InventorySlot slot;         //inited w/ this instantiated

    //linked to 'use' button:
    public void UseButton()
    {
        print("Use Button Pressed");

        //uses item if item in slot:
        slot.UseItem();
    }

    //linked to 'Drop' button:
    public void DropButton()
    {
        print("Drop Button Pressed");

        //drop item on ground, remove it from inventory, and clear slot:
        slot.SlotDrop();
    }

    //linked to 'examine' button:
    public void ExamineButton()
    {
        print("Examine Button Pressed");

        //shows item description at cursor position:
        slot.ShowItemDescription();
    }
}

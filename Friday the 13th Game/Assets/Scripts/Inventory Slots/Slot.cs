﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Slot : MonoBehaviour
{
    public Image icon; //to update our icon w/ add or remove item 
    public Button removeButton; //to show/hide remove button on each slot

    public TMP_Text itemCountTxt; //to show/hide and update item count 

    public Inventory inventoryAttachedTo { private set; get; } //inventory this slot is attached to 

    public virtual void Start() //can make a system def'd class overwritable
    {
        //init inventory this slot attached to:
        inventoryAttachedTo = GetComponentInParent<PausedUICallbacks>().playerInventory;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //update icon's enabled + whether it's currently interactive:
    public void AddSomethingToSlot()
    {
        //update our icon's enable:
        icon.enabled = true; //show inventory icon

        //show remove button:
        removeButton.interactable = true;
    }

    public virtual void OnRemoveButton()
    {
        //remove item:
        //inventoryAttachedTo.RemoveItemFromInventoryList(item);

        Debug.Log("Remove this item");
    }

    //clear slot by resetting the sprite, disabling the icon, and setting buttons as inactive:
    public virtual void ClearSlot()
    {
        //clear item:
           //done in inherited classes

        //clear icon:
        icon.sprite = null;
        icon.enabled = false;

        //clear remove button:
        removeButton.interactable = false;
    }

    // show/hide stack amt based on param:
    public void ShowStackAmt(bool showAmt)
    {
        if(showAmt)
        {
            Debug.Log("show amt");

            //show:
            itemCountTxt.color = Color.white;
        }
        else
        {
            //hide:
            itemCountTxt.color = Color.clear;
        }
    }
}

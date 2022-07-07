using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PausedUICallbacks : MonoBehaviour
{
    #region Variables 

    public Transform slotsParent; //stores parent transform of all inventory slots

    //for bag slots:
    [HideInInspector]
    public Inventory playerInventory; //made public to init before its instantiation
    private InventorySlot[] inventorySlots; //to store ref to bag slots
    private InventorySlot[] mediumSlots;
    private InventorySlot[] tinySlots;

    //for caching inventory lists:
    private List<Item> inventoryMediumList;
    private List<Item> inventoryTinyList;

    //for equipment:
    [HideInInspector]
    public EquipmentManager equipManager; //init this before instantiation
    private EquipmentSlotScript[] equipSlots;

    //for inventory stat display:
    [HideInInspector]
    public CharacterStats charStats; //*******init in UI_Instantiator***** (for setting names)
    [HideInInspector]
    public StatApplication statApply; //*******init in UI_Instantiator*****   (for updating vals)
    private InventoryStatDisplay[] inventoryStatDisplays;
    private Stat[] everyStat;      //need to make it a new array? (just for lists?)
    public GameObject StatDisplaysParent; //*****init in inspector***

    #endregion

    private void Start()
    {
        //fill array w/ all inventory slots:
        inventorySlots = slotsParent.GetComponentsInChildren<InventorySlot>();

        //split inventorySlots into tiny and med slots:
        tinySlots = new InventorySlot[playerInventory.maxTinySlots];
        mediumSlots = new InventorySlot[playerInventory.maxBagSlots];
        int tinyIndex = 0;
        int medIndex = 0;
        foreach (InventorySlot slot in inventorySlots)
        {
            if(slot.carrySize == CarrySize.Tiny)
            {
                tinySlots[tinyIndex] = slot;

                tinyIndex++;
            }
            else
            {
                mediumSlots[medIndex] = slot;

                medIndex++;
            }
        }

        //cache inventory lists:
        inventoryTinyList = playerInventory.tinyList;
        inventoryMediumList = playerInventory.bagList;

        //fill array w/ all equipment slots:
        equipSlots = slotsParent.GetComponentsInChildren<EquipmentSlotScript>();

        //fill inventory stat displays:
        inventoryStatDisplays = StatDisplaysParent.GetComponentsInChildren<InventoryStatDisplay>();

        //fill char stats array:
        everyStat = charStats.allStats;

        //fill inventory stat display names and vals:     (need to have same number of inventory stat displays as total stats)
        for (int i = 0; i < inventoryStatDisplays.Length; i++)
        {
            //set names:
            inventoryStatDisplays[i].statName.text = "" + everyStat[i].name;

            //set vals:
            inventoryStatDisplays[i].statValue.text = "" + everyStat[i].GetValue();
        }

        //sub method to add items to equipment slots on equip:
        equipManager.onEquipmentChangedCallback += UpdateEquipmentUI;

        //sub method to update stack UI w/ needed:
        playerInventory.OnStackAmtChangedCallback += UpdateStackUI;

        //sub method to update bag UI w/ needed:
        playerInventory.onItemChangedCallback += UpdateBagUI; //subscribe 'UpdateUI' method to the event 'onItemChangedCallback'

        //sub method to update inventory stat UI's:    (cant sub in this script bc then wont call this method until after stats set)
        statApply.onStatChangedCallback += UpdateInventoryStatDisplay;
    }

    #region Update Action Methods

    //update the bag UI everytime an item added/removed from the inventory:
    private void UpdateBagUI()
    {
        Debug.Log("Updating bag UI");

        //add every item in the inventory item list to an inventory slot, and 'clearSlot()' if not occupied by an item:
        for (int i = 0; i < tinySlots.Length; i++)
        {
            //fill slots w/ items from inventory item list:
            if (inventoryTinyList.Count > i)
            {
                tinySlots[i].AddItemToSlot(inventoryTinyList[i]);
            }
            //if cant bc more slots than current inventory items, fill extra slots w/ empties:s
            else
            {
                tinySlots[i].ClearSlot();
            }
        }

        //add every item in the inventory item list to an inventory slot, and 'clearSlot()' if not occupied by an item:
        for (int i = 0; i < mediumSlots.Length; i++)
        {
            //fill slots w/ items from inventory item list:
            if ( inventoryMediumList.Count > i)
            {
                mediumSlots[i].AddItemToSlot(inventoryMediumList[i]);
            }
            //if cant bc more slots than current inventory items, fill extra slots w/ empties:
            else
            {
                mediumSlots[i].ClearSlot();
            }
        }
    }

    //update the equipment UI everytime an equip added/removed from the EquipmentManager:
    private void UpdateEquipmentUI(Equipment newEquip, Equipment oldEquip) //needs these two args to sub to the callback
    {
        Debug.Log("Updating Inventory UI");

        //clear old slot, don't equip newEquip in UI if no new equipment or new equip is a default item: 
        if (newEquip == null || newEquip.isDefaultItem)
        {
            //dont clear slot if there's no old equip:
            if (oldEquip == null)
            {
                return;
            }

            //clear slot of old item that matches its equip slot:
            foreach (EquipmentSlotScript slot in equipSlots)
            {
                //clear old item's slot:
                if (oldEquip.equipSlot == slot.equipableslot)
                {
                    slot.ClearSlot();
                }
            }

            return;
        }

        //if item being equipt has an appropriate equipment slot, equip it to that slot: 
        foreach (EquipmentSlotScript slot in equipSlots)
        {
            //dont need to clear slot before adding to it, adding to it will overwrite previous equipment

            //equip equipment to correct slot:
            if (newEquip.equipSlot == slot.equipableslot)
            {
                slot.AddEquipToSlot(newEquip);
            }

        }
    }

    private void UpdateStackUI(Item updateItem, Equipment updateEquip, int newItemAmt)
    {
        Debug.Log("Update stack UI");

        //should update either inventory slot or equipment slot

        //specify whether updating equipment UI or bag UI:
        bool bagUI;
        if (updateItem == null)
        {
            bagUI = false;
        }
        else
        {
            bagUI = true;
        }

        //working w/ bag UI:
        if (bagUI)
        {

            //if same name and txt amt is less than max, update slot's max amt and return (so only update 1 slot):
            foreach (InventorySlot slot in inventorySlots)
            {
                if (slot.item != null)      //didnt need this before but need it now?
                {
                    if (slot.item.name == updateItem.name)
                    {
                        //if txt amt not more than max, set new txt amt and show/hide stack UI:
                        if (int.Parse(slot.itemCountTxt.text) < updateItem.maxStack)
                        {
                            slot.itemCountTxt.text = "" + newItemAmt; //converts int to string

                            //rehide item amt if under 2, show it if 2 or more:
                            if (newItemAmt < 2)
                            {
                                slot.ShowStackAmt(false);
                            }
                            else
                            {
                                slot.ShowStackAmt(true);
                            }

                            return;
                        }
                    }
                }
            }
        }
        else //working w/ equipment UI:
        {
            //if same name and txt amt is less than max, update slot's max amt and return (so only update 1 slot):
            foreach (EquipmentSlotScript slot in equipSlots)
            {
                if (slot.equip != null)
                {
                    if (slot.equip.name == updateItem.name)
                    {
                        //if txt amt not more than max, set new txt amt and show/hide stack UI:
                        if (int.Parse(slot.itemCountTxt.text) < updateEquip.maxStack)
                        {
                            slot.itemCountTxt.text = "" + newItemAmt; //converts int to string

                            //rehide item amt if under 2, show it if 2 or more:
                            if (newItemAmt < 2)
                            {
                                slot.ShowStackAmt(false);
                            }
                            else
                            {
                                slot.ShowStackAmt(true);
                            }

                            return;
                        }
                    }
                }
            }
        }
    }

    private void UpdateInventoryStatDisplay(Stat changedStat)
    {
        //loop thru all inventory stat displays:
        foreach (InventoryStatDisplay statDisplay in inventoryStatDisplays)
        {
            //if the name of one matches the one being changed:
            if(statDisplay.statName.text == changedStat.name)
            {
                //update its UI value:
                statDisplay.statValue.text = "" + changedStat.GetValue();
            }
        }
    }

    #endregion
}

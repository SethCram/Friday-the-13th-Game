using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Supclass of child script meant for equipment holding slot
/// </summary>
public class EquipmentSlotScript : Slot
{
    //type of equip slot:
    public EquipmentSlot equipableslot;

    public Equipment equip { private set; get; } //keeps track of curr equip in slot

    private EquipmentManager equipManager;
    
    public override void Start()
    {
        //init inventory:
        base.Start();

        //init equip manager:
        equipManager = inventoryAttachedTo.GetComponent<EquipmentManager>();
    }
    
    public void AddEquipToSlot(Equipment newEquip)
    {
        //set equip:
        equip = newEquip;

        //update our icon:
        icon.sprite = equip.icon; //set item icon to inventory icon

        //update icon's enabled + whether it's currently interactive:
        base.AddSomethingToSlot();
    }

    #region Removal Methods

    //clear slot of equipment:
    public override void ClearSlot()
    {
        //clear curr equip:
        equip = null;

        base.ClearSlot();
    }

    public override void OnRemoveButton()
    {
        base.OnRemoveButton();
    }

    //linked to slot button to unequip equipment to the inventory:
    public void UnequipToInventory()         //must be public to be accessed by button
    {
        //dont try and unequip if no equip present:
        if(equip == null)
        {
            return;
        }

        //check: Debug.Log("Remove equip at: " + equip.equipSlot);

        //remove equip and store either null or unequipt Equipment:
        Equipment unequiptEquipment = equipManager.Unequip((int)equip.equipSlot);     //false passed in bc not swapping equip

        //equip a default item in its place if successfully unequipped equipment:
        if (unequiptEquipment != null)                 //supposed to pass in as the equip slot index
        {
            equipManager.EquipDefaultItem(unequiptEquipment.equipSlot); //cant use 'equip' here bc the unequip method messes it up somehow
        }
    }

    #endregion Removal Methods
}
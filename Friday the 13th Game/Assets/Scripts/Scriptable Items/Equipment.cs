using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//can create new 'equipment' from same area creating new items:
[CreateAssetMenu(fileName = "New Equipment", menuName = "Inventory/Equipment")] 
public class Equipment : Item
{
    //public int equipSlot; //could be used to identify which equipment slot this piece is used in (head or chest, etc.), but not intuitive
    public EquipmentSlot equipSlot; //what slot this piece meant for

    //can't bc init order random: new public dynamic description = name;

    public SkinnedMeshRenderer mesh; //each item's 3d obj
    public EquipmentMeshRegion[] coveredMeshRegions;

    //equipment modifiers: (use these w/ create player stats)
    public int meleeModifier;
    public int rangedModifier;
    public int stealthModifier;
    public int bulkModifier;

    //to cache equip manager:   (also filled for inherited classes)
    public EquipmentManager equipManager { private set; get; }

    //for inherited classes to read from:
    public bool properlyEquipt { private set; get; } = false;

    //if use equipment, equips it:
    public override void Use(Inventory playerInventory)
    {
        //init equip manager as long as on same obj as Inventory:
        equipManager = playerInventory.GetComponent<EquipmentManager>();

        base.Use(playerInventory); //placeholder, just call og funct

        //remove this equip from the inventory: (remove before equip)
        RemoveFromInventory(playerInventory);                 // make sure to remove item from inventory before equip it so incase need slot to swap items (don't need to ref parent class bc this class derived from it)

        //equip the equipment w/ the curr player's equip manager:
        if( equipManager.Equip(this))                           //feed 'this' bc method needs arg of type 'Equipment'
        {
            properlyEquipt = true;
        }
        //if equip failed, re-add removed item back to inventory
        else
        {
            //if cant add to inventory
            if( !AddToInventory(playerInventory) )
            {
                //throw error
                Debug.LogError("Couldn't add " + this.name + " back to inventory after removing it.");
            }

            properlyEquipt = false;
        }
    }
}

//had to add a 'none' option since need to def this in 'Item' class:
public enum EquipmentSlot { Head, Chest, Legs, Feet, Mainhand, OffHand, Container, Ammo } //defing our own variable type, can be used in any class (each possible one has own index val)

public enum EquipmentMeshRegion { Legs, Arms, Torso }; //corresponds to body blendshapes
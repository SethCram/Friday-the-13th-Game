using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/EquipmentExtensions/Weapon")]
public class Weapon : Equipment
{
    //damage this weapon has:
    public int damage;

    //hands required to wield:
    public HandsRequired handsRequired;

    //specifies which stat we use for dmg calc w/ this weapon:
    public StatUsedForDamage statUsedForDamage;

    public string weaponContactSoundClipName;

    //call the 'Equipment' use method, not the 'Item' one (bc we wanna equip this):
    public override void Use(Inventory playerInventory)
    {
        //if 2 hands required:
        if(handsRequired == HandsRequired.two)
        {
            //remove weapon from inventory to free up space

            //equip weapon if can unequip both main and offhand   (rn, both failing to unequip and unequipping an empty slot returns null!)
        }

        //remove from inventory + equip this Equipment:
        base.Use(playerInventory);      // 'base' refers to inherited class's method

        //add to equip manager's 'currWeapons[]' if properly equipt in 'Equipment' script:
        if (properlyEquipt)
        {
            //equipManager = playerInventory.GetComponent<EquipmentManager>(); //already cached from base use call

            equipManager.currWeapons.Add(this);
        }

    }
}

public enum HandsRequired { one, two };

public enum StatUsedForDamage { Melee, Ranged}
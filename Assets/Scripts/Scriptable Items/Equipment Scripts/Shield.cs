using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Shield", menuName = "Inventory/EquipmentExtensions/Shield")]
public class Shield : Equipment
{
    //max shield health:
    public int maxShieldHP;

    //current shield health:
    public int currShieldHP;

    //hands required to wield:
    public HandsRequired handsRequired;

    public string weaponContactSoundClipName;

    //call the 'Equipment' use method, not the 'Item' one (bc we wanna equip this):
    public override void Use(Inventory playerInventory)
    {
        //remove from inventory + equip this Equipment:
        base.Use(playerInventory);      // 'base' refers to inherited class's method

        //add to equip manager's 'currShields' if properly equipt in 'Equipment' script:
        if (properlyEquipt)
        {
            equipManager.currShields.Add(this);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : CharacterStats
{
    //inited beforehand:
        //public PlayerManager playerManager; //unneeded?
    public EquipmentManager equipManager;
    public StatApplication statApply;

    /*
    //currently does nothing new
    public override void Die()
    {
        //says who died in console:
        base.Die();

            //Kill the player
            //  (death anim done in 'CharacterAnimator';
            //      could add gameover screen after player killed,
            //      then prompt to respawn w/ penalty) (we just restart lvl for now):

            //delay scene reset by _ secs so player death anim can play out
            // playerManager.Invoke("ResetScene", deathAnimDelay); 

        //delay scene reset by _ secs so player death anim can play out
        //playerManager.Invoke("ResetToMainMenu", deathAnimDelay); 
    }
    */

    // Start is called before the first frame update
    void Start()
    {
        equipManager.onEquipmentChangedCallback += EquipmentModsChanged;

        //test this player dying:
        //Invoke("Die", 5);
    }

    // add/remove stat effects granted/subtracted by worn equipment:
    private void EquipmentModsChanged(Equipment newEquip, Equipment oldEquip) //any methods subscribed to the callback have to take these args and have this name (so only 1 method can subscribe to the callback per class?)
    {

        //if any mod addition or subtraction done, 'StatApplication' script's callback invoked w/ the affected stat passed

        //add new mods:
        if (newEquip != null) //bc 'Unequip' method invokes the callback with a 'null' newItem, cant add mods to list w/ no new item
        {
            //check: print("new equip: " + newEquip.name);

            if ( statDict["Melee"].AddModifier(newEquip.meleeModifier) )
            {
                statApply.onStatChangedCallback.Invoke(statDict["Melee"]);
            }
           if( statDict["Ranged"].AddModifier(newEquip.rangedModifier))
            {
                statApply.onStatChangedCallback.Invoke(statDict["Ranged"]);
            }
           if( statDict["Stealth"].AddModifier(newEquip.stealthModifier) )
            {
                statApply.onStatChangedCallback.Invoke(statDict["Stealth"]);
            }
           if( statDict["Bulk"].AddModifier(newEquip.bulkModifier) )
            {
                statApply.onStatChangedCallback.Invoke(statDict["Bulk"]);
            }
        }

        //remove old mods:
        if(oldEquip != null)
        {
            //check: print("old equip: " + oldEquip.name);

            if ( statDict["Melee"].RemoveModifier(oldEquip.meleeModifier))
            {
                statApply.onStatChangedCallback.Invoke(statDict["Melee"]);
            }
           if( statDict["Ranged"].RemoveModifier(oldEquip.rangedModifier))
            {
                statApply.onStatChangedCallback.Invoke(statDict["Ranged"]);
            }
           if( statDict["Stealth"].RemoveModifier(oldEquip.stealthModifier))
            {
                statApply.onStatChangedCallback.Invoke(statDict["Stealth"]);
            }
           if( statDict["Bulk"].RemoveModifier(oldEquip.bulkModifier))
            {
                statApply.onStatChangedCallback.Invoke(statDict["Bulk"]);
            }
        }
    }
}

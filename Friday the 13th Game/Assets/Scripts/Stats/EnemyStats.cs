using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : CharacterStats
{
    public override void Die()
    {
        //print to console who died:
        base.Die();

        // add ragdoll effect/death anim

        //unsubscribe enemy method before destroying enemy with a delay for death anim:
        //EquipmentManager.instance.onEquipmentChangedCallback -= GetComponent<EnemyController>().OnEquipmentChanged;
        Destroy(gameObject, deathAnimDelay);

        //good place to add loot to ground
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

//create a 'CharacterStats' comp if one not already attached to obj:
[RequireComponent(typeof(CharacterStats))]
public class CharacterCombat : MonoBehaviourPun
{
    //const float combatCooldown = 5f;
    //private float lastAttackTime; //world time when last attack happened

    //needed to make sure cant open UI when atking:
    public bool isAtking = false;

    //quick, easy way to create a 'delegate' w/ a return type of void and an arg: (called 'Event method')
    public event System.Action<int> OnAttackCallback;                //just leave '<>' empty if no arg type

    private CharacterStats myStats;
    private CharacterStats opponentStats;

    //for dealing dmg:
    public EquipmentManager equipManager; //init in inspector
    private List<Weapon> equiptWeapons;
    private List<Shield> equiptShields;

    private void Start()
    {
        myStats = GetComponent<CharacterStats>();

        //cache weapon and shield lists:
        equiptWeapons = equipManager.currWeapons;
        equiptShields = equipManager.currShields;
    }

    //left mouse click atk:
    public void ActivateAttack(int atkIndex)//CharacterStats targetsStats)
    {
        //check: Debug.LogError("Attack activated");

        //opponentStats = targetsStats;

        //currently atking: (set to false in '3rdPersonMovement')
        isAtking = true;

        if(PhotonNetwork.IsConnected)
        {
            //animate correct atk on networked player:
            photonView.RPC("RPC_AnimateAttack", RpcTarget.All, atkIndex);
        }
        else
        {
            //signal attack started:
            if (OnAttackCallback != null) //if any methods subscribed to this event
            {
                OnAttackCallback(atkIndex); //dont have to 'Invoke()'
            }
        }

    }

    //do dmg to passed in stats:      
    public void DoDamage(CharacterStats hitStats)
    {
        /*
        if(!photonView.IsMine)
        {
            Debug.LogError("Dont do dmg bc we aren't the photon view owner.");

            return;
        }
        */


        //dmg dealt to hit stats:
        int damageDealt;

        //if dealing dmg w/ a weapon:
        if (equiptWeapons.Count > 0)
        {
            //find the main slot weapon:
            foreach (Weapon weapon in equiptWeapons)
            {
                if(weapon.equipSlot == EquipmentSlot.Mainhand)
                {
                    //if mainhand weapon uses melee for dmg:      
                    if (weapon.statUsedForDamage == StatUsedForDamage.Melee)
                    {
                        //calc dmg dealt based on melee stat + wapon dmg:
                        damageDealt = myStats.statDict["Melee"].GetValue() + weapon.damage;

                        //do damage w/ melee weapon
                        hitStats.TakeDamage(damageDealt);

                        return;
                    }
                    //if mainhand weapon uses ranged for dmg:
                    else if (weapon.statUsedForDamage == StatUsedForDamage.Ranged)
                    {
                        //calc dmg dealt based on ranged stat + wapon dmg:
                        damageDealt = myStats.statDict["Ranged"].GetValue() + weapon.damage;

                        //do damage w/ ranged weapon
                        hitStats.TakeDamage(damageDealt);

                        return;
                    }
                }
            }
        }
        /*
         * if dealing dmg w/ a shield:
        else if(equiptShields.Count > 0)
        {
            foreach (Shield shield in equiptShields)
            {
                if (shield.equipSlot == EquipmentSlot.Mainhand)
                {
                    //if mainhand weapon uses melee for dmg:      
                    if (shield.statUsedForDamage == StatUsedForDamage.Melee)
                    {

                    }
                    //if mainhand weapon uses ranged for dmg:
                    else if (shield.statUsedForDamage == StatUsedForDamage.Ranged)
                    {

                    }
                }
            }
        }
        */

        //dealing dmg w / punches:
        damageDealt = myStats.statDict["Unarmed"].GetValue() + 1;       //added 1 just incase no unarmed stat, so will actually deal "some" damage
        hitStats.TakeDamage(damageDealt);
    }

    //delay damage to allow attack anim to play out partially:
    /*
    private IEnumerator DoDamage(CharacterStats stats, float delay)
    {
        yield return new WaitForSeconds(delay);

        
        stats.TakeDamage(myStats.atk.GetValue());

        //not in combat if target dies:
        if(stats.currHealth <= 0)
        {
            inCombat = false;
        }
        
    }
    */

    /*
    //hit anim event passed on from 'CharacterAnimEventReceiver' script to deal damage:
    public void AttackHit_AnimEvent()
    {
        opponentStats.TakeDamage(myStats.statDict["Melee"].GetValue());         //assumes melee for now

        //not in combat if target dies:
        if (opponentStats.currHealth <= 0)
        {
            inCombat = false;
        }
    }
    */
}

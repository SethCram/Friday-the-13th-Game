using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerAnimator : CharacterAnimator //IPunObservable
{
    public EquipmentManager equipmentManager; // *****init in inspector***

    public WeaponAnims[] weaponAnims; //'WeaponAnims' type def'd below  ********def'd in inspector***************
    Dictionary<string, AnimationClip[]> weaponAnimsDict; //index is 'Equipment' type and it's associated type is 'AnimationClip[]'

    //link weapon to a set of attack animations:
    [System.Serializable] //so below method's local vars visible in inspector
    public struct WeaponAnims
    {
        public string weaponName;
        public AnimationClip[] clips;
    }

    protected override void Awake()
    {
        //call 'CharAnim' Awake():
        base.Awake();

        //Debug.LogError("Dictionary inited in Awake()");

        //initialize and populate attack anim dictionary:
        weaponAnimsDict = new Dictionary<string, AnimationClip[]>();
        foreach (WeaponAnims atkAnim in weaponAnims)
        {
            weaponAnimsDict.Add(atkAnim.weaponName, atkAnim.clips);
        }

        equipmentManager.onEquipmentChangedCallback += OnEquipmentChanged; //subscribe method
    }

    //Change active animator layer based on what equip changes:
    private void OnEquipmentChanged(Equipment newEquip, Equipment oldEquip) //framework for subscribable method
    {
        //check if new weapon equipt (for w/ equipping):
        if (newEquip != null && newEquip.equipSlot == EquipmentSlot.Mainhand)
        {
            print("New equip is: " + newEquip.name);

            //if new weapon has it's own set of atk anims, assign it to the curr atk anim set:
            if (weaponAnimsDict.ContainsKey(newEquip.name))
            {
                if(PhotonNetwork.IsConnected)
                {
                    //globally sync attack anim using its equipment's name:
                    photonView.RPC("SyncAttackAnim", RpcTarget.AllBuffered, newEquip.name);
                }
                else
                {
                    //set attack anims to weapon's:
                    currAtkAnimSet = weaponAnimsDict[newEquip.name];
                }
            }
        } //check if new item not a weapon, but old one was (for w/ unequipping):
        else if (newEquip == null && oldEquip != null && oldEquip.equipSlot == EquipmentSlot.Mainhand)
        {
            Debug.Log("Atk anims reset.");

            if(PhotonNetwork.IsConnected)
            {
                photonView.RPC("ResetAttackAnim", RpcTarget.AllBuffered);
            }
            else
            {
                //reset attack animations:
                currAtkAnimSet = defaultAtkAnimSet;
            }
        }
    }

    //sync attack anim using its equipment's name:
    [PunRPC]
    private void SyncAttackAnim(string equipName)
    {
        Debug.Log("Sync atk anim to: " + equipName);

        currAtkAnimSet = weaponAnimsDict[equipName];
    }

    [PunRPC]
    private void ResetAttackAnim()
    {
        //reset attack animations:
        currAtkAnimSet = defaultAtkAnimSet;
    }

    /*
    public void OnPhotonSerializeView(PhotonStream photonStream, PhotonMessageInfo message)
    {
        Debug.LogError("Serialize View called.");

        //synchronize the current atk anim:
        if(photonStream.IsWriting)
        {
            photonStream.SendNext((Object[])currAtkAnimSet);

            Debug.LogError("Sending: " + currAtkAnimSet.ToString());
        }
        else
        {
            currAtkAnimSet = (AnimationClip[])photonStream.ReceiveNext();

            Debug.LogError("Recieving: " + currAtkAnimSet.ToString());
        }
    }
    */

}

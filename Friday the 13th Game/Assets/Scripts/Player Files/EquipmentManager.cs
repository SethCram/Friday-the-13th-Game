﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EquipmentManager : MonoBehaviourPun
{

    //event setup:
    public delegate void OnEquipmentChanged(Equipment newItem, Equipment oldItem); //any methods subscribed to the below def'd 'callback' take these args and need to have this name?
    public OnEquipmentChanged onEquipmentChangedCallback; //other methods can subscribe to this

    //to keep track of currently worn equip:
    public Equipment[] currEquipment; //array of equipment slots
    public SkinnedMeshRenderer[] currMeshes; //array of currently equipt meshes
    public List<Weapon> currWeapons; //at most will have 2 weapons, 1 in mainhand and 1 in offhand     (will have to have 2 in scene, one obj for mainhand and 1 for offhand)
    public List<Shield> currShields; //at most will have 2 shields, 1 in mainhand and 1 in offhand     (will have to have 2 in scene, one obj for mainhand and 1 for offhand)

    //all our basic default equip ************should be filled in inspector**********:
    public Equipment[] defaultEquipment;

    //mesh of player equip manager is targeting:
    public SkinnedMeshRenderer playerMesh;

    //arm used to register attacks:
    public GameObject atkArmObject;

    private Inventory inventory; //for caching inventory

    private void Start()
    {
        //make sure the default equipment array is filled, if not output an error:
        if(defaultEquipment.Length > 0)
        {
            foreach (Equipment clothing in defaultEquipment)
            {
                if(clothing == null)
                {
                    Debug.LogError("Default Equipment array not filled");
                }
            }
        }
        else
        {
            Debug.LogError("Default Equipment array not filled");
        }

        //init player inventory:
        inventory = GetComponent<Inventory>();

        //sub method to callback:
        onEquipmentChangedCallback += ChangeArmMesh;

        //System.Enum.GetNames(typeof(EquipmentSlot)); //gets all enums of specified enum type

        //set number of equipment slots:
        int numOfEquipSlots = System.Enum.GetNames(typeof(EquipmentSlot)).Length; //number of slots = length of 'EquipmentSlot' enum

        //initialize array sizes:
        currEquipment = new Equipment[numOfEquipSlots];
        currMeshes = new SkinnedMeshRenderer[numOfEquipSlots];

        EquipAllDefaultItems();
    }
    
    //equip new item if possible by;       using its equip slot's index, calling the equipChangedCallback, activating equip in scene, both equip and mesh arrays, and deforming the correct body part:
    public bool Equip(Equipment newEquip) //equip passed in 'newItem', but placement matters
    {
        int slotIndex = (int)newEquip.equipSlot; //enum converted to int to find its correct placement slot (add 1 bc added 'none' field?)

        //unequip old item at this slot index:
        Equipment oldItem = Unequip(slotIndex, true); //pass in true bc we're swapping items

        //don't equip new item if can't unequip previous item:
        if(oldItem == null && currEquipment[slotIndex] != null)
        {
            Debug.Log("Couldn't unequip previous equipment");

            return false;
        }

        //caching new mesh call:
        SkinnedMeshRenderer newMesh = newEquip.mesh; //my replacement

        //activate new mesh:
        if(PhotonNetwork.IsConnected)
        {
            //use RPC to enable item on all chars:
            photonView.RPC("RPC_ChangeMeshActive", RpcTarget.AllBuffered, newEquip.mesh.name, true);        
        }
        else
        {
            //enable item locally:
            ChangeMeshActive(newEquip.mesh.name, playerMesh.transform.parent, true);
        }

        //fill mesh array w/ equipt mesh:
        currMeshes[slotIndex] = newMesh;

        //fill equip array w/ equipt mesh:
        currEquipment[slotIndex] = newEquip;

        //deform body for new item:
        SetEquipmentBlendShapes(newEquip, 100);

        // w/ equipment changes, call methods:
        if (onEquipmentChangedCallback != null) //if any methods subscribed to call back
        {
            onEquipmentChangedCallback.Invoke(newEquip, oldItem); //run methods subscribed
        }

        return true;
    }

    //unequip equipment at specified slot index, if possible returns the old item, if not or nothing to unequip returns null:
    public Equipment Unequip(int slotIndex, bool swapping = false) //unequip item at 'slotIndex', bool to signify if swapping equip (automatically set to false, so dont need to pass in two args unless swapping items)
    {
        //if specified equipment slot is empty, return null bc we dont unequip anything:
        if (currEquipment[slotIndex] == null)
        {
            return null;
        }

        //errror check:
        if(currMeshes[slotIndex] == null)
        {
            Debug.Log("Current mesh slot is empty but it's equipment slot isn't??");
        }

        //if something in both the mesh and equipment slot:

        //store old item:
        Equipment oldEquip = currEquipment[slotIndex];

        //try adding it to our inventory:
        bool itemAddedtoInventory;
        itemAddedtoInventory = inventory.AddItemToInventoryList(oldEquip); //add item back into inventory + store if successful

        //if cant add item to inventory and it isnt a default item, return null:
        if(!(itemAddedtoInventory) && !(oldEquip.isDefaultItem))
        {
            return null;
        }
        else //otherwise clear the equipment and mesh slot and disable the item in-scene:
        {
            //disable item in-scene:
            if (PhotonNetwork.IsConnected)
            {
                //use RPC to disable item on all chars:
                photonView.RPC("RPC_ChangeMeshActive", RpcTarget.AllBuffered, currMeshes[slotIndex].name, false);             
            }
            else
            {
                //disable item locally:
                ChangeMeshActive(currMeshes[slotIndex].name, playerMesh.transform.parent, false);
            }

            //reset blend shape disconfig:
            SetEquipmentBlendShapes(oldEquip, 0);

            //clear slots:
            currEquipment[slotIndex] = null;
            currMeshes[slotIndex] = null;
        }

        //if unequipt item is a mainhand or offhand item:
        if(oldEquip.equipSlot == EquipmentSlot.Mainhand || oldEquip.equipSlot == EquipmentSlot.OffHand)
        {
            //try removing oldEquip from currWeapon list:
            if(!(RemoveFromCurrWeaponList(oldEquip)))
            {
                //then if fail try currShield list:
                RemoveFromCurrShieldList(oldEquip);
            }

            /*
            //loop thru shields list:
            foreach (Shield shield in currShields)
            {
                //remove shield from currShields if unequipping it:
                if (shield.name == oldEquip.name)
                {
                    currShields.Remove(shield);
                }
            }
            */
        }

        //equipment has been changed:
        if (onEquipmentChangedCallback != null && !(swapping)) //if any methods subscribed to call back
        {
            onEquipmentChangedCallback.Invoke(null, oldEquip); //run methods subscribe (no newItem bc unequiping an item)
        }

        return oldEquip;
    }

    #region HelperMethods

    //if isWeapon set to false, removing from Shield List, else try and remove from weapon list
    private bool RemoveFromCurrWeaponList(Equipment oldEquip)
    {
        //loop thru weapons list:
        foreach (Weapon weapon in currWeapons)
        {
            //remove weapon from currWeapons if unequipping it:
            if (weapon.name == oldEquip.name)
            {
                currWeapons.Remove(weapon);
        
                return true;
            }
        }

        //if make it to the bottom, weapon not in array:
        return false;
    }

    private bool RemoveFromCurrShieldList(Equipment oldEquip)
    {
        //loop thru shields list:
        foreach (Shield shield in currShields)
        {
            //remove shield from currShields if unequipping it:
            if (shield.name == oldEquip.name)
            {
                currShields.Remove(shield);

                return true;
            }
        }

        //shield wasnt equipt to remove from list:
        return false;
    }

    [PunRPC]
    private void RPC_ChangeMeshActive(string objName, bool itemState)
    {
        //Debug.LogError("Find: " + objName);

        ChangeMeshActive(objName, playerMesh.transform.parent, itemState);
    }

    //set gameobj w/ obj name to itemState:
    private void ChangeMeshActive(string objName, Transform parent, bool itemState)
    {
        foreach (Transform child in parent)
        {
            //debug: print("child name: " + child.name + " and new mesh name: " + newMesh.name);

            if (child.name == objName)
            {
                child.gameObject.SetActive(itemState);

                //check: Debug.LogError(child.name + " had visibility set to: " + itemState);

                break; //stop looking after mesh found
            }
            ChangeMeshActive(objName, child, itemState); //uses recursion for each item, so seems pretty unnecessarily intensive 
        }
    }    

    //unequip all curr equipment:
    public void UnequipAll()
    {
        for (int i = 0; i < currEquipment.Length; i++) //loop thru all equipment slots
        {
            Unequip(i);    //false bc not swapping equip
        }

        EquipAllDefaultItems(); //equipts default items regardless of if all items were able to successfully unequip
    }

    //deform body to move smoothly with armor/clothes:
    private void SetEquipmentBlendShapes(Equipment item, int weight)
    {
        foreach (EquipmentMeshRegion blendShape in item.coveredMeshRegions) //loops thru each blendshape region covered by passed in item
        {
            playerMesh.SetBlendShapeWeight((int)blendShape, weight); //set that blendshape weight to desired (usually 100 w/ equiping, and 0 w/ unequiping)
        }
    }

    //equip every piece of default equipment:
    private void EquipAllDefaultItems()
    {
        foreach (Equipment equipmentPiece in defaultEquipment)
        {
            Equip(equipmentPiece);
        }
    }
    #endregion

    //equip the default equip with the passed in equipment slot:
    public void EquipDefaultItem(EquipmentSlot equipSlot)
    {
        foreach (Equipment equipmentPiece in defaultEquipment)
        {
            //compare equipment slot w/ the passed in one:
                   //if (equipmentPiece.equipSlot == equipSlot) //cant directly compare enums
            if(equipmentPiece.equipSlot.CompareTo(equipSlot) == 0)
            {
                Equip(equipmentPiece);
            }
        }
    }

    private void Update()
    {
        //temporary, test for unequipping:
        if(Input.GetKeyDown(KeyCode.U))
        {
            UnequipAll();
        }
    }

    //method subbed to on equip changed callback:
    private void ChangeArmMesh(Equipment newEquip, Equipment oldEquip)
    {
        //if not equipping new equip:
        if (newEquip == null)
        {
            //if unequipping a mainhand:
            if (oldEquip.equipSlot == EquipmentSlot.Mainhand)
            {
                //reactivate arm as weapon:
                atkArmObject.SetActive(true);
            }

            //return so if no new item we dont evaluate it:
            return;
        }

        //if equipping a mainhand:
        if (newEquip.equipSlot == EquipmentSlot.Mainhand)
        {
            //deactivate arm as a weapon:
            atkArmObject.SetActive(false);
        }

        /*
        //if swapping, new equip isnt mainhand, and old equip was mainhand: (never executed)
        if (newEquip.equipSlot != EquipmentSlot.Mainhand && oldEquip.equipSlot == EquipmentSlot.Mainhand)
        {
            //reactivate arm as weapon:
            atkArmObject.SetActive(true);
        }
        */
    }
}

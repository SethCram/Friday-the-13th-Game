﻿using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

//req sphere for interacting
[RequireComponent(typeof(SphereCollider))]
public class ItemPickup : Interactable //this class is now derived from/a child of the 'Interactable' class (but since not a child of monobehavior, cant access any unity functs? (incorrect bc the class we derive from derives from monobehavior) )
{
    public Item item;

    [HideInInspector]
    public bool shouldDelete; //actually want to delete obj on destroy

    private GameObject itemIconCopy;

    public override void Start()
    {
        base.Start();

        //create item's icon in world space for rendering
        itemIconCopy = new GameObject(name: "pickupIcon");
        //fill sprite field
        itemIconCopy.AddComponent<SpriteRenderer>().sprite = item.icon;
        //rot so can see icon
        itemIconCopy.transform.eulerAngles = new Vector3( 90, 
                                                        itemIconCopy.transform.eulerAngles.y, 
                                                        itemIconCopy.transform.eulerAngles.z);
        //set to minimap layer
        itemIconCopy.layer = 12; 
        //set to spawn position with 100 in the y
        itemIconCopy.transform.position = new Vector3(transform.transform.position.x, 
                                                        100, 
                                                        transform.transform.position.z); 
        //scale icon up so visible on minimap
        itemIconCopy.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f); 
    }

    //override og 'Interact()' to have the interacting player 'Pickup()' the item:
    public override void Interact(Transform playerInteracting)
    {
        base.Interact(playerInteracting); //calls 'Interactable' Interact() method

        /*
        //for every interactable player
        foreach (Transform player in interactablePlayers)
        {
            //make msgs blank
            player.GetComponent<PlayerManager>().SetInteractMsg("");

            //make msgs dissapear
            player.GetComponent<PlayerManager>().SetInteractVisibility(false);
        }
        */

        //Debug.Log(interactablePlayers.ToString());
        
        //dissapear interact msg (what if someone else picks up while we in range??)
        //playerInteracting.GetComponent<PlayerManager>().photonView.RPC("SetInteractVisibility", RpcTarget.Others, false);
        playerInteracting.GetComponent<PlayerManager>().SetInteractVisibility(false);

        Pickup(playerInteracting);
    }

    //add item to interacting player's inventory and destroy its scene obj:
    private void Pickup(Transform playerInteracting)
    {
        Debug.Log("Picking up " + item.name);

        // Add to interacting player's inventory:
        bool wasPickedUp = playerInteracting.GetComponent<Inventory>().AddItemToInventoryList(item); //add item to list w/ pickup

        //if picked up, destroy scene obj:
        if (wasPickedUp)
        {
            //if connected to photon network:
            if (PhotonNetwork.IsConnected)
            {
                //tell every scene to set this pickup's delete:
                photonView.RPC("SetDelete", RpcTarget.All);

                //playerInteracting.GetComponent<PlayerManager>().LocalDestroyPhotonView(gameObject.GetPhotonView());

                //pass photon view to destroy its gameobj globally:
                playerInteracting.GetComponent<PlayerManager>().GlobalDestroyPhotonView(gameObject.GetPhotonView());
            }
            else
            {
                //destroy obj for interacting player:
                Destroy(gameObject);
            }
            
        }
    }

    [PunRPC]
    private void SetDelete()
    {
        //tell Item pickup that item should be deleted on desctruction:
        shouldDelete = true;
    }

    new private void OnDisable()
    {
        //if item icon created
        if ( itemIconCopy != null )
        {
            //disable icon
            itemIconCopy.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        //if item icon created
        if( itemIconCopy != null )
        {
            //destroy it locally
            Destroy(itemIconCopy);
        }

        //check: Debug.LogError(item.name + " destroyed");

        //if photon view not setup yet:
        if (photonView == null)
        {
            Debug.LogWarning("Photon view not yet setup, so " + item.name + " not destroyed.");

            return;
        }

        //if not connceted to network:
        if (!PhotonNetwork.IsConnected)
        {
            return;
        }

        //if this is our photon view: (dont need to re-create our own obj since we leaving)
        if (photonView.IsMine)
        {
            Debug.Log("Didnt re-create item bc its our photon view.");

            return;
        }

        //if no players left in room: (dont need?)
        if (PhotonNetwork.CountOfPlayers <= 0)
        {
            Debug.Log("Didnt re-create item bc players in room = " + PhotonNetwork.CountOfPlayers);
            return;
        }


        //if scene item shouldnt be deleted:
        if (!(shouldDelete))
        {
            GameObject itemPickup = item.itemPickup;

            //re-create in its old spot:
            PhotonNetwork.InstantiateRoomObject(itemPickup.name, transform.position, transform.rotation);

            Debug.LogWarning("re-created: " + itemPickup.name);
        }
    }

    public override string getGuiMsg(bool isOpen)
    {
        return "Press E/Fire1 to Pickup";
    }

    
}

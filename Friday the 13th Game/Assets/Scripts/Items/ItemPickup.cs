using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ItemPickup : Interactable //this class is now derived from/a child of the 'Interactable' class (but since not a child of monobehavior, cant access any unity functs? (incorrect bc the class we derive from derives from monobehavior) )
{
    public Item item;

    [HideInInspector]
    public bool shouldDelete; //actually want to delete obj on destroy

    //override og 'Interact()' to have the interacting player 'Pickup()' the item:
    public override void Interact(Transform playerInteracting)
    {
        base.Interact(playerInteracting); //calls 'Interactable' Interact() method

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

    private void OnDestroy()
    {
        //check: Debug.LogError(item.name + " destroyed");

        //if photon view not setup yet:
        if(photonView == null)
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

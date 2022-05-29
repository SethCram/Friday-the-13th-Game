using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class AnimateDissapear : Entryway
{
	public GameObject[] dissapearingGameobjs;

    //override og 'Interact()' to have the interacting player 'animate' the item:
    public override void Interact(Transform playerInteracting)
    {
        base.Interact(playerInteracting); //calls 'Interactable' Interact() method

		//invert isOpen
		isOpen = !isOpen;

		//should invert state of entryway
		if (PhotonNetwork.IsConnected)
		{
			//over network
			photonView.RPC("RPC_InvertState", RpcTarget.AllBufferedViaServer, !isOpen);

		}
		else
		{
			//local
			RPC_InvertState(!isOpen);
		}

		//msg = getGuiMsg(!isOpen);
	}

	[PunRPC]
	private void RPC_InvertState(bool openState)
    {

		foreach (GameObject invObj in dissapearingGameobjs)
		{
			Debug.Log("Gameobj " + invObj.name + " should invert states");

			//should invert state of entryway
			invObj.SetActive(openState);
		}
	}

	public override string getGuiMsg(bool isOpen)
	{
		string rtnVal;
		if (isOpen)
		{
			rtnVal = "Press E/Fire1 to Close";
		}
		else
		{
			rtnVal = "Press E/Fire1 to Open";
		}

		return rtnVal;
	}
}

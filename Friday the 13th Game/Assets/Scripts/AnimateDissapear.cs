using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class AnimateDissapear : Interactable
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
			photonView.RPC("RPC_InvertState", RpcTarget.AllBuffered);

		}
		else
		{
			//local
			RPC_InvertState();
		}

		//msg = getGuiMsg(!isOpen);
	}

	[PunRPC]
	private void RPC_InvertState()
    {
		foreach (GameObject gameobj in dissapearingGameobjs)
		{
			//should invert state of entryway
			gameobj.SetActive(!isOpen);
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

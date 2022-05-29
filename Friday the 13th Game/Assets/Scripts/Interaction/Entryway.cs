using Photon.Pun;
using UnityEngine;

public class Entryway : Interactable
{
    //pickups in container
    public GameObject[] pickups;

    public override void Interact(Transform playerInteracting)
	{
		base.Interact(playerInteracting); //calls 'Interactable' Interact() method

		//should invert state of entryway
		if (PhotonNetwork.IsConnected)
		{
			//over network
			photonView.RPC("RPC_InvertPickups", RpcTarget.AllBufferedViaServer, !isOpen);

		}
		else
		{
			//local
			RPC_InvertPickups();
		}
	}

	[PunRPC]
	private void RPC_InvertPickups()
    {
		//if no pickups
		if( pickups == null || pickups.Length == 0)
        {
			//dont invert anything
			return;
        }

		//for each pickup
		foreach (GameObject pickup in pickups)
		{
			//make sure pickup hasnt been removed
			if( pickup != null)
            {
				//invert its state
				pickup.SetActive(!pickup.activeSelf);
			}
		}
	}
}

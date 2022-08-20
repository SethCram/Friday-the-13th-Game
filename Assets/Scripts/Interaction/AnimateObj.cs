using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// animate this entryway
/// </summary>
public class AnimateObj : Entryway
{
    #region Vars

    private Animator anim;

	private const string animBoolName = "isOpen_Obj_";

	private MoveableObject moveableObject;
	private string animBoolNameNum = "";

    #endregion Vars

    #region Unity Methods

    public override void Start()
    {
		//init player list + collider
        base.Start();

		//create AnimatorOverrideController to re-use animationController for sliding draws.
		anim = GetComponent<Animator>();
		anim.enabled = false;  //disable animation states by default.

		moveableObject = GetComponentInChildren<MoveableObject>(); //have to get moveable obj in children too?
	}

    private void LateUpdate() //doesnt override superclass update bc private
    {
		//if (playerEntered)
		if (interactablePlayers.Count > 0) //players within interaction range
		{
			animBoolNameNum = animBoolName + moveableObject.objectNumber.ToString();

			isOpen = anim.GetBool(animBoolNameNum);    //need current state for message.
		}

	}

	#endregion Unity Methods

	//override og 'Interact()' to have the interacting player 'animate' the item:
	public override void Interact(Transform playerInteracting)
    {

        base.Interact(playerInteracting);

		//should invert state of entryway
		if ( PhotonNetwork.IsConnected)
        {
			//over network + stays after this player leaves
			photonView.RPC("RPC_InvertAnim", RpcTarget.AllBufferedViaServer, animBoolNameNum, !isOpen);
		}
        else
        {
			//local
			RPC_InvertAnim(animBoolNameNum, !isOpen);
		}

	}

	//should invert state of entryway
	[PunRPC]
	private void RPC_InvertAnim(string animName, bool openState)
    {
		Debug.Log("Aimation " + anim.name + " should play");

		//verify local isOpen
		//isOpen = !openState;

		//animate
		anim.enabled = true;
		anim.SetBool(animName, openState);
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

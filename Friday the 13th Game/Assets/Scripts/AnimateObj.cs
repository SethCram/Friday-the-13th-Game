using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateObj : Interactable
{
	private Animator anim;

	private const string animBoolName = "isOpen_Obj_";

	private MoveableObject moveableObject;
	private string animBoolNameNum = "";

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

    //override og 'Interact()' to have the interacting player 'animate' the item:
    public override void Interact(Transform playerInteracting)
    {
        base.Interact(playerInteracting); //calls 'Interactable' Interact() method

		//should invert state of entryway
		if ( PhotonNetwork.IsConnected)
        {
			//over network
			photonView.RPC("RPC_InvertAnim", RpcTarget.AllBuffered);

		}
        else
        {
			//local
			RPC_InvertAnim();
		}

	}

	//should invert state of entryway
	[PunRPC]
	private void RPC_InvertAnim()
    {
		anim.enabled = true;
		anim.SetBool(animBoolNameNum, !isOpen);
	}

	public override void OnTriggerEnter(Collider other)
	{
		//add to player list
		base.OnTriggerEnter(other);

		if (other.tag == "Player" || other.tag == "Enemy")     //player has collided with trigger
		{
			//playerEntered = true;

		}
	}

	public override void OnTriggerExit(Collider other)
	{
		//rm from player list
		base.OnTriggerExit(other);

		if (other.tag == "Player" || other.tag == "Enemy")     //player has exited trigger
		{
			//playerEntered = false;

			//hide interact message as player may not have been looking at object when they left
			//showInteractMsg = false;
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

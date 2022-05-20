using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerButtons : MonoBehaviourPun
{
    //init in inspector:
        //public CharacterCombat combat;    //needed to check if atking

    public bool playerInteract = false;
        //public bool openInventory = false;
        //public bool openOptions = false;

    // Update is called once per frame
    void Update()
    {
        if(PhotonNetwork.IsConnected && !photonView.IsMine)
        {
            return;
        }

        //if player presses 'Interact', set player interact to true for one 'Update()', then reset it:
        playerInteract = false;
        if(Input.GetButtonDown("Interact"))
        {
            playerInteract = true;
        }
        /*

        //if player grounded and not atking, not dodging, allow access to 'paused' UI:
        if (playerMovement.isGrounded && !(combat.isAtking) && !(playerMovement.isDodging))
        {
            AccessPausedUI();
        }
        else
        {
            //Debug.LogWarning("Access to pause UI not allowed.");
        }
        */
    }

    /*
    private void AccessPausedUI()
    {
        //Debug.LogWarning("Access to pause UI allowed.");

        //if press inventory button, invert its open state for one 'Update()', then reset it:
        openInventory = false;
        if (Input.GetButtonDown("Inventory"))
        {
            //invert pause state
            //paused = !paused;

            openInventory = true;
        }

        //if press options button, invert its open state for one 'Update()', then reset it:
        openOptions = false;
        if (Input.GetButtonDown("Options"))
        {
            // invert pause state
            //paused = !paused;

            openOptions = true;
        }
    }
    */
}

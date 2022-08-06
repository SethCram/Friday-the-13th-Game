﻿using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoseTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        //store colliding w/ transform
        Transform collidingWithTransform = other.GetComponent<Transform>();
        PlayerManager playerManager = null;

        //if colliding w/ jason or counselor
        if (other.tag == "Player" || other.tag == "Enemy")
        {
            //store player manager
            playerManager = other.GetComponent<PlayerManager>();

        }

        //if in the game scene
        if (GameManager.Instance.currentScene == GameManager.CurrentScene.GAME)
        {
            //if counselor colliding + player not dead
            if (other.tag == "Player")
            {
                //if other photon view is mine or not connected to network
                if (other.GetComponent<PhotonView>().IsMine || !PhotonNetwork.IsConnected)
                {
                    //cause counselor to lose + check if all counselors dead
                    playerManager.CounselorDied(playerWon: false);
                }

            }
            //if jason colliding
            else if (other.tag == "Enemy")
            {
                //if other photon view is mine or not connected to network
                if (other.GetComponent<PhotonView>().IsMine || !PhotonNetwork.IsConnected)
                {
                    //tell jason died + lost
                    playerManager.JasonDied(playerWon: false);
                }
            }
        }
        //if in game lobby scene
        else if (GameManager.Instance.currentScene == GameManager.CurrentScene.GAME_LOBBY)
        {
            //show generic death screen 
            playerManager.GenericDeathScreen();
        }
    }
}

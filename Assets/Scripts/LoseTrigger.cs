using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoseTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerManager playerManager = null;

        //if colliding w/ jason or counselor or player
        if (GameManager.Instance.TagIsPlayableCharacter(other.tag) )
        {
            //store player manager
            playerManager = other.GetComponent<PlayerManager>();

        }

        //if in the game scene
        if (GameManager.Instance.currentScene == GameManager.CurrentScene.GAME)
        {
            //if counselor colliding + player not dead
            if (GameManager.Instance.TagIsCounselor(other.tag))
            {
                //if other photon view is mine or not connected to network
                if (other.GetComponent<PhotonView>().IsMine || !PhotonNetwork.IsConnected)
                {
                    //cause counselor to lose + check if all counselors dead
                    playerManager.CounselorDied(localPlayerWon: false);
                }

            }
            //if jason colliding
            else if (GameManager.Instance.TagIsJason(other.tag))
            {
                //if other photon view is mine or not connected to network
                if (other.GetComponent<PhotonView>().IsMine || !PhotonNetwork.IsConnected)
                {
                    //tell jason died + lost
                    playerManager.JasonDied(localPlayerWon: false);
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

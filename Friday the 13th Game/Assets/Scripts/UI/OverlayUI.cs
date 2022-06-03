using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class OverlayUI : MonoBehaviour
{
    public TMP_Text interactTxt;
    public GameObject minimap;

    //for slider
    public Slider healthSlider;
    public TMP_Text healthRatio;
    public GameObject fillAmt;

    //for starting game
    public Toggle voteToggle;
    //[HideInInspector]
    //public int startVotes = 0; //needa incr/decr over RPC
    public PlayerManager playerManager;
    private bool startedGame = false;

    private void Awake()
    {
        //start w/ interact txt off
        interactTxt.gameObject.SetActive(false);

        //start w/ minimap off bc driven by if perception in range
        minimap.SetActive(false);

        //start w/ health slider off
        healthSlider.gameObject.SetActive(false);
    }

    //update health slider using HP changed event
    public void UpdateHealthSlider(int maxHP, int currHP)
    {
        print("Update HP slider");

        //if no HP left
        if( currHP <= 0)
        {
            //delete fill amt
            fillAmt.SetActive(false);
        }

        //update filled amt (float casts needed)
        healthSlider.value = (float)currHP / (float)maxHP;

        //update ratio text
        healthRatio.text = currHP + "/" + maxHP;
    }

    private void Update()
    {
        //if started game
        if( startedGame )
        {
            //dont allow voting
            return;
        }

        //if press V
        if( Input.GetKeyDown(key:KeyCode.V))
        {
            //invert toggle
            voteToggle.isOn = !voteToggle.isOn;

            if( PhotonNetwork.IsConnected)
            {
                //if now voting to start
                if (voteToggle.isOn == true)
                {
                    //should incr votes over RPC so works for everyone
                    playerManager.photonView.RPC("RPC_ChangeVoteCount",
                        RpcTarget.AllBufferedViaServer,
                        playerManager.startVotes + 1);
                }
                //not voting to start
                else
                {
                    //should descr votes over RPC so works for everyone
                    playerManager.photonView.RPC("RPC_ChangeVoteCount",
                        RpcTarget.AllBufferedViaServer,
                        playerManager.startVotes - 1);
                }
            }
            else
            {
                //if now voting to start
                if (voteToggle.isOn == true)
                {
                    //should incr votes over RPC so works for everyone
                    playerManager.RPC_ChangeVoteCount(playerManager.startVotes + 1);
                }
                //not voting to start
                else
                {
                    //should descr votes over RPC so works for everyone
                    playerManager.RPC_ChangeVoteCount(playerManager.startVotes - 1);
                }
            }

            if(PhotonNetwork.IsConnected)
            {
                //if all players voted to start
                if (playerManager.startVotes >= PhotonNetwork.CurrentRoom.PlayerCount)
                {
                    //Start game for everyone
                    Debug.Log("Should start game.");
                    StartGame();
                }
            }
            else
            {
                //if voted to start
                if( playerManager.startVotes >= 1)
                {
                    //Start game locally
                    Debug.Log("Should start game.");
                    StartGame();
                }
            }
            
        }
    }

    /// <summary>
    /// start game thru disabling vote toggle UI + moving players
    /// </summary>
    public void StartGame()
    {
        //disable voting UI
        voteToggle.gameObject.SetActive(false);

        startedGame = true;

        //change levels:
        playerManager.ChangeLevels();

        //move players to game level
        MovePlayers();
    }

    /// <summary>
    /// move players to start of actual game
    /// </summary>
    private void MovePlayers()
    {
        if(!PhotonNetwork.IsConnected)
        {
            playerManager.player.transform.position = playerManager.counselorStart.position;
            return;
        }

        //pick rando # tween 1 and number of players in room
        int enemyIndex = Random.Range(min:0, max:PhotonNetwork.PlayerList.Length);

        //find all players
        //GameObject[] playerList = GameObject.FindGameObjectsWithTag("Player");
        Player[] playerList = PhotonNetwork.PlayerList;

        //[enemyIndex].tag = "Enemy";

        //walk thru player list
        for (int i = 0; i < playerList.Length; i++)
        {

            //if index is enemy index
            if( i == enemyIndex )
            {
                //change player's tag to enemy tag
                //playerList[enemyIndex].tag = "Enemy";

                //teleport enemy to enemy start loc
                //playerManager.photonView.RPC("RPC_StartJason", playerList[i]);
                playerManager.StartJasonWrapper(playerList[i]);
                
            }
            //if not enemy
            else
            {
                //teleport players to player start
                //playerManager.photonView.RPC("RPC_StartCounselor", playerList[i]);
                playerManager.StartCounselorWrapper(playerList[i]);
            }
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class OverlayUI : MonoBehaviour
{
    #region Variables

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
    [HideInInspector]
    public PlayerManager playerManager;
    private bool startedGame = false;
    [HideInInspector]
    public GameManager gameManager;
    private Scene currScene;

    #endregion

    #region Unity

    private void Awake()
    {
        //start w/ interact txt off
        interactTxt.gameObject.SetActive(false);

        //start w/ minimap off bc driven by if perception in range
        minimap.SetActive(false);

        //start w/ health slider off
        healthSlider.gameObject.SetActive(false);

        //can do bc singleton
        gameManager = FindObjectOfType<GameManager>();

        //cache curr scene
        currScene = SceneManager.GetActiveScene();

        //if game scene
        if(currScene.name == "Game")
        {
            //inactivate vote overlay
            voteToggle.gameObject.SetActive(false);
        }
    }

    
    private void Update()
    {
        //if started game already
        if (startedGame || currScene.name == "Game")
        {
            //Debug.Log("Game already started");

            //dont allow voting
            return;
        }

        //if press V
        if (Input.GetKeyDown(key: KeyCode.V))
        {
            //invert toggle
            voteToggle.isOn = !voteToggle.isOn;

            //if online
            if (PhotonNetwork.IsConnected)
            {
                //if now voting to start
                if (voteToggle.isOn == true)
                {
                    //should incr votes over RPC so works for everyone
                    gameManager.photonView.RPC("RPC_ChangeVoteCount",
                        RpcTarget.AllBuffered,
                        gameManager.startVotes + 1);
                }
                //not voting to start
                else
                {
                    //should descr votes over RPC so works for everyone
                    gameManager.photonView.RPC("RPC_ChangeVoteCount",
                        RpcTarget.AllBuffered,
                        gameManager.startVotes - 1);
                }
            }
            //if local
            else
            {
                //if now voting to start
                if (voteToggle.isOn == true)
                {
                    //should incr votes over RPC so works for everyone
                    gameManager.RPC_ChangeVoteCount(gameManager.startVotes + 1);
                }
                //not voting to start
                else
                {
                    //should descr votes over RPC so works for everyone
                    gameManager.RPC_ChangeVoteCount(gameManager.startVotes - 1);
                }
            }
        }

        //if online
        if (PhotonNetwork.IsConnected)
        {
            //if all players voted to start
            if (gameManager.startVotes >= PhotonNetwork.CurrentRoom.PlayerCount)
            {
                //Start game for everyone
                //Debug.LogError("Should start game.");

                //if in game lobby
                if( SceneManager.GetActiveScene().name == "Game Lobby")
                {
                    //advance scene
                    playerManager.AdvanceScene();
                }
                //not in game lobby
                else
                {
                    //start game
                    StartGame();
                }
                
                startedGame = true;
            }
        }
        //if local
        else
        {
            //if voted to start
            if (gameManager.startVotes >= 1)
            {
                //Start game locally
                //Debug.LogError("Should start game.");

                //if in game lobby
                if (SceneManager.GetActiveScene().name == "Game Lobby")
                {
                    //load next scene
                    playerManager.AdvanceScene();
                }
                //if not in game lobby
                else
                {
                    //start the game in this scene
                    StartGame();
                }

                startedGame = true;
            }
        }
    }

    #endregion

    #region Update UI

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

    #endregion

    #region Start Game

    /// <summary>
    /// start game thru disabling vote toggle UI + moving players
    /// </summary>
    public void StartGame()
    {
        //disable voting UI
        voteToggle.gameObject.SetActive(false);

        startedGame = true;

        //change levels:
        //gameManager.ChangeLevels();

        //move players to game level
        //MovePlayers();
        playerManager.TeleportPlayers();
    }
    /*
    /// <summary>
    /// master client move players to start of actual game
    /// </summary>
    private void MovePlayers()
    {
        //if not connected to network 
        if(!PhotonNetwork.IsConnected)
        {
            //move local player
            playerManager.player.transform.position = playerManager.counselorStart.position;

            //dont move other players
            return;
        }
        //if not master client
        else if (!PhotonNetwork.IsMasterClient)
        {
            //dont move players
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
                playerManager.StartJasonWrapper(playerList[enemyIndex]);
                
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
    */
    #endregion
}

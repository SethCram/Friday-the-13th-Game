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
    [HideInInspector]
    public PlayerManager playerManager;
    private bool startedGame = false;

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

        //if not game lobby scene
        if( GameManager.Instance.currentScene != GameManager.CurrentScene.GAME_LOBBY)
        {
            //inactivate vote overlay
            voteToggle.gameObject.SetActive(false);
        }
    }

    
    private void Update()
    {
        //if started game already or not in game lobby
        if ( startedGame || 
            GameManager.Instance.currentScene != GameManager.CurrentScene.GAME_LOBBY)
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
                    GameManager.Instance.photonView.RPC("RPC_ChangeVoteCount",
                        RpcTarget.AllBuffered,
                        GameManager.Instance.startVotes + 1);
                }
                //not voting to start
                else
                {
                    //should descr votes over RPC so works for everyone
                    GameManager.Instance.photonView.RPC("RPC_ChangeVoteCount",
                        RpcTarget.AllBuffered,
                        GameManager.Instance.startVotes - 1);
                }
            }
            //if local
            else
            {
                //if now voting to start
                if (voteToggle.isOn == true)
                {
                    //should incr votes over RPC so works for everyone
                    GameManager.Instance.RPC_ChangeVoteCount(GameManager.Instance.startVotes + 1);
                }
                //not voting to start
                else
                {
                    //should descr votes over RPC so works for everyone
                    GameManager.Instance.RPC_ChangeVoteCount(GameManager.Instance.startVotes - 1);
                }
            }
        }

        //if online
        if (PhotonNetwork.IsConnected)
        {
            //if all players voted to start
            if (GameManager.Instance.startVotes >= PhotonNetwork.CurrentRoom.PlayerCount)
            {
                //Start game for everyone
                //Debug.LogError("Should start game.");

                //advance scene
                playerManager.AdvanceScene();
                
                startedGame = true;
            }
        }
        //if local
        else
        {
            //if voted to start
            if (GameManager.Instance.startVotes >= 1)
            {
                //Start game locally
                //Debug.LogError("Should start game.");
                    
                //load next scene
                playerManager.AdvanceScene();

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

}

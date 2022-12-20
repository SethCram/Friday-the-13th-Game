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

    //for hp slider
    public Slider healthSlider;
    public TMP_Text healthRatio;
    public GameObject hpFillAmt;

    //for stamina slider
    public Slider staminaSlider;
    public TMP_Text staminaRatio;
    public GameObject staminaFillAmt;

    //for starting game
    public Toggle voteToggle;
    [HideInInspector]
    public PlayerManager playerManager;
    private bool startedGame = false;

    private int prevStamina = 100;

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

        //start w/ stamina slider off
        staminaSlider.gameObject.SetActive(false);

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

            VoteToggleAndIncrStartVotes();
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

    /// <summary>
    /// Toggle the vote check box and incr # of votes to start the game.
    /// </summary>
    public void VoteToggleAndIncrStartVotes()
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
                //should incr vote anyways
                GameManager.Instance.RPC_ChangeVoteCount(GameManager.Instance.startVotes + 1);
            }
            //not voting to start
            else
            {
                //should descr vote anyways
                GameManager.Instance.RPC_ChangeVoteCount(GameManager.Instance.startVotes - 1);
            }
        }
    }

    //update health slider using HP changed event
    public void UpdateHealthSlider(int maxHP, int currHP)
    {
        //print("Update HP slider");

        UpdateAnySlider(healthSlider, healthRatio, hpFillAmt, currHP, maxHP);
    }

    //update stamina slider using Stamina changed event
    public void UpdateStaminaSlider(int maxStamina, int currStamina)
    {
        //if stamina being increased from 0
        if(prevStamina == 0 && currStamina > 0 )
        {
            //enable stamina fill amt
            staminaFillAmt.SetActive(true);
        }

        prevStamina = currStamina;

        //print("Update stamina slider");

        UpdateAnySlider(staminaSlider, staminaRatio, staminaFillAmt, currStamina, maxStamina);
    }

    private void UpdateAnySlider(Slider slider, TMP_Text ratioText, GameObject fillAmt, int currAmt, int maxAmt)
    {
        //if no amt left
        if( currAmt <= 0)
        {
            //delete fill amt
            fillAmt.SetActive(false);
        }
        //if reset
        else if( currAmt == maxAmt)
        {
            //show fill amt
            fillAmt.SetActive(true);
        }

        //update filled amt (float casts needed)
        slider.value = (float)currAmt / (float)maxAmt;

        //update ratio text
        ratioText.text = currAmt + "/" + maxAmt;
    }

    #endregion

}

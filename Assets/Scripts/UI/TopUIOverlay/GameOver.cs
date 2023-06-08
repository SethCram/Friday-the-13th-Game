using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public GameObject gameOverPanel;
    public TMP_Text titleText;
    public TMP_Text afterDeathActionBtnTxtObj;
    public GameObject afterDeathActionBtn;

    [HideInInspector]
    public Transform player; //fill in UI instantiator
    private PlayerManager playerManager;

    // Start is called before the first frame update
    void Start()
    {
        gameOverPanel.SetActive(false);

        //if player field not filled by UI_instantiator
        if( player != null)
        {
            playerManager = player.GetComponent<PlayerManager>();
        }

        //if in game scene:
        if (GameManager.Instance.currentScene == GameManager.CurrentScene.GAME)
        {
            //set btn to spectate
            afterDeathActionBtnTxtObj.text = "SPECTATE";
        }
        //if not in game scene:
        else
        {
            //set btn to respawn 
            afterDeathActionBtnTxtObj.text = "RESPAWN";
        }

    }

    /// <summary>
    /// show only menu btn thru disabling after death action btn
    /// </summary>
    public void ShowOnlyMainMenuBtn()
    {
        afterDeathActionBtn.SetActive(false);
    }

    public void UpdateTitleText( string newTitle )
    {
        titleText.text = newTitle;
    }

    //allow player to spectate thru btn
    public void Spectate()
    {
        //deactivate game over screen
        gameOverPanel.SetActive(false);

        //no longer dead
        playerManager.SetDead( false );

        //try to tele player
        playerManager.SetTeleportPlayer( true );
    }

    /// <summary>
    /// Call game manager's main menu btn method
    /// </summary>
    public void MainMenuButtonWrapper()
    {
        GameManager.Instance.MainMenuButton();
    }

}

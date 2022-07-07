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

    [HideInInspector]
    public Transform player; //fill in UI instantiator
    private PlayerManager playerManager;

    //public Vector3 spectatorSpawn = new Vector3(0, 500, 0);

    // Start is called before the first frame update
    void Start()
    {
        gameOverPanel.SetActive(false);

        //if player field not filled by UI_instantiator
        if( player != null)
        {
            playerManager = player.GetComponent<PlayerManager>();
        }

    }



    public void UpdateTitleText( string newTitle )
    {
        titleText.text = newTitle;
    }

    //allow player to spectate
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

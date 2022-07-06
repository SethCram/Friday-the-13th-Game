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

        playerManager = player.GetComponent<PlayerManager>();
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

    //reset player to main menu:
    public void ResetToMainMenu()
    {
        //use escape panel's disconnect + load
        //EscapePanel escapePanel = GameObject.FindObjectOfType(typeof(EscapePanel), true); //GameObject.FindObjectOfType(typeof(EscapePanel), true);

        StartCoroutine(DisconnectAndLoad());
    }

    //disconnect client and load main menu:
    private IEnumerator DisconnectAndLoad()
    {
        //load main menu if no longer connected:
        SceneManager.LoadScene(0);

        //disconnect client from server:
        PhotonNetwork.Disconnect();

        //while still connected to network:
        while (PhotonNetwork.IsConnected)
        {
            //dont yet load scene:
            yield return null;
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; //to load scenes
using Photon.Pun; //needed to access photon network

public class EscapePanel : MonoBehaviour
{
    //***init in inspector*****:
    public PausedUI pauseUI;

    //called by resume button:
    public void ResumeButton()
    {
        //tell to close escape menu:
        pauseUI.CloseEscapeMenu();
    }

    //called by main menu button:
    public void MainMenuButton()
    {
        //disconnect client and load main menu:
        StartCoroutine(DisconnectAndLoad());
    }

    //disconnect client and load main menu:
    public IEnumerator DisconnectAndLoad()
    {
        //disconnect client from server:
        PhotonNetwork.Disconnect();

        //while still connected to network:
        while(PhotonNetwork.IsConnected)
        {
            //dont yet load scene:
            yield return null;
        }

        //load main menu if no longer connected:
        SceneManager.LoadScene(0);
    }
}

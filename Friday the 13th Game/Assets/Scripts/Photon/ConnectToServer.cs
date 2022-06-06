using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; //gives access to photon tools (need for 'PhotonNetwork')
using UnityEngine.SceneManagement; //need to switch scenes
using UnityEngine.UI;
using TMPro;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    /// <summary>
    /// for loading
    /// </summary>
    public Slider slider;
    public TMP_Text progressTxt;

    private void Start()
    {


        //set nickname to master manager's:
        PhotonNetwork.NickName = MasterManager.gameSettings.Nickname;

        //lock all players to master manager version:
        PhotonNetwork.GameVersion = MasterManager.gameSettings.gameVersion;

        //connect to photon server using our settings: (what settings?)
        PhotonNetwork.ConnectUsingSettings();
    }

    //callback funct for w/ server ready for matchmaking so it joins them to the lobby:
    public override void OnConnectedToMaster() //called w/ client connects to server + ready for matchmaking
    {
        //? base.OnConnectedToMaster();

        //client joins lobby:
        PhotonNetwork.JoinLobby();

        //check nickname setting correctly:
        print(PhotonNetwork.LocalPlayer.NickName);
    }

    //callback funct for w/ client joins lobby we switch scene to 'Lobby' scene: (so scene objs r loaded once everytime a client joins)
    public override void OnJoinedLobby()
    {
        //? base.OnJoinedLobby();

        //load next scene in build settings (should be Lobby scene):
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        StartCoroutine(LoadLevelAsynch());
    }

    /// <summary>
    /// Asynchronously loads level and updates loading bar + text.
    /// </summary>
    /// <returns>Returns null to update every frame</returns>
    private IEnumerator LoadLevelAsynch()
    {
        //have multiplayer server asynchly load next scene in build settings (should be Lobby scene):
        PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().buildIndex + 1);

        //check if level loaded yet
        while (PhotonNetwork.LevelLoadingProgress < 1)
        {
            slider.value = PhotonNetwork.LevelLoadingProgress;

            //if loading is still zero
            if (PhotonNetwork.LevelLoadingProgress == 0)
            {
                slider.fillRect.gameObject.SetActive(false);
            }
            //loading is non zero
            else
            {
                slider.fillRect.gameObject.SetActive(true);
            }

            //set progress percentage
            int progressPercentage = (int)(PhotonNetwork.LevelLoadingProgress * 100f);
            progressTxt.text = progressPercentage.ToString() + "%";

            //wait a frame
            yield return null;
        }
    }
}

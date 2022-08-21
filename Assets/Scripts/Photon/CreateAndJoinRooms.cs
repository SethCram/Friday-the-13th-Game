using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; //allows us to connect to input fields and buttons
using Photon.Pun; //needed to join/create rooms + load game scene
using TMPro; //needed for text mesh pro input fields
using UnityEngine.SceneManagement;
using Photon.Realtime; //for room options

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    public TMP_InputField createInput;
    public TMP_Text errorCreate;
    public TMP_InputField joinInput;
    public TMP_Text errorJoin;

    //for loading 
    public GameObject loadingScreen;
    public GameObject roomSelectionScreen;
    public Slider slider;
    public TMP_Text progressTxt;

    public int maxPlayers = 4;

    private void Start()
    {
        //set screen visibility 

        roomSelectionScreen.SetActive(true);

        loadingScreen.SetActive(false);
    }

    #region Create Room

    //create room w/ corresponding input as room name w/ create button pressed:
    public void CreateRoom()
    {
        //output msg if empty room field
        if (string.IsNullOrEmpty(createInput.text))
        {
            AssignMsgText(errorCreate, "CreateRoom failed. A roomname is required.", error: true);

            //dont create room
            return;
        }

        //set room options:
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)maxPlayers; //0 = no limit

        //roomOptions.CleanupCacheOnLeave = false; //doesnt cleanup player or objs they have ownership over w/ leave
        //roomOptions.PublishUserId = true;

        //clear error msg w/ client msg
        AssignMsgText(errorCreate, "Creating room...", error: false);

        //make and join room:
        PhotonNetwork.CreateRoom(createInput.text, roomOptions);
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();

        Debug.Log("Room successfully created");
    }

    /// <summary>
    /// When create room fails, output return code in log + tell user it failed.
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);

        //output why cant create room:
        Debug.Log($"Room not created. Return code = {returnCode}. ");

        AssignMsgText(errorCreate, message, error: true);

    }

    #endregion

    #region Join Room

    //join room w/ corresponding input as room name w/ join button pressed:
    public void JoinRoom()
    {
        //output msg if empty room field
        if (string.IsNullOrEmpty(joinInput.text))
        {
            //output error msg
            AssignMsgText(errorJoin, "JoinRoom failed. A roomname is required.", error: true);

            //dont join room
            return;
        }

        //output client msg
        AssignMsgText(errorJoin, "Joining room...", error: false);

        //join room
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    /// <summary>
    /// When join room failed, tell user message and log return code.
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);

        Debug.Log($"On Join room failed. Return code = {returnCode}.");

        AssignMsgText(errorJoin, message, error: true);
    }

    //callback for w/ any client joins room then multiplayer game loaded:
    public override void OnJoinedRoom() //called both w/ client and other people join room
    {
        //isnt displayed
        Debug.Log("Game lobby scene loading.");

        //load level asynchly
        StartCoroutine(LoadLevelAsynch());
    }

    #endregion

    /// <summary>
    /// Assign given message to the msg text, logging color depends on if error or not.
    /// </summary>
    /// <param name="msgText"></param>
    /// <param name="message"></param>
    /// <param name="error"></param>
    public void AssignMsgText(TMP_Text msgText, string message, bool error = false)
    {
        msgText.text = message;

        //if error
        if (error == true)
        {
            //output error as red txt:
            Debug.Log($"<color=red>Error message: {message} </color>");
        }
        // not error
        else
        {
            //output client reg msg
            Debug.Log("Client message: " + message);
        }

    }

    private IEnumerator LoadLevelAsynch()
    {
        //deactivate room selection screen
        roomSelectionScreen.SetActive(false);

        //activate loading screen
        loadingScreen.SetActive(true);

        //have multiplayer server asynchly load next scene in build settings (should be Game scene):
        PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().buildIndex + 1);

        //check if level loaded yet
        while (PhotonNetwork.LevelLoadingProgress < 1)
        {
            //log progress (breaks it)
            //Debug.Log(PhotonNetwork.LevelLoadingProgress);

            //set to 100% even if only 90% thru clamping
            float progress = Mathf.Clamp01(PhotonNetwork.LevelLoadingProgress / 0.9f);
            slider.value = progress;

            //slider.value = PhotonNetwork.LevelLoadingProgress;


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
            int progressPercentage = (int)(progress * 100f);
            progressTxt.text = progressPercentage.ToString() + "%";

            //wait a frame
            yield return null;
            //yield return new WaitForEndOfFrame();
        }
    }

    /// <summary>
    /// kick player back to main menu if disconnected from photon
    /// </summary>
    /// <param name="cause"></param>
    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);

        //create disconnect msg
        string disconnectMsg = "Disconnected for reason: " + cause.ToString();

        //print out why disconnected:
        AssignMsgText(msgText: errorJoin, message: disconnectMsg);

        //load menu menu
        SceneManager.LoadScene(0);
    }
}

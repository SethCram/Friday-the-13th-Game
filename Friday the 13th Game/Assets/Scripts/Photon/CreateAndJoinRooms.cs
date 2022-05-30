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

    //public int maxPlayers = 4;

    //create room w/ corresponding input as room name w/ create button pressed:
    public void CreateRoom()
    {
        //output msg if empty room field
        if ( string.IsNullOrEmpty( createInput.text) )
        {
            AssignErrorText(errorCreate, "CreateRoom failed. A roomname is required.");

            //dont create room
            return;
        }

        //set room options:
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 0; //4; 0 = no limit

        //roomOptions.CleanupCacheOnLeave = false; //doesnt cleanup player or objs they have ownership over w/ leave
        //roomOptions.PublishUserId = true;

        //clear error msg
        AssignErrorText(errorCreate, "Creating room...");

        //make and join room:
        PhotonNetwork.CreateRoom(createInput.text, roomOptions);
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();

        Debug.Log("Room successfully created");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);

        //output why cant create room:
        Debug.LogWarning("Room not created bc: " + message);

        AssignErrorText(errorCreate, message);

    }

    //join room w/ corresponding input as room name w/ join button pressed:
    public void JoinRoom()
    {
        //output msg if empty room field
        if (string.IsNullOrEmpty(joinInput.text))
        {
            //output error msg
            AssignErrorText(errorJoin, "JoinRoom failed. A roomname is required.");

            //dont join room
            return;
        }

        //output client msg
        AssignErrorText(errorJoin, "Joining room...", false);

        //join room
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);

        AssignErrorText(errorJoin, message);
    }

    //assign error text
    public void AssignErrorText( TMP_Text errorText, string message, bool error = true)
    {
        errorText.text = message;

        //if error
        if( error == true )
        {
            //output error:
            Debug.LogError("Error message: " + message);
        }
        // not error
        else
        {
            //output client reg msg
            Debug.Log("Client message: " + message);
        }

    }

    //callback for w/ any client joins room then multiplayer game loaded:
    public override void OnJoinedRoom() //called both w/ client and other people join room
    {
        //have multiplayer server load next scene in build settings (should be Game scene):
        PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().buildIndex + 1);
    }


}

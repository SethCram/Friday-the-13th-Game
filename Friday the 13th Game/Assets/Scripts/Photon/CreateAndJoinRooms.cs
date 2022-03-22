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
    public TMP_InputField joinInput;

    //public int maxPlayers = 2;

    //create room w/ corresponding input as room name w/ create button pressed:
    public void CreateRoom()
    {
        //set room options:
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;

        //roomOptions.CleanupCacheOnLeave = false; //doesnt cleanup player or objs they have ownership over w/ leave
        //roomOptions.PublishUserId = true;

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
    }

    //join room w/ corresponding input as room name w/ join button pressed:
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);

        //output why cant join room:
        Debug.LogWarning("Room not joined bc: " + message);
    }

    //callback for w/ any client joins room then multiplayer game loaded:
    public override void OnJoinedRoom() //called both w/ client and other people join room
    {
        //have multiplayer server load next scene in build settings (should be Game scene):
        PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().buildIndex + 1);
    }
}

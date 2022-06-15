using TMPro;
using Photon.Realtime;
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class RoomListing : MonoBehaviourPunCallbacks
{
    //to setup room txt:
    public TMP_Text nameText;
    public TMP_Text playersText;

    //to store curr room info:
    public RoomInfo _room_Info;

    public CreateAndJoinRooms createJoinRooms;

    //called by 'RoomListingsMenu' w/ Room List Updated:
    public void SetRoomInfo(RoomInfo roomInfo)
    {
        //set room name:
        nameText.text = roomInfo.Name;

        //store curr room info for this listing:
        _room_Info = roomInfo;
    }

    /// <summary>
    /// Update player count text on listing entry.
    /// </summary>
    public void UpdatePlayerCount( int currPlayers = 0 )
    {
        //if passed in param not set
        if( currPlayers == 0)
        {
            //set curr/max room player amt: 
            playersText.text = _room_Info.PlayerCount.ToString() + "/" + _room_Info.MaxPlayers.ToString();
        }
        //if passed in param set, use it
        else
        {
            //set curr/max room player amt: 
            playersText.text = currPlayers.ToString() + "/" + _room_Info.MaxPlayers.ToString();
        }

    }

    //called w/ listing clicked:
    public void JoinRoom()
    {
        //call createJoinRooms join room call
        //createJoinRooms.JoinRoom();

        //clear error msg
        createJoinRooms.AssignErrorText(createJoinRooms.errorJoin, "Joining room...", false);

        //join room:
        PhotonNetwork.JoinRoom(nameText.text);

        // 'OnJoinedRoom()' callback should call if successful, and will load game scene
    }
    
}

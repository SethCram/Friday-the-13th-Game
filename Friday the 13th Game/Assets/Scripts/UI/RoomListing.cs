using TMPro;
using Photon.Realtime;
using Photon.Pun;

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

        //set max room player amt:
        playersText.text = roomInfo.MaxPlayers.ToString();
        
        //clear player count txt
        //playersText.text = "";

        //store curr room info for this listing:
        _room_Info = roomInfo;
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

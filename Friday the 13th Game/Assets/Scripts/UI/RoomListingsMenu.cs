using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomListingsMenu : MonoBehaviourPunCallbacks
{
    //to create new room listings:
    public Transform roomListingsParent;
    public RoomListing roomListingPrefab;
    public CreateAndJoinRooms createJoinRoom;

    //to store rooms added to room listings (always init lists):
    private List<RoomListing> listings = new List<RoomListing>();
    private List<int> previousPlayers = new List<int>(); //attached to each room listing to make sure dont create another room w/ join a room

    public override void OnJoinedLobby()
    {
        //instantiate all currently open rooms
        //PhotonNetwork.room
    }

    // w/ ever a visible room created/destroyed, this called:
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);

        int j = 0; //cnting var for previous players

        //loop thru every room info in the list:
        foreach (RoomInfo roomInfo in roomList)
        {

            //if removed from room list:
            if(roomInfo.RemovedFromList)
            {
                    //int roomIndex = listings.FindIndex(x => x._room_Info.Name == roomInfo.Name); //does the same as below for loop in less code
                    //listings.Remove(listings[roomIndex]);

                //loop thru all room listings:
                for (int i = 0; i < listings.Count; i++)
                {
                    RoomListing currListing = listings[i];

                    //if a listings name is the one we're removing:
                    if(currListing._room_Info.Name == roomInfo.Name)
                    {
                        //destroy listing:
                        Destroy(currListing.gameObject);

                        //remove that listing from list:
                        listings.Remove(currListing);

                        //remove previous player entry for comparison:
                        previousPlayers.Remove(previousPlayers[j]);

                        //break from for loop:
                        break;
                    }
                }
            }
            //added to room list:
            else
            {
                //if previous players havent been added yet bc no rooms exist:
                if(previousPlayers.Count == 0)
                {
                    //create room listing w/ given info:
                    CreateRoomListing(roomInfo);
                }
                //if room has more players than previously:
                else if(roomInfo.PlayerCount > previousPlayers[j])
                {
                    //dont create a new room bc a player joined a room

                    //increase previous player cnt:
                    previousPlayers[j]++;

                    Debug.LogError("Didnt create a new room bc joined another room.");
                }
                else
                {
                    //create room listing w/ given info:
                    CreateRoomListing(roomInfo);
                }
            }

            j++;
        }
    }

    //create room listing w/ given info:
    private void CreateRoomListing(RoomInfo roomInfo)
    {
        //create a new room listing:
        RoomListing roomListing = Instantiate(roomListingPrefab, roomListingsParent);

        //set room listing's room info:
        roomListing.SetRoomInfo(roomInfo);

        roomListing.createJoinRooms = createJoinRoom;

        //add listing to list: (need to add over buffered RPC?) (could have players join empty room if needed to run RPCs?)
        listings.Add(roomListing);

        //init previous players list as starting each entry w/ 1:
        previousPlayers.Add(1);
    }
}

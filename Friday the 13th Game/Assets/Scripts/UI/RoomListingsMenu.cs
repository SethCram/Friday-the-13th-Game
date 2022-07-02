using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomListingsMenu : MonoBehaviourPunCallbacks
{
    #region vars

    //to create new room listings:
    public Transform roomListingsParent;
    public RoomListing roomListingPrefab;
    public CreateAndJoinRooms createJoinRoom;

    //to store rooms added to room listings (always init lists):
    //private List<RoomListing> listings = new List<RoomListing>();
    //private List<int> previousPlayers = new List<int>(); //attached to each room listing to make sure dont create another room w/ join a room

    //Room listing data struct to replace the need for 2 diff lists
    private struct RoomListingData
    {
        public RoomListing listing;
        public int expectedPlayerCount;
    }
    private List<RoomListingData> roomListingDataList = new List<RoomListingData>(); 

    private int playerCurrCnt;
    private bool enteredRoomUpdateBefore = false;

    #endregion

    #region Photon Methods

    /// <summary>
    /// placeholder funct for future use
    /// </summary>
    public override void OnJoinedLobby()
    {
        //placeholder
    }

    // w/ ever a visible room created/destroyed, this called:
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
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
                for (int i = 0; i < roomListingDataList.Count; i++)
                {
                    //cache vals for use
                    RoomListingData currListingData = roomListingDataList[i];
                    RoomListing currListing = currListingData.listing;

                    //if a listings name is the one we're removing:
                    if (currListing._room_Info.Name == roomInfo.Name)
                    {
                        //destroy listing:
                        Destroy(currListing.gameObject);

                        //remove that data from list:
                        roomListingDataList.Remove(roomListingDataList[i]);

                        //break from for loop:
                        break;
                    }
                }
            }
            //added to room list bc someone created room or someone joined room or left room:
            else
            {

                //if data hasnt been added yet bc no rooms exist:
                if(roomListingDataList.Count == 0)
                {
                    //create room listing w/ given info:
                    CreateRoomListing(roomInfo);
                }
                //if room has more players than accounted for:
                else if(roomInfo.PlayerCount > roomListingDataList[j].expectedPlayerCount)
                {
                    //dont create a new room bc a player joined a room

                    //increase previous player cnt:
                    RoomListingData roomListingData = roomListingDataList[j];
                    roomListingData.expectedPlayerCount++;
                    roomListingDataList[j] = roomListingData;

                    Debug.LogError("Didnt create a new room bc player joined another room.");
                }
                //if room has less players than previously:
                else if(roomInfo.PlayerCount < roomListingDataList[j].expectedPlayerCount)
                {
                    //decr previous player cnt:
                    RoomListingData roomListingData = roomListingDataList[j];
                    roomListingData.expectedPlayerCount--;
                    roomListingDataList[j] = roomListingData;

                    Debug.LogError("Didnt create a new room bc player left another room.");
                }
                /*
                else
                {
                    //create room listing w/ given info:
                    CreateRoomListing(roomInfo);
                }
                */
            }

            j++;
        }

        //walk thru listings
        for (int i = 0; i < roomListingDataList.Count; i++)
        {
            //cache curr player cnt
            playerCurrCnt = roomListingDataList[i].listing._room_Info.PlayerCount;

            //if room + stored player counts diff and initing rooms for 1st time
            if (!enteredRoomUpdateBefore && playerCurrCnt != roomListingDataList[i].expectedPlayerCount)
            {
                //set to room player count (bc always right at lobby start)
                RoomListingData roomListingData = roomListingDataList[i];
                roomListingData.expectedPlayerCount = playerCurrCnt;
                roomListingDataList[i] = roomListingData;
            }

            //make sure their player count up to date in UI (curr players not dynamically updated)
            roomListingDataList[i].listing.UpdatePlayerCount(roomListingDataList[i].expectedPlayerCount);

            //debug: Debug.LogError("\n list's player count = " + previousPlayers[i] + ", curr player count = " + playerCurrCnt);

        }
        
        //if 1st time updating rooms:
        if (!enteredRoomUpdateBefore)
        {
            //don't reset previous players cnt again 
            enteredRoomUpdateBefore = true;
        }
    }

    //create room listing w/ given info:
    private void CreateRoomListing(RoomInfo roomInfo)
    {
        //create a new room listing + struct:
        RoomListing roomListing = Instantiate(roomListingPrefab, roomListingsParent);
        RoomListingData roomListingData = new RoomListingData();

        //fill out struct data
        roomListingData.listing = roomListing;
        roomListingData.expectedPlayerCount = 1;

        //set room listing's room info:
        roomListing.SetRoomInfo(roomInfo);

        roomListing.createJoinRooms = createJoinRoom;

        //add data listing to list: 
        roomListingDataList.Add(roomListingData);
    }

    #endregion
}

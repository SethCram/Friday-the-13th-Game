using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayers : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public Transform jasonSpawn;
    public Transform counselorSpawn;
    public Transform customLocalSpawn;

    // Start is called before the first frame update
    void Start()
    {
        NetworkCloseRoom();

        SpawnPlayersAtStart();
    }

    /// <summary>
    /// Spawn players at start of game
    /// </summary>
    public void SpawnPlayersAtStart()
    {
        //on network
        if(PhotonNetwork.IsConnected)
        {
            //if master of room
            if (PhotonNetwork.IsMasterClient)
            {
                //print out player list
                Debug.Log("player count =" + PhotonNetwork.PlayerList.Length);

                int index = 0;

                //walk thru players
                foreach (Player pl in PhotonNetwork.PlayerList)
                {
                    //if 1st player
                    if (index == 0)
                    {
                        //start player as Jason
                        photonView.RPC("SpawnPlayer", pl, index, true);
                    }
                    //not 1st player
                    else
                    {
                        //start player as counselor
                        photonView.RPC("SpawnPlayer", pl, index, false);
                    }
                    index++;
                }
            }
        }
        //local game
        else
        {
            //create a local player as counselor:
            Instantiate(playerPrefab, 
                customLocalSpawn.position, 
                playerPrefab.transform.rotation);

        }
    }

    /// <summary>
    /// Spawn player based on whether jason or not
    /// </summary>
    /// <param name="index"></param>
    /// <param name="jason"></param>
    [PunRPC]
    public void SpawnPlayer(int index, bool jason = false)
    {
        //create an obj for every new player joining, when they load in

        //if jason
        if (jason)
        {
            GameObject spawnedPlayer = PhotonNetwork.Instantiate(playerPrefab.name,
                jasonSpawn.position,
                playerPrefab.transform.rotation);
            spawnedPlayer.tag = "Enemy";
        }
        //if not jason
        else
        {
            PhotonNetwork.Instantiate(playerPrefab.name,
                counselorSpawn.position + Vector3.right * index,
                playerPrefab.transform.rotation);
        }
    }

    /// <summary>
    /// close off room to network
    /// </summary>
    public void NetworkCloseRoom()
    {
        //if connected + master client
        if(PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            //close room
            PhotonNetwork.CurrentRoom.IsOpen = false;

            //disable visibility in lobby
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }
    }
}

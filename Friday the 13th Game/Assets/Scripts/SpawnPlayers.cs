using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject playerPrefab;
    private Vector3 spawnPoint;
    public PhotonView photonView;
    public Transform jasonSpawn;
    public Transform counselorSpawn;

    // Start is called before the first frame update
    void Start()
    {
        SpawnPlayersAtStart();
    }

    public void SpawnPlayersAtStart()
    {
        if(PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                int index = 0;

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
        else
        {
            //create a local scimitar:
            Instantiate(playerPrefab, counselorSpawn.position, playerPrefab.transform.rotation);

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
        //create a obj for every new player joining, when they load in:
        GameObject spawnedPlayer = PhotonNetwork.Instantiate(playerPrefab.name, 
            new Vector3(0, 0, 0), 
            playerPrefab.transform.rotation);

        //if jason
        if (jason)
        {
            spawnedPlayer.transform.position = jasonSpawn.position;
            spawnedPlayer.tag = "Enemy";
        }
        //if not jason
        else
        {
            spawnedPlayer.transform.position = counselorSpawn.position + Vector3.right * index;
        }
    }
}

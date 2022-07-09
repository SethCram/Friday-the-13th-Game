using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayers : MonoBehaviourPunCallbacks
{
    #region vars

    public const int MAX_FRAMES_WAITED = 100000;

    public GameObject playerPrefab;
    public Transform jasonSpawn;
    public Transform counselorSpawn;
    public Transform customLocalSpawn;

    #endregion

    // Start is called before the first frame update
    void Start()
    {

        //if on network
        if (PhotonNetwork.IsConnected)
        {
            //debug: Debug.LogError("players game ready = " + (int)(GameManager.Instance.playersGameReady + 1));

            //incr # of players game ready on network
            GameManager.Instance.photonView.RPC("RPC_IncrPlayersGameReady", RpcTarget.AllBuffered);

            //if master client
            if( PhotonNetwork.IsMasterClient)
            {
                //debug: Debug.LogAssertion("isMasterClient = " + PhotonNetwork.IsMasterClient);

                //check if all players are ready
                StartCoroutine(CheckAllPlayersReady());
            }
        }
        //if local 
        else
        {
            //spawn local player at start
            SpawnPlayersAtStart();
        }
        
        NetworkCloseRoom();

        //SpawnPlayersAtStart();
    }

    #region Spawning Methods

    /// <summary>
    /// Spawn players at start of game
    /// </summary>
    public void SpawnPlayersAtStart()
    {
        //on network and master of room
        if(PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            //print out player list
            //Debug.LogError("player count = " + PhotonNetwork.PlayerList.Length);

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
    /// Checks whether all players loaded into the scene yet.
    /// </summary>
    /// <returns></returns>
    public IEnumerator CheckAllPlayersReady()
    {
        int framesWaited = 0;

        // if not all players loaded into room
        while (PhotonNetwork.CurrentRoom.PlayerCount > GameManager.Instance.playersGameReady)
        {
            //wait a frame then check again
            yield return null;

            framesWaited++;

            //if more than max num of frames waited
            if( framesWaited > MAX_FRAMES_WAITED )
            {
                Debug.LogError("More than " + MAX_FRAMES_WAITED + " frames waited to spawn players, so not spawned all at the same time.");
                Debug.LogError("Current player count = " + PhotonNetwork.CurrentRoom.PlayerCount + " players loaded into the game = " + GameManager.Instance.playersGameReady);
                break;
            } 
        }

        Debug.LogAssertion("number of frames waited to spawn players = " + framesWaited);

        SpawnPlayersAtStart();
    }

    /// <summary>
    /// Spawn player based on whether jason or not
    /// </summary>
    /// <param name="index"></param>
    /// <param name="jason"></param>
    [PunRPC]
    private void SpawnPlayer(int index, bool jason = false)
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

    #endregion

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

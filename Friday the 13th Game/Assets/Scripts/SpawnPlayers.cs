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
    public bool spawnLocalAsJasonTagged = false;

    public bool spawnedPlayer { private set; get; }

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
            //choose jason player randomly
            int jasonIndex = Random.Range(0, PhotonNetwork.PlayerList.Length - 1);

            Debug.LogAssertion($"Jason index gen'd by master client using random class = {jasonIndex} ");

            int index = 0;

            //walk thru players
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                //if index matches jason's index
                if (index == jasonIndex)
                {
                    //start player as Jason
                    photonView.RPC("SpawnPlayer", player, index, true);
                }
                //not 1st player
                else
                {
                    //start player as counselor
                    photonView.RPC("SpawnPlayer", player, index, false);
                }
                index++;
            }
        }
        //local game
        else
        {
            //create a local player as counselor:
            GameObject localPlayer =  Instantiate(playerPrefab, 
                                        customLocalSpawn.position, 
                                        playerPrefab.transform.rotation);

            if (spawnLocalAsJasonTagged)
            {
                localPlayer.tag = "Enemy";
            }

            spawnedPlayer = true;
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

        //make new custom props inst for player
        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();

        //add lost + won game keys
        customProperties.Add(key: (object)GameManager.LOST_GAME_STR, value: GameManager.CUSTOM_PROP_FALSE);
        customProperties.Add(key: (object)GameManager.WON_GAME_STR, value: GameManager.CUSTOM_PROP_FALSE);

        //if jason
        if (jason)
        {
            GameObject spawnedPlayer = PhotonNetwork.Instantiate(playerPrefab.name,
                jasonSpawn.position,
                playerPrefab.transform.rotation);

            //set tag on player using player manager funct
            spawnedPlayer.GetPhotonView().RPC("SetTag", RpcTarget.AllBuffered, "Enemy");

            //set jason 
            customProperties.Add(key: (object)GameManager.IS_JASON_STR, value: GameManager.CUSTOM_PROP_TRUE);
            customProperties.Add(key: (object)GameManager.IS_COUNSELOR_STR, value: GameManager.CUSTOM_PROP_FALSE);
        }
        //if not jason
        else
        {
            PhotonNetwork.Instantiate(playerPrefab.name,
                counselorSpawn.position + Vector3.right * index,
                playerPrefab.transform.rotation);

            //give counselor
            customProperties.Add(key: (object)GameManager.IS_JASON_STR, value: GameManager.CUSTOM_PROP_FALSE);
            customProperties.Add(key: (object)GameManager.IS_COUNSELOR_STR, value: GameManager.CUSTOM_PROP_TRUE);
        }

        //cement player custom props
        PhotonNetwork.SetPlayerCustomProperties(customProperties);

        spawnedPlayer = true;
    }

    #endregion

    /// <summary>
    /// close off room to network
    /// </summary>
    private void NetworkCloseRoom()
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

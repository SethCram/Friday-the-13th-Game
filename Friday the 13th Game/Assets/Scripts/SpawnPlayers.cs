﻿using Photon.Pun;
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

            //if master client
            if( PhotonNetwork.IsMasterClient)
            {
                //debug: Debug.LogAssertion("isMasterClient = " + PhotonNetwork.IsMasterClient);

                //assign jason + counselor custom props
                AssignCustomProps();

                //check if all players are ready
                StartCoroutine(CheckAllPlayersReady(PhotonNetwork.CurrentRoom.PlayerCount));
            }
        }
        //if local 
        else
        {
            //check if all players are ready
            StartCoroutine(CheckAllPlayersReady(expectedPlayerCount: 1));
        }

    }

    /// <summary>
    /// If networked + master client, assign all the players custom props.
    /// </summary>
    public void AssignCustomProps()
    {
        //on network and master of room
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            //choose jason player randomly
            int jasonIndex = Random.Range(0, PhotonNetwork.PlayerList.Length - 1);

            Debug.LogAssertion($"Jason index gen'd by master client using random class = {jasonIndex} ");

            int index = 0;

            //walk thru players
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                //make new custom props inst for player
                ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();

                //add lost + won game keys
                customProperties.Add(key: (object)GameManager.LOST_GAME_STR, value: GameManager.CUSTOM_PROP_FALSE);
                customProperties.Add(key: (object)GameManager.WON_GAME_STR, value: GameManager.CUSTOM_PROP_FALSE);

                //if jason index
                if (index == jasonIndex)
                {
                    //set as jason custom props
                    customProperties.Add(key: (object)GameManager.IS_JASON_STR, value: GameManager.CUSTOM_PROP_TRUE);
                    customProperties.Add(key: (object)GameManager.IS_COUNSELOR_STR, value: GameManager.CUSTOM_PROP_FALSE);
                }
                //if not jason index
                else
                {
                    //set as counselor custom props
                    customProperties.Add(key: (object)GameManager.IS_JASON_STR, value: GameManager.CUSTOM_PROP_FALSE);
                    customProperties.Add(key: (object)GameManager.IS_COUNSELOR_STR, value: GameManager.CUSTOM_PROP_TRUE);
                }

                //cement player custom props
                PhotonNetwork.SetPlayerCustomProperties(customProperties);

                index++;
            }

            GameManager.Instance.customPropsSet = true;
        }
    }

    /// <summary>
    /// Checks whether all players loaded into the scene yet.
    /// </summary>
    /// <returns></returns>
    public IEnumerator CheckAllPlayersReady(int expectedPlayerCount)
    {
        int framesWaited = 0;

        // if not all players loaded into room
        while (expectedPlayerCount > GameManager.Instance.playersSpawnReady)
        {
            //wait a frame then check again
            yield return null;

            framesWaited++;

            /*
            //if more than max num of frames waited
            if (framesWaited > MAX_FRAMES_WAITED)
            {
                Debug.LogError("More than " + MAX_FRAMES_WAITED + " frames waited to spawn players, so not spawned all at the same time.");
                Debug.LogError("Expected player count = " + expectedPlayerCount + ", players spawn ready = " + GameManager.Instance.playersSpawnReady);
                break;
            }
            */
        }

        Debug.LogAssertion("number of frames waited to spawn players = " + framesWaited);

        //deactivate game intro panel
        GameManager.Instance.gameIntro.gameIntroPanel.SetActive(false);

        //spawn players
        SpawnPlayersAtStart();
    }

    #region Spawning Methods

    /// <summary>
    /// Spawns players at the start of the game
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
                //if player is jason
                if (GameManager.Instance.PlayerIsJason(player))
                {
                    //start player as Jason
                    photonView.RPC("SpawnPlayer", player, index, true);
                }
                //if player is counselor
                else if(GameManager.Instance.PlayerIsCounselor(player))
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

            //if local player is jason
            if (GameManager.Instance.localPlayerIsJason)
            {
                //tag as jason
                localPlayer.tag = GameManager.JASON_TAG;
            }
            //if local player isnt jason
            else
            {
                //tag as counselor
                localPlayer.tag = GameManager.COUNSELOR_TAG;
            }

            GameManager.Instance.localPlayerSpawned = true;
        }
    }

    /// <summary>
    /// Spawn player based on whether jason or not and set their tags.
    /// </summary>
    /// <param name="index">Index of this player.</param>
    /// <param name="jason">Whether this player is jason.</param>
    [PunRPC]
    private void SpawnPlayer(int index, bool jason = false)
    {
        GameObject spawnedPlayer;

        //if jason
        if (jason)
        {
            spawnedPlayer = PhotonNetwork.Instantiate(playerPrefab.name,
                jasonSpawn.position,
                playerPrefab.transform.rotation);

            //set tag on player using player manager funct
            spawnedPlayer.GetPhotonView().RPC("SetTag", RpcTarget.AllBuffered, GameManager.JASON_TAG);
        }
        //if not jason
        else
        {
            spawnedPlayer = PhotonNetwork.Instantiate(playerPrefab.name,
                counselorSpawn.position + Vector3.right * index,
                playerPrefab.transform.rotation);

            //set tag on player using player manager funct
            spawnedPlayer.GetPhotonView().RPC("SetTag", RpcTarget.AllBuffered, GameManager.COUNSELOR_TAG);
        }

        GameManager.Instance.localPlayerSpawned = true;
    }

    #endregion
}

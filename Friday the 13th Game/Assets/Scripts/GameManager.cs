using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class GameManager : MonoBehaviour
{
    [HideInInspector]
    public int startVotes { private set; get; } = 0; //needa incr/decr over RPC

    public PhotonView photonView;

    public Transform jasonStart;
    public Transform counselorStart;
    public GameObject gameLevel;
    public GameObject lobbyLevel;

    /// <summary>
    /// RPC to change start vote count.
    /// </summary>
    /// <param name="newVoteCount"></param>
    [PunRPC]
    public void RPC_ChangeVoteCount(int newVoteCount)
    {
        startVotes = newVoteCount;
    }    

    /// <summary>
    /// deactivate lobby + activate game approp levels
    /// </summary>
    public void ChangeLevels()
    {
        gameLevel.SetActive(true);

        lobbyLevel.SetActive(false);
    }
}

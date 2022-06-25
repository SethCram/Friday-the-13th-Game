using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPun 
{
    [HideInInspector]
    public int startVotes { private set; get; } = 0; //needa incr/decr over RPC

    /// <summary>
    /// RPC to change start vote count.
    /// </summary>
    /// <param name="newVoteCount"></param>
    [PunRPC]
    public void RPC_ChangeVoteCount(int newVoteCount)
    {
        startVotes = newVoteCount;
    }    
}

using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// game manager for keeping track of game data
/// NOT A SINGLETON
/// </summary>
public class GameManager : MonoBehaviourPun 
{
    [HideInInspector]
    public int startVotes { private set; get; } = 0; //needa incr/decr over RPC

    [HideInInspector]
    public int deadCounselors { private set; get; } = 0;

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
    /// RPC to change # of dead counselors
    /// </summary>
    /// <param name="newDeadCounselorsCount"></param>
    [PunRPC]
    public void RPC_ChangeCounselorsDead(int newDeadCounselorsCount)
    {
        deadCounselors = newDeadCounselorsCount;

    }

}

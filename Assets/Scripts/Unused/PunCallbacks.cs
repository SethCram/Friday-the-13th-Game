using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime; //for disconnection
using UnityEngine.SceneManagement;

public class PunCallbacks : MonoBehaviourPunCallbacks
{
    public PlayerManager playerManager;

    private void Start()
    {
        // w/ game loads, fill player manager field:
        playerManager = FindObjectOfType<PlayerManager>();
    }

    //callback funct for w/ client disconnected:
    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);

        //print out why disconnected:
        Debug.LogWarning("Disconnected for reason: " + cause.ToString());

        //Debug.LogError("Destroying gameobj attached to photon view ID: " + playerManager.gameObject.GetPhotonView().ViewID);

        //destroy disconnected player for everybody:
        //PhotonNetwork.Destroy(playerManager.gameObject);

        //try putting it in a coroutine to give it time to execute:
        //StartCoroutine(sendOutGoingCommands());
    }

    /*
    private IEnumerator sendOutGoingCommands()
    {
        //doesnt work bc already disconnected:
        PhotonNetwork.SendAllOutgoingCommands();

        yield return new WaitForSeconds(2);
    }
    */
    
    /*
    //callback for w/ another player leaves:
    public override void OnPlayerLeftRoom(Player otherPlayer) //passes in player disconnected
    {
        base.OnPlayerLeftRoom(otherPlayer);

        Debug.LogError(otherPlayer.NickName + " left the game and their user ID is: " + otherPlayer.UserId);

        Debug.Log("Number of players in game: " + PhotonNetwork.CountOfPlayersInRooms);

        //decrement player count w/ keeping track of them locally
    }
    */
}

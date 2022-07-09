using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// game manager for keeping track of game data
/// NOT A PERSISTENT DATA SINGLETON
/// </summary>
public class GameManager : MonoBehaviourPun 
{
    [HideInInspector]
    public int startVotes { private set; get; } = 0; //needa incr/decr over RPC

    [HideInInspector]
    public int deadCounselors { private set; get; } = 0;

    public int playersGameReady { private set; get; } = 0;

    #region Singleton

    private static GameManager instance = null;

    /*
     * Summary: Singleton definition of GameManager instance. 
     *          //Persistent across scenes and used by other classes.
     *          Created when needed.
     * 
     */
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                //find instance in scene
                instance = FindObjectOfType<GameManager>();

                //if no instance in scene
                if (instance == null)
                {
                    //create new manager in scene:
                    GameObject mnger = new GameObject();
                    mnger.name = "GameManager";
                    mnger.AddComponent<PhotonView>();
                    mnger.tag = "GameManager";
                    instance = mnger.AddComponent<GameManager>();

                    Debug.LogWarning("new game manager created");

                    //make sure not destroyed w/ change scenes
                    //DontDestroyOnLoad(mnger);
                }
            }
            return instance;
        }
    }

    #endregion Singleton

    /*
     * Summary: Makes sure there's only one instance of Singleton GameManager in scenes.
     *          Only checks if game manager script already attached to obj in scene.
     * 
     */
    void Awake()
    {
        //instance not here
        if (instance == null)
        {
            //set instance to this script (triggers above Instance method to create/destroy)
            instance = this;
            //DontDestroyOnLoad(this.gameObject);
        }
        //instance here
        else
        {
            //destroy it
            Destroy(gameObject);
        }
    }

    #region RPC's

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

    /// <summary>
    /// RPC to incr number of players loaded into game scene
    /// </summary>
    [PunRPC]
    public void RPC_IncrPlayersGameReady()
    {
        playersGameReady++;

        //debug: Debug.LogAssertion("players game ready = " + playersGameReady.ToString());
    }

    #endregion RPC's

    //called by main menu button:
    public void MainMenuButton()
    {
        //disconnect client and load main menu:
        StartCoroutine(DisconnectAndLoad());
    }

    //disconnect client and load main menu:
    private IEnumerator DisconnectAndLoad()
    {
        //disconnect client from server:
        PhotonNetwork.Disconnect();

        //while still connected to network:
        while (PhotonNetwork.IsConnected)
        {
            //dont yet load scene:
            yield return null;
        }

        //load main menu if no longer connected:
        SceneManager.LoadScene(0);
    }

}

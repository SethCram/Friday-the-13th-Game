using Photon.Pun;
using Photon.Realtime;
using System;
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
    #region Vars

    [HideInInspector]
    public int startVotes { private set; get; } = 0; //needa incr/decr over RPC

    [HideInInspector]
    public int deadCounselors { private set; get; } = 0;

    [HideInInspector]
    public int playersGameReady { private set; get; } = 0;

    [HideInInspector]
    public GameObject jasonPlayer;
    [HideInInspector]
    public GameObject ourPlayer;
    [HideInInspector]
    public PlayerManager playerManager;

    private bool jasonLeft = false;

    private string currSceneName;

    //state mach:
    public enum State { MENU, INIT, PLAY, LOADLEVEL, GAMEOVER, PAUSE, SPECTATE};
    private State _state;
    private State unpauseState;
    private bool _isSwitchingState;

    //scene tracking vars
    public enum CurrentScene { LOBBY, GAME_LOBBY, GAME };
    public CurrentScene currentScene { private set; get; }

    //to keep track of what panel active: (unneeded?)
    private GameObject currPanel;
    public GameObject panelMenu;
    public GameObject panelPlay;
    public GameObject panelLevelCompleted;
    public GameObject panelGameOver;
    public GameObject panelPause;
    public GameObject panelHelp;

    //music vars
    private bool musicShouldPlay = true;
    private AudioSource audioSrc;

    //game success or failure 
    private bool lostGame = false;
    public bool wonGame { get; set; } = false;

    #endregion Vars

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

    #region Startup

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

        //store scene details
        currSceneName = SceneManager.GetActiveScene().name;

        //determine current scene
        if (SameString_IgnoreCase(currSceneName, "Lobby"))
        {
            currentScene = CurrentScene.LOBBY;
        }
        else if (SameString_IgnoreCase(currSceneName, "Game Lobby"))
        {
            currentScene = CurrentScene.GAME_LOBBY;
        }
        else if (SameString_IgnoreCase(currSceneName, "Game"))
        {
            currentScene = CurrentScene.GAME;
        }

        //default state to menus
        _state = State.MENU;
    }

    private void Start()
    {

        //if not starting in the lobby
        if ( currentScene != CurrentScene.LOBBY )
        {
            //wait till our player instance filled to fit vars in
            StartCoroutine(WaitTillPlayerInstanceFilled());
        }

        
    }

    /// <summary>
    /// Waits asynchly till player instance filled to fill fields
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitTillPlayerInstanceFilled()
    {
        //while player inst not filled
        while (ourPlayer == null)
        {
            Debug.Log("Wait 1 frame bc our player inst not filled.");

            //wait a frame
            yield return null;
        }

        //if our player is counselor
        if (ourPlayer.tag == "Player")
        {
            //cache jason player
            jasonPlayer = GameObject.FindGameObjectWithTag("Enemy");
        }
        //if our player is jason
        else
        {
            //store our player as jason player
            jasonPlayer = ourPlayer;
        }

        //player instance filled so cache player manager
        playerManager = ourPlayer.GetComponent<PlayerManager>();

        //startCheckingIfJasonLeft = true;
    }

    /// <summary>
    /// return whether the passed in strings are the same ignoring case
    /// </summary>
    /// <param name="str1"></param>
    /// <param name="str2"></param>
    /// <returns></returns>
    public bool SameString_IgnoreCase( string str1, string str2)
    {
        return string.Compare(str1, str2, StringComparison.OrdinalIgnoreCase) == 0;
    }

    #endregion Startup

    private void Update()
    {
        /*
        //if not game scene or jason already left or local play
        if( notGameScene ||  jasonLeft || !PhotonNetwork.IsConnected)
        {
            //don't check if jason left
            return;
        }

        //if(currentScene)

        //if jason player empty
        if(jasonPlayer == null)
        {
            //find jason player
            jasonPlayer = GameObject.FindGameObjectWithTag("Enemy");

            //if jason player actually left
            if (jasonPlayer == null)
            {
                //make sure player manager filled
                playerManager = ourPlayer.GetComponent<PlayerManager>();

                //tell remaining players they won
                playerManager.Win(isGameOver: true);
                
                //don't repeat telling players they won
                jasonLeft = true;
            }
            //jason player didn't actually leave
            else
            {
                playerManager = ourPlayer.GetComponent<PlayerManager>();
            }
        }
        */

        //stop playing music if it should stop:
        if (musicShouldPlay == false)
        {
            //audioSrc.Stop();
            audioSrc.Pause();
        }

        //run state code every frame
        switch (_state) 
        {
            case State.INIT:
                //spawn players and items
                /*
                if( playersSpawned )
                {
                    SwitchState(State.MENU);
                }
                */

                break;
            case State.MENU:

                break;
            case State.PLAY:
                //alow player control
                break;
            case State.SPECTATE:
                //allow player control
                break;
            case State.GAMEOVER:
                //revoke player control
                break;
            case State.LOADLEVEL:
                //revoke player control
                break;
            case State.PAUSE:
                //revoke player control
                break;
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
    /// RPC to incr # of dead counselors
    /// </summary>
    [PunRPC]
    public void RPC_IncrCounselorsDead()
    {
        deadCounselors++;
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

    #region Setters & Getters

    public void SetLostGame(bool isGameLost)
    {
        lostGame = isGameLost;
    }

    public bool GetLostGame()
    {
        return lostGame;
    }

    #endregion Setters & Getters

    #region State Methods

    //to change state to passed in state:
    public void SwitchState(State newState, float delay = 0) //aka next state logic process?
    {
        //print("Switch state to " + newState);

        //waits zero seconds unless 'delay' arg overwritten:
        StartCoroutine(SwitchDelay(newState, delay));
    }

    /// <summary>
    /// Switch scenes with the given delay by ending the curr state and beginning the next
    /// </summary>
    /// <param name="newState"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    private IEnumerator SwitchDelay(State newState, float delay) //changes _state to specified state and passes to end then begin after changing
    {
        _isSwitchingState = true;

        yield return new WaitForSeconds(delay);

        EndState();

        _state = newState; //bc just changed states below

        BeginState(newState);

        _isSwitchingState = false;

    }

    //does operations based on current state
    private void BeginState(State newState) //aka output logic process
    {
        //print("Current state: " + newState);

        switch (newState)
        {
            case State.MENU:
                
                break;
            case State.INIT: //for resetting score and such
                
                SwitchState(State.LOADLEVEL);
                break;
            case State.PLAY:
                break;
            case State.LOADLEVEL:

                break;
            case State.GAMEOVER:
                //panelGameOver.SetActive(true);
                break;
            case State.PAUSE:
                //panelPause.SetActive(true);
                //pauseApp(); //apply pause settings
                break;
        }
    }

    private void EndState()
    {
        //print("State ending: " + _state);

        switch (_state)
        {
            case State.MENU:
                //panelMenu.SetActive(false);
                break;
            case State.INIT:
                break;
            case State.PLAY:
                break;
            case State.LOADLEVEL:
                break;
            case State.GAMEOVER:
                //musicShouldPlay = false;
                //panelPlay.SetActive(false);
                //panelGameOver.SetActive(false);
                break;
            case State.PAUSE:
                //should resume playing paused audio src :
                //audioSrc.Play();
                //musicShouldPlay = true;

                //Cursor.visible = false;
                //panelPause.SetActive(false);
                //Time.timeScale = 1; //resume time
                break;
        }
    }

    #endregion State Methods

    #region Load Main Menu

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
    #endregion Load Main Menu
}

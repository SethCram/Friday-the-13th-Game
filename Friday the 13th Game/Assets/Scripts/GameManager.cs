using Cinemachine;
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

    #region needa incr/decr over RPC
    [HideInInspector]
    public int startVotes { private set; get; } = 0; 

    [HideInInspector]
    public int deadCounselors { private set; get; } = 0;

    [HideInInspector]
    public int playersSpawnReady { private set; get; } = 0;

    #endregion needa incr/decr over RPC

    #region Player Fields
    [HideInInspector]
    public GameObject jasonPlayer;
    [HideInInspector]
    public GameObject ourPlayer;
    [HideInInspector]
    public PlayerManager playerManager;
    public GameObject statsUI { get; set; }
    [HideInInspector]
    public GetSetStats getSetStats;
    #endregion Player Fields

    //fill in game scene's inspector
    public GameIntro gameIntro;

    /// <summary>
    /// Number of secs pass before check if players left again.
    /// </summary>
    public int checkIfPlayersLeftInterval = 1;

    #region Enums + Related Fields

    //state mach:
    public enum State { MENU, INIT, PLAY, LOADLEVEL, GAMEOVER, PAUSE, SPECTATE };
    private State _state;
    private State unpauseState;
    private bool _isSwitchingState;

    //scene tracking vars
    public enum CurrentScene { MAIN_MENU, LOADING, LOBBY, GAME_LOBBY, GAME };
    public CurrentScene currentScene { private set; get; }
    private string currSceneName;

    #endregion Enums + Related Fields

    //music vars
    private bool musicShouldPlay = true;
    private AudioSource audioSrc;

    //game success or failure 
    private bool lostGame = false;
    private bool wonGame = false;

    public bool localPlayerSpawned { set; private get; } = false;
    private int prevCounselorCount = 0;
    private bool gameReady = false;
    private bool checkingIfPlayersLeft = false;
    private bool customPropsSet = false;

    public bool localPlayerIsJason = false;

    #region Custom Prop Fields

    public const string WON_GAME_STR = "wonGame";
    public const string LOST_GAME_STR = "lostGame";

    //is jason/counselor fields
    public const string IS_JASON_STR = "isJason";
    public const string IS_COUNSELOR_STR = "isCounselor";

    public const string CUSTOM_PROP_TRUE = "True";
    public const string CUSTOM_PROP_FALSE = "False";

    #endregion Custom Prop Fields

    #region Tag Names
    public const string JASON_TAG = "Jason";
    public const string COUNSELOR_TAG = "Counselor";
    public const string PLAYER_TAG = "Player";
    #endregion Tag Names
    #region Scene Names
    public const string MAIN_MENU_SCENE_NAME = "Start Menu";
    public const string LOADING_SCENE_NAME = "Loading";
    public const string LOBBY_SCENE_NAME = "Lobby";
    public const string GAME_LOBBY_SCENE_NAME = "Game Lobby";
    public const string GAME_SCENE_NAME = "Game";
    #endregion Scene Names

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
        if(SameString_IgnoreCase(currSceneName, MAIN_MENU_SCENE_NAME))
        {
            currentScene = CurrentScene.MAIN_MENU;
        }
        else if (SameString_IgnoreCase(currSceneName, LOADING_SCENE_NAME))
        {
            currentScene = CurrentScene.LOADING;
        }
        else if (SameString_IgnoreCase(currSceneName, LOBBY_SCENE_NAME))
        {
            currentScene = CurrentScene.LOBBY;
        }
        else if (SameString_IgnoreCase(currSceneName, GAME_LOBBY_SCENE_NAME))
        {
            currentScene = CurrentScene.GAME_LOBBY;
        }
        else if (SameString_IgnoreCase(currSceneName, GAME_SCENE_NAME))
        {
            currentScene = CurrentScene.GAME;
        }

        //default state
        _state = State.INIT;
    }

    private void Start()
    {
        //if not starting in the lobby
        if ( currentScene != CurrentScene.LOBBY )
        {
            //wait till our player instance filled to fit vars in
            StartCoroutine(WaitTillGameInited());
        }

    }

    /// <summary>
    /// Waits asynchly till game inited
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitTillGameInited()
    {
        int framesWaited = 0;

        //wait while player inst not filled 
        while (ourPlayer == null )
        {
            framesWaited++;

            //wait a frame
            yield return null;
        }
        Debug.LogAssertion($"Waited {framesWaited} frames for player instance to fill.");

        //player instance filled so cache player manager
        playerManager = ourPlayer.GetComponent<PlayerManager>();

        //cache more fields
        getSetStats = statsUI.GetComponent<GetSetStats>();

        framesWaited = 0;

        //wait till all players spawned
        while(!AllPlayersSpawned())
        {
            Debug.Log("Waiting for all players to spawn.");

            framesWaited++;

            //wait a frame
            yield return null;
        }
        Debug.LogAssertion($"Waited {framesWaited} frames for all other players to spawn.");

        //if our player is a counselor
        if ( TagIsCounselor(ourPlayer.tag))
        {
            //cache jason player
            jasonPlayer = GameObject.FindGameObjectWithTag(JASON_TAG);
        }
        //if our player is jason
        else
        {
            //store our player as jason player
            jasonPlayer = ourPlayer;
        }

        gameReady = true;
    }

    #endregion Startup

    private void Update()
    {

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
                //if spawned local player + game ready
                if( localPlayerSpawned && gameReady )
                {
                    SwitchState(State.PLAY);
                }

                break;
            case State.MENU:

                break;
            case State.PLAY:
                //if we're the master client and not checking players left yet
                if (PhotonNetwork.IsMasterClient && !checkingIfPlayersLeft)
                {
                    //check players left every _ secs
                    StartCoroutine(CheckIfPlayersLeft(checkIfPlayersLeftInterval));

                    checkingIfPlayersLeft = true;
                }

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

        Debug.LogAssertion($"Dead couneslors incr'd to {deadCounselors}");
    }

    /// <summary>
    /// RPC to incr number of players loaded into game scene
    /// </summary>
    [PunRPC]
    public void RPC_IncrPlayersSpawnReady()
    {
        playersSpawnReady++;

        //debug: Debug.LogAssertion("players game ready = " + playersGameReady.ToString());
    }

    #endregion RPC's

    #region Setters & Getters

    [PunRPC]
    public void SetWhetherCustomPropsSet(bool ifCustomPropsSet)
    {
        customPropsSet = ifCustomPropsSet;
    }

    public bool GetWhetherCustomPropsSet()
    {
        return customPropsSet;
    }

    /// <summary>
    /// Set lost game param and update corresponding player's custom prop.
    /// </summary>
    /// <param name="isGameLost"></param>
    public void SetLostGame(bool isGameLost)
    {
        lostGame = isGameLost;

        //make new custom props inst for changing player props
        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();

        //copy over local player's custom props
        customProperties = PhotonNetwork.LocalPlayer.CustomProperties;

        //if game lost
        if(isGameLost)
        {
            //set lost game custom prop to true
            customProperties[GameManager.LOST_GAME_STR] = GameManager.CUSTOM_PROP_TRUE;
        }
        //if game not lost
        else
        {
            //set lost game custom prop to true
            customProperties[GameManager.LOST_GAME_STR] = GameManager.CUSTOM_PROP_FALSE;
        }

        //submit changed custom props
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
    }

    public bool GetLostGame()
    {
        return lostGame;
    }

    /// <summary>
    /// Set won game param and update corresponding player's custom prop.
    /// </summary>
    /// <param name="isGameWon"></param>
    public void SetWonGame(bool isGameWon)
    {
        wonGame = isGameWon;

        //make new custom props inst for changing player props
        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();

        //copy over local player's custom props
        customProperties = PhotonNetwork.LocalPlayer.CustomProperties;

        //if game won
        if (isGameWon)
        {
            //set lost game custom prop to true
            customProperties[WON_GAME_STR] = CUSTOM_PROP_TRUE;
        }
        //if game not lost
        else
        {
            //set lost game custom prop to false
            customProperties[WON_GAME_STR] = CUSTOM_PROP_FALSE;
        }

        //submit changed custom props
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
    }

    public bool GetWonGame()
    {
        return wonGame;
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
                //SwitchState(State.LOADLEVEL);
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

    #region Death Methods

    #region Public Death Methods

    /// <summary>
    /// Every activationDelay, check if a player left + take approp action.
    /// Works properly in Game + Game Lobby scenes.
    /// </summary>
    /// <param name="activationDelay"></param>
    /// <returns></returns>
    private IEnumerator CheckIfPlayersLeft( int activationDelay )
    {
        //wait for reactivation delay
        yield return new WaitForSeconds(activationDelay);

        //if jason left in game scene: (bc can't find his tag)
        if ( !JasonInRoom() && currentScene == CurrentScene.GAME)
        {
            Debug.LogAssertion("Jason left.");

            //walk thru players left
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                //tell each one game over
                TellCounselorGameOver(player);
            }

            //switch to game over state
            SwitchState(State.GAMEOVER);
        }
        //if jason didn't leave in game scene:
        else
        {
            //store curr # of counselors in scene
            int currCounselorCount = PhotonCounselorCount();

            //if counselor left: (bc used to be more)
            if (prevCounselorCount > currCounselorCount)
            {
                Debug.LogAssertion("A counselor left.");

                //update counselor count before recursion
                prevCounselorCount = currCounselorCount;

                //if in game scene
                if (currentScene == CurrentScene.GAME)
                {
                    //if not all counselors dead (don't die locally)
                    if (!CheckAllCounselorsDead(localDie: false))
                    {
                        //start check again
                        StartCoroutine(CheckIfPlayersLeft(activationDelay));
                    }
                    //if all counselors dead
                    else
                    {
                        //switch to game over state
                        SwitchState(State.GAMEOVER);
                    }
                }
                //if in game lobby scene
                else if(currentScene == CurrentScene.GAME_LOBBY)
                {
                    //start check again
                    StartCoroutine(CheckIfPlayersLeft(activationDelay));
                }
            }
            //if counselor didn't leave
            else
            {
                //update counselor count before recursion
                prevCounselorCount = currCounselorCount;

                //start check again
                StartCoroutine(CheckIfPlayersLeft(activationDelay));
            }
        }
    }

    /// <summary>
    /// Global refers to incr couneslor whether online or offline
    /// </summary>
    public void GlobalIncrCounselorsDead()
    {
        //if on network
        if (PhotonNetwork.IsConnected)
        {
            //incr # of dead counselors for everyone present + later joining
            photonView.RPC("RPC_IncrCounselorsDead", RpcTarget.AllBuffered);
        }
        //not on network
        else
        {
            //incr dead counselor count
            RPC_IncrCounselorsDead();
        }

    }

    /// <summary>
    /// If all counselors dead: Check how many counselors won/lost + act based on it.
    /// Calls local win/lose too whether networked or not.
    /// </summary>
    /// <param name="localDie">True if need to die locally.</param>
    /// <param name="localLose">True if need call local lost, false if need call local win. False by default.</param>
    /// <returns>Whether all counselors dead or not.</returns>
    public bool CheckAllCounselorsDead(bool localDie, bool localLose = false )
    {
        Debug.Log("dead counselors = " + deadCounselors);

        // if network not connected
        if(!PhotonNetwork.IsConnected)
        {
            //if local player should die
            if(localDie)
            {
                //if any counselors dead
                if (deadCounselors >= 1)
                {
                    //if need local loss
                    if(localLose)
                    {
                        //lose locally w/ game over
                        playerManager.Lose(isGameOver: true);
                    }
                    //if don't want local loss
                    else
                    {
                        //win locally w/ game over
                        playerManager.Win(isGameOver: true);
                    }

                    return true;
                }
                //no counselors dead
                else
                {
                    return false;
                }
            }
        }

        //if all counselors dead
        if (deadCounselors >= PhotonCounselorCount() )
        {
            Debug.LogAssertion("All counselors are dead. Game Over.");

            //if all counselors won
            if (AllCounselorsWon())
            {
                //counselors win + jason loses
                Debug.LogAssertion("Tell counselors they won and jason he lost.");
                BroadCastGameOver(allCounselorsWon: true, jasonLost: true);
            }
            //if all counselors lost
            else if (AllCounselorsLost())
            {

                //all counselors lose + jason wins
                Debug.LogAssertion("Tell counselors they lost and jason he won.");
                BroadCastGameOver(allCounselorsLost: true, jasonWon: true);
            }
            //if some counselors lost and others won
            else
            {
                //counselors who won win, those who lost lose, jason generic game over's
                Debug.LogAssertion("Tell counselors who won they win, " +
                    "counselors who lost they lose, and jason generic game over.");
                BroadCastGameOver(someCounselorsWonSomeLost: true);
            }

            return true;

            //should also do this in Update() incase someone leaves
        }
        //if not all counselors dead
        else
        {
            //still call local death
            if(localDie)
            {
                //if lose locally
                if (localLose)
                {
                    //lose locally w/ no game over
                    playerManager.Lose(isGameOver: false);
                }
                //if don't lose locally
                else
                {
                    //win locally w/ no game over
                    playerManager.Win(isGameOver: false);
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Tell given counselor game over w/ lose if they've lost, and win if they haven't lost yet.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="gameOver"></param>
    public void TellCounselorGameOver(Player player, bool gameOver = true)
    {
        //if player is jason
        if (PlayerIsJason(player))
        {
            Debug.LogError("Jason player passed into a counselor only method. " +
                "Method not executed.");
            return;
        }

        //if counselor already lost game
        if (PlayerAlreadyLost(player))
        {
            //lose with gameover
            playerManager.photonView.RPC("Lose", player, gameOver);
        }
        //if counselor hasn't lost yet
        else
        {
            //win with gameover
            playerManager.photonView.RPC("Win", player, gameOver);
        }
    }

    #endregion Public Death Methods

    #region Private Death Methods

    /// <summary>
    /// Broadcast game over to everyone, 
    ///  using given params and custom props to communicate correctly. 
    /// </summary>
    /// <param name="jasonLost"></param>
    /// <param name="jasonWon"></param>
    /// <param name="allCounselorsLost"></param>
    /// <param name="allCounselorsWon"></param>
    /// <param name="someCounselorsWonSomeLost"></param>
    private void BroadCastGameOver(
        bool jasonLost = false, 
        bool jasonWon = false, 
        bool allCounselorsLost = false, 
        bool allCounselorsWon = false, 
        bool someCounselorsWonSomeLost = false
    )
    {
        //game is over
        const bool gameOver = true;

        //walk thru players
        foreach (Player player in PhotonNetwork.PlayerList)
        {

            //if player is jason
            if ( PlayerIsJason(player) )
            {
                //if jason lost
                if( jasonLost )
                {
                    //tell jason they lost 
                    playerManager.photonView.RPC("Lose", player, gameOver);
                }
                //if jason won
                else if( jasonWon )
                {
                    //tell jason he won
                    playerManager.photonView.RPC("Win", player, gameOver);
                }
                //if some couneslors lost + others won
                else if( someCounselorsWonSomeLost )
                {
                    //tell jason they generic game over
                    playerManager.photonView.RPC("GenericGameOver", player);
                }

            }
            //if player is counselor
            else if (PlayerIsCounselor(player))
            {
                //if all counselor lost
                if (allCounselorsLost)
                {
                    //tell couneselor he lost
                    playerManager.photonView.RPC("Lose", player, gameOver);
                }
                //if all couneselors won
                else if (allCounselorsWon)
                {
                    //tell counselor he won
                    playerManager.photonView.RPC("Win", player, gameOver);
                }
                //if some couneslors won + some lost
                else if (someCounselorsWonSomeLost)
                {

                    TellCounselorGameOver(player);
                }
            }
            //if player isnt counselor or jason
            else
            {
                Debug.LogError("Player isn't Jason or Counselor.");
            }
        }
    }

    /// <summary>
    /// Determines if all counselors won game.
    /// </summary>
    /// <returns>Returns false if not all counselors dead yet, or no counselors ingame.</returns>
    private bool AllCounselorsWon()
    {
        //cache counselor count
        int counselorCount = CounselorCount();

        //if all counselors won + not all counselors left
        if (CountWonGameCounselors() >= counselorCount 
            && counselorCount != 0)
        {
            return true;
        }
        //if not all counselors won
        {
            return false;
        }
    }

    /// <summary>
    /// Determines if all counselors lost game.
    /// </summary>
    /// <returns>Returns false if not all counselors lost yet.</returns>
    private bool AllCounselorsLost()
    {
        //if all counselors lost (or left)
        if (CountLostGameCounselors() >= PhotonCounselorCount() )
        {
            return true;
        }
        //if not all counselors lost
        {
            return false;
        }
    }

    /// <summary>
    /// Uses the Photon player list to calculate # of counselors won the game.
    /// </summary>
    /// <returns>Returns number of counselors already won the game.</returns>
    private int CountWonGameCounselors()
    {
        int numOfGameWinningCounselors = 0;
        
        //walk thru players
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            //if counselor won game
            if ( PlayerAlreadyWon(player) 
                && PlayerIsCounselor(player) )
            {
                //incr # of won game counselors
                numOfGameWinningCounselors++;
            }
        }

        Debug.Log($"Number of game winning counselors {numOfGameWinningCounselors}");

        return numOfGameWinningCounselors;
    }

    /// <summary>
    /// Uses the Photon player list to calculate # of counselors won the game.
    /// </summary>
    /// <returns>Returns number of counselors already lost the game.</returns>
    private int CountLostGameCounselors()
    {
        int numOfGameLosingCounselors = 0;

        //walk thru players
        foreach (Player player in PhotonNetwork.PlayerList)
        {

            //if player lost game + is a counselor
            if ( PlayerAlreadyLost(player)
                && PlayerIsCounselor(player) )
            {
                //incr # of won game counselors
                numOfGameLosingCounselors++;
            }
        }

        Debug.Log($"Number of game losing counselors {numOfGameLosingCounselors}");

        return numOfGameLosingCounselors;
    }

    #endregion Private Death Methods

    #endregion Death Methods

    #region Cursor Methods

    /// <summary>
    /// Lock cursor to center of screen and make invisible.
    /// </summary>
    public void LockCursor()
    {
        //lock cursor and make invisible:
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Unlock cursor from the center of screen + make visible.
    /// </summary>
    public void UnlockCursor()
    {
        //unlock cursor and make visible:
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    #endregion Cursor Methods

    #region General Utility

    /// <summary>
    /// return whether the passed in strings are the same ignoring case
    /// </summary>
    /// <param name="str1"></param>
    /// <param name="str2"></param>
    /// <returns></returns>
    public bool SameString_IgnoreCase(string str1, string str2)
    {
        return string.Compare(str1, str2, StringComparison.OrdinalIgnoreCase) == 0;
    }

    #region Custom Property Methods

    /// <summary>
    /// Determines if player is Jason using custom props.
    /// </summary>
    /// <param name="player"></param>
    /// <returns>Whether given player is Jason.</returns>
    public bool PlayerIsJason( Player player )
    {
        object isJason = false;

        //get val of isJason
        player.CustomProperties.TryGetValue((object)IS_JASON_STR, out isJason);

        //if is jason returned as filled
        if( isJason != null)
        {
            //return if player is jason
            return SameString_IgnoreCase(isJason.ToString(), CUSTOM_PROP_TRUE);
        }
        //if cant find isJason custom prop
        else
        {
            Debug.LogError("Couldn't find isJason custom prop.");
            return false;
        }

    }

    /// <summary>
    /// Determines if player is Customer using custom props.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public bool PlayerIsCounselor( Player player)
    {
        object isCounselor = false;

        //get val of isCounselor
        player.CustomProperties.TryGetValue((object)IS_COUNSELOR_STR, out isCounselor);

        //if is counselor returned as filled
        if (isCounselor != null)
        {
            //return if player is counselor
            return SameString_IgnoreCase(isCounselor.ToString(), CUSTOM_PROP_TRUE);
        }
        //if cant find isCounselor custom prop
        else
        {
            Debug.LogError("Couldn't find isCounselor custom prop.");
            return false;
        }
    }

    public bool PlayerAlreadyWon( Player player )
    {
        object wonGame = false;

        //get val of wonGame
        player.CustomProperties.TryGetValue((object)WON_GAME_STR, out wonGame);

        //if counselor already won game
        return SameString_IgnoreCase(wonGame.ToString(), CUSTOM_PROP_TRUE);
    }

    public bool PlayerAlreadyLost( Player player )
    {
        object lostGame = false;

        //get val of lostGame
        player.CustomProperties.TryGetValue((object)LOST_GAME_STR, out lostGame);

        //if counselor already lost game
        return SameString_IgnoreCase(lostGame.ToString(), CUSTOM_PROP_TRUE);
    }

    #endregion Custom Property Methods

    #region Player Spawn/Intro Methods

    public void HideGameIntroPanel()
    {
        //deactivate game intro panel
        gameIntro.gameIntroPanel.SetActive(false);
    }

    public void ShowGameIntroPanel()
    {
        //activate game intro panel
        gameIntro.gameIntroPanel.SetActive(true);
    }

    /// <summary>
    /// Uses GameObject tags to determine whether all players spawned yet.
    /// Works in local + networked play.
    /// </summary>
    /// <returns>Whether all players spawned yet.</returns>
    private bool AllPlayersSpawned()
    {
        //if local play
        if(!PhotonNetwork.IsConnected)
        {
            //return spawn state of local player
            return localPlayerSpawned;
        }

        int playerExpectedCount = 0;

        //if game scene
        if (currentScene == CurrentScene.GAME)
        {
            //init expected players to curr player count
            playerExpectedCount = PhotonNetwork.CurrentRoom.PlayerCount;
        }
        //if game lobby
        else if( currentScene == CurrentScene.GAME_LOBBY)
        {
            //set expected players to max players for room
            playerExpectedCount = PhotonNetwork.CurrentRoom.MaxPlayers;
        }

        //Debug.Log($"Expected players = {playerExpectedCount}, Current players = {PlayerCount()}");

        //return whether same number of players counted as expected
        return PlayerCount() == playerExpectedCount;
    }

    #endregion Player Spawn/Intro Methods

    #region Tag Methods

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tag">Tag attached to player.</param>
    /// <returns>Whether tag is attached to Jason.</returns>
    public bool TagIsJason(string tag)
    {
        return SameString_IgnoreCase(tag, JASON_TAG);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tag">Tag attached to player.</param>
    /// <returns>Whether tag is attached to Counselor.</returns>
    public bool TagIsCounselor(string tag)
    {
        return SameString_IgnoreCase( tag, COUNSELOR_TAG );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tag">Tag attached to player.</param>
    /// <returns>Whether tag is attached to Player.</returns>
    public bool TagIsPlayer(string tag)
    {
        return SameString_IgnoreCase(tag, PLAYER_TAG);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tag">Tag attached to player.</param>
    /// <returns>Whether tag is attached to a playable character.</returns>
    public bool TagIsPlayableCharacter(string tag)
    {
        //return if tagged w/ jason or counselor or player
        return (GameManager.Instance.TagIsCounselor(tag)
            || GameManager.Instance.TagIsJason(tag)
            || GameManager.Instance.TagIsPlayer(tag));
    }

    #endregion Tag Methods

    #region Count Methods

    /// <summary>
    /// Counts number of counselors.
    /// </summary>
    /// <returns>Number of counselors spawned in.</returns>
    private int CounselorCount()
    {
        //if can find a counselor
        if(CounselorInRoom())
        {
            //return number of counselors
            return GameObject.FindGameObjectsWithTag(COUNSELOR_TAG).Length;
        }
        //if can find a counselor
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// Counts number of jasons.
    /// </summary>
    /// <returns>Number of jasons spawned in.</returns>
    private int JasonCount()
    {
        //if can find a jason
        if (JasonInRoom())
        {
            //return number of jasons
            return GameObject.FindGameObjectsWithTag(JASON_TAG).Length;
        }
        //if can find a jason
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// Counselor count according to Photon and the scene.
    /// Assumes only 1 jason in game scene.
    /// </summary>
    private int PhotonCounselorCount()
    {
        if (currentScene == CurrentScene.GAME)
        {
            return PhotonNetwork.CurrentRoom.PlayerCount - 1;
        }
        else
        {
            return PhotonNetwork.CurrentRoom.PlayerCount;
        }
    }

    /// <summary>
    /// Count number of players in room.
    /// </summary>
    /// <returns>Number of players in room.</returns>
    public int PlayerCount()
    {
        return JasonCount() + CounselorCount();
    }

    #endregion Count Methods

    #region Existence Methods

    /// <summary>
    /// 
    /// </summary>
    /// <returns>Whether a jason player is in the current room.</returns>
    private bool JasonInRoom()
    {
        return GameObject.FindGameObjectWithTag(JASON_TAG) != null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>Whether a counselor player is in the current room.</returns>
    private bool CounselorInRoom()
    {
        bool counselorInRoom = GameObject.FindGameObjectWithTag(COUNSELOR_TAG) != null;

        //Debug.Log($"Counselor in room = {counselorInRoom}");

        return counselorInRoom;
    }

    #endregion Existence Methods

    #endregion General Utility
}

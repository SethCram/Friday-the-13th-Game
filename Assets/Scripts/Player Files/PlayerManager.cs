using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; //used to reload scene
using Cinemachine; //needed to access cinemachine comps
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(ThirdPersonMovement))] //needed to cut/reconnect motion controls
public class PlayerManager : MonoBehaviourPunCallbacks
{
    #region Variables

    public GameObject player;
    
    public GameObject thirdPersonCamController; //need to activate/deactivate camera controls

    public Inventory inventory;
    public EquipmentManager equipmentManager;

    //cache so dont use 'getcomp' more than once:
    public ThirdPersonMovement playerMovement;

    [HideInInspector]
    public bool paused = false;

    //private bool showInteractMsg = false;

    //private GUIStyle guiStyle;
    private string msg = "";

    [HideInInspector]
    public OverlayUI overlayUI;
    
    [HideInInspector]
    public GameObject topOverlayUIObject;
    private Loading loadingUI;
    private GameOver gameOver;

    [HideInInspector]
    public GameObject minimapUI;

    //respawn/death vars
    private bool dead = false;
    private bool teleportPlayer = false;
    public Vector3 spectatorSpawn = new Vector3(0, 500, 0);
    private bool prevDead = false;
    private const string winText = "Congratulations, You Win!";
    private const string loseText = "You Lose";
    private const string gameOverText = "Game Over";
    private const string dieText = "You Died";

    private CharacterAnimator characterAnimator;
    [HideInInspector]
    public PausedUI pauseUI;

    public CharacterStats characterStats { get; private set; }

    public AudioSource soundEffectsAudioSrc;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        //if my photon view or not online
        if(photonView.IsMine || !PhotonNetwork.IsConnected)
        {
            Debug.Log("Audio Listener added to player.");

            //add audio listener to model player
            gameObject.AddComponent<AudioListener>();
        }
    }

    private void Start()
    {
        //just incase disabled for some reason
        //EnableCamControl();

        //if my photon view or not connected
        if(photonView.IsMine || !PhotonNetwork.IsConnected)
        {
            //cache UI overlay scripts
            loadingUI = topOverlayUIObject.GetComponent<Loading>();
            gameOver = topOverlayUIObject.GetComponent<GameOver>();
        }

        characterAnimator = GetComponent<CharacterAnimator>();
        characterStats = GetComponent<CharacterStats>();
    }

    private void Update()
    {

    }

    /// <summary>
    /// Fixed update used for respawning player
    /// ALWAYS USE FOR PHYSICS
    /// </summary>
    private void FixedUpdate()
    {
        //if should tele player
        if( teleportPlayer )
        {
            //respawn player 
            RespawnPlayer();
        }
    }

    #endregion

    #region Setters

    /// <summary>
    /// Used to sync player tag over RPC.
    /// </summary>
    /// <param name="tagName"></param>
    [PunRPC]
    public void SetTag(string tagName)
    {
        Debug.Log($"Tag set to {tagName}");

        tag = tagName;
    }

    public void SetTeleportPlayer( bool shouldTeleport)
    {
        teleportPlayer = shouldTeleport;
    }

    #endregion

    #region Scene Change Methods

    /// <summary>
    /// Load the nxt scene through cutting motion + cam controls and closing the room.
    /// </summary>
    public void AdvanceScene()
    {
        //cut player motion + disable cam movement
        CutMotionControls();
        DisableCamControl();

        //if scene is game lobby
        if(GameManager.Instance.currentScene == GameManager.CurrentScene.GAME_LOBBY)
        {
            //close room to joining players
            NetworkCloseRoom();
        }

        //asynchly load nxt scene
        //StartCoroutine(overlayUI.LoadLevelAsynch());
        StartCoroutine(loadingUI.LoadLevelAsynch());

    }

    /// <summary>
    /// close off room to network + future joining.
    /// </summary>
    private void NetworkCloseRoom()
    {
        //if connected + master client
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            //close room
            PhotonNetwork.CurrentRoom.IsOpen = false;

            //disable visibility in lobby
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }
    }

    //reload the scene:
    public void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); //reloads current scene using its 'buildIndex'
    }

    /*
    //reset player to main menu:
    public void ResetToMainMenu()
    {
        //use escape panel's disconnect + load
        //EscapePanel escapePanel = GameObject.FindObjectOfType(typeof(EscapePanel), true); //GameObject.FindObjectOfType(typeof(EscapePanel), true);

        StartCoroutine(DisconnectAndLoad());
    }
    */

    #endregion 

    #region Player Control

    //let player control character + lock cursor:
    public void EnablePlayerControl()
    {
        EnableCamControl();

        ResumeMotionControls();

        //lock cursor and make invisible:
        GameManager.Instance.LockCursor();
    }

    //dont let player control character + unlock cursor:
    public void DisablePlayerControl()
    {
        CutMotionControls();

        DisableCamControl();

        //unlock cursor and make visible:
        GameManager.Instance.UnlockCursor();
    }

    #endregion

    #region Small Player Control Methods
    

    public void DisableCamControl()
    {
        //pause mouse controls:
        CinemachineCore.UniformDeltaTimeOverride = 0;
    }

    private void EnableCamControl()
    {
        //re-enable mouse controls:
        CinemachineCore.UniformDeltaTimeOverride = -1; //reset time value
    }
    

    public void CutMotionControls()
    {
        playerMovement.cutMotionControls = true;
    }

    private void ResumeMotionControls()
    {
        playerMovement.cutMotionControls = false;
    }
    #endregion

    #region Destroy Methods

    public void LocalDestroyPhotonView(PhotonView destroyingView)
    {
        if(!photonView.IsMine)
        {
            Debug.LogError("Returned bc photon view isnt ours, this shouldn't be called.");
            return;
        }

        //tell all instances of our player to destroy this obj by passing its ID:  
        photonView.RPC("IndividualDestroy", RpcTarget.AllBuffered, destroyingView.ViewID);  //pass target as buffered so new players joining also have items destroyed
    }

    //destroy passed ID's obj for each individual game instance:
    [PunRPC]
    private void IndividualDestroy(int viewID)
    {
        //find photon view destroying using view ID:
        PhotonView destroyingView = PhotonView.Find(viewID);

        Destroy(destroyingView.gameObject);

        //checks:
        Debug.LogError("successfully destroyed gameobj locally"); //Debug.LogError("destroy owner is: " + destroyingView.Owner.ToString() + " and this photon view's owner is: " + this.photonView.Owner.ToString());
    }


    public void GlobalDestroyPhotonView(PhotonView destroyingView)
    {
        //if we arent the obj being destroyed current owner:
        if (destroyingView.Owner != photonView.Owner)
        {
            //transfer ownership to us:
            destroyingView.TransferOwnership(photonView.Owner);

            Debug.Log("Ownership transfer to: " + photonView.name);

            //delay obj destruction for ownership transfer to occur:
            StartCoroutine(DelayedNetworkDestroy(destroyingView.gameObject));

            return;
        }
        else
        {
            //we destroy obj across network:
            PhotonNetwork.Destroy(destroyingView.gameObject);

            Debug.Log("successfully destroyed networked gameobj");
        }
    }

    //delay destroy passed obj over the network:
    private IEnumerator DelayedNetworkDestroy(GameObject destroyingObj)
    {
        //deactivate this object inidividually using its ID: 
        photonView.RPC("DeactivateObject", RpcTarget.All, destroyingObj.GetPhotonView().ViewID);

        //wait for a second:
        yield return new WaitForSeconds(1);

        //destroy object after a second:
        PhotonNetwork.Destroy(destroyingObj);

        Debug.Log("successfully destroyed network gameobj w/ delay");
    }    

    //drop all equipment and inventory items
    public void DropEverything()
    {        
        //wait till drop all inventory
        while(!inventory.DropInventory());

        //wait till drop all equipment
        while(!equipmentManager.DropEquipment() );

        Debug.Log("All items dropped in " + GameManager.Instance.currentScene.ToString());
    }

    #endregion

    #region Photon Methods
    /*
    //disconnect client and load main menu:
    public IEnumerator DisconnectAndLoad()
    {

        //load main menu if no longer connected:
        SceneManager.LoadScene(0);

        //disconnect client from server:
        PhotonNetwork.Disconnect();

        //while still connected to network:
        while (PhotonNetwork.IsConnected)
        {
            //dont yet load scene:
            yield return null;
        }
    }
    */

    //called w/ another client leaves room:
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //dont do anything if not my photon view
        //if (!photonView.IsMine)
            //return;

        base.OnPlayerLeftRoom(otherPlayer);

        Debug.LogWarning(otherPlayer.NickName + " left the game.");

        //Debug.LogError("Leaving user view ID is: " + otherPlayer.UserId);

        //Debug.LogError("Curr photon view's user ID is: " + photonView.Owner.UserId);

        /*
        //if player leaving is the owner of this player photon view:
        if (otherPlayer.UserId == photonView.Owner.UserId)
        {
            //network destroy player obj:
            PhotonNetwork.Destroy(gameObject);

            Debug.LogError("Destroyed leaving player gameobj.");
        }
        */
    }

    #endregion

    #region Game Over

    /// <summary>
    /// respawn player thru teleporting player, allowing player to die again, giving them control and closing menus
    /// ALWAYS CALL FROM FIXED UPDATE (bc teleport)
    /// </summary>
    private void RespawnPlayer()
    {
        //teleport to spectator spawn
        transform.position = spectatorSpawn;

        //dont teleport player again
        SetTeleportPlayer(false);

        //stop animing dead player
        characterAnimator.SetAnimDead(false);

        //allow player control
        EnablePlayerControl();

        //tell to close all menus
        pauseUI.CloseAllChildrenPanels();

        //reset hp to max
        characterStats.SetHealthToMax();

        //if game lobby scene:
        if( GameManager.Instance.currentScene == GameManager.CurrentScene.GAME_LOBBY )
        {
            //show stats menu
            GameManager.Instance.statsUI.SetActive(true);

            //disable player control of their char to access stats UI
            DisablePlayerControl();
        }
        //if game scene:
        else if( GameManager.Instance.currentScene == GameManager.CurrentScene.GAME )
        {
            //make sure stats menu hidden 
            GameManager.Instance.statsUI.SetActive(false);
        }
    }

    /// <summary>
    /// show game over + death screen w/ only main menu btn
    /// </summary>
    [PunRPC]
    public void GenericGameOver()
    {
        ShowDeathScreen(gameOverText);

        //show only the main menu btn
        gameOver.ShowOnlyMainMenuBtn();
    }

    /// <summary>
    /// show death screen w/ generic death txt
    /// </summary>
    public void GenericDeathScreen()
    {
        ShowDeathScreen(dieText);
    }

    /// <summary>
    /// lose game thru showing death screen + setting lose var
    /// only runs if haven't already won or lost game + game over
    /// </summary>
    [PunRPC]
    public void Lose(bool isGameOver)
    {

        //set game as lost just incase player didn't die (not needed but not bad)
        GameManager.Instance.SetLostGame(true);

        //if game over
        if (isGameOver)
        {
            //show lose game over screen
            ShowDeathScreen(gameOverText + " " + loseText);

            //show only the main menu btn
            gameOver.ShowOnlyMainMenuBtn();
        }
        //if not game over
        else
        {
            //show lose game over screen
            ShowDeathScreen(loseText);
        }
    }

    /// <summary>
    /// win game thru displaying death screen
    /// if game over don't allow respawn
    /// </summary>
    /// <param name="isGameOver"></param>
    [PunRPC]
    public void Win(bool isGameOver)
    {

        //set game as won just incase player didn't die (not needed but not bad)
        GameManager.Instance.SetWonGame(true);

        //if game over
        if (isGameOver)
        {
           //show win game over screen
           ShowDeathScreen(gameOverText + " " + winText);

           //show only the main menu btn
           gameOver.ShowOnlyMainMenuBtn();
        }
        //if not game over
        else
        {
            //show win screen
            ShowDeathScreen(winText);
        }
    }

    /// <summary>
    /// show given game over text 
    /// </summary>
    /// <param name="gameOverTxt"></param>
    public void ShowDeathScreen(string gameOverTxt)
    {
        
        //not my photon view + connected to network
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            //game over not shown
            //Debug.LogAssertion("game over not shown bc not my photon view");
            //return;

            //find game over script (should only be one)
            gameOver = FindObjectOfType<GameOver>();
        }

        //reactivate game over screen
        gameOver.gameOverPanel.SetActive(true);

        //update title w/ game over txt
        gameOver.UpdateTitleText(gameOverTxt);

        //disable player control
        DisablePlayerControl();

    }

    /// <summary>
    /// When jason dies, tell counselors they won/lost and I won/lost + game over for everyone
    /// </summary>
    public void JasonDied( bool locaPlayerWon)
    {
        SetDead(true);

        Debug.LogAssertion("<color=yellow>Jason dead</color>");

        //if on network and more than 1 player in room
        if (PhotonNetwork.IsConnected 
            && GameManager.Instance.PlayerCount() > 1)
        {
            //walk thru room players
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                //if player isn't local (isn't jason)
                if (!player.IsLocal)
                {
                    //tell counselor game over and who lost lose and who won win
                    GameManager.Instance.TellCounselorGameOver(player);
                }
            }
        }

        if( locaPlayerWon)
        {
            Win(isGameOver: true);
        }
        else
        {
            //make jason player lose bc died + game over
            Lose(isGameOver: true);
        }

        //should do this in update incase jason leaves
    }

    /// <summary>
    /// When counselor dies, incr dead counselors and win/lose. 
    /// Gameover if all counselors dead.
    /// </summary>
    public void CounselorDied(bool localPlayerWon)
    {
        SetDead(true);

        Debug.Log("Counselor dead");

        GameManager.Instance.GlobalIncrCounselorsDead();

        //if player won 
        if( localPlayerWon)
        {
            GameManager.Instance.SetWonGame(true);
        }
        //if player didn't win but dying
        else
        {
            //set game as lost by local dead counselor
            GameManager.Instance.SetLostGame(true);
        }


        //check if all counselors dead + kill local player 
        Debug.Log("Check for if all counselors dead resulted in: " +
            GameManager.Instance.CheckAllCounselorsDead(localDie: true, localLose: !localPlayerWon));
    }

    #endregion

    #region Dead Get&Set

    public bool GetDead()
    {
        return dead;
    }

    /// <summary>
    /// Set dead to what's passed in + reset dead collider if no longer dead.
    /// </summary>
    /// <param name="isDead"></param>
    public void SetDead(bool isDead)
    {
        //set dead to passed in
        dead = isDead;

        //if player set to not dead and was dead
        if(!dead && prevDead)
        {
            //reset dead colliders
            
            //if network
            if(PhotonNetwork.IsConnected)
            {
                playerMovement.photonView.RPC("ResetDeadColliders", RpcTarget.AllBuffered);
            }
            //if local
            else
            {
                playerMovement.ResetDeadColliders();
            }
            
        }

        //store state of death for nxt time
        prevDead = dead;
    }

    #endregion Dead Get&Set

    #region GUI Config

    /// <summary>
    /// Change visibility of interaction msg 
    /// </summary>
    /// <param name="state">Is the interactable message visible?</param>
    public void SetInteractVisibility(bool state)
    {
        //if txt showing and overlay UI set
        if( overlayUI != null ) // && state == true)
        {
            Debug.Log("interact msg: " + msg);

            //make sure correct message shown
            overlayUI.interactTxt.text = msg;

            //invoke method incase txt doesnt dissapear in 5 seconds
            //TurnIntTextOffIn(5);

            //change state of txt
            overlayUI.interactTxt.gameObject.SetActive(state);
        }
    }

    //change msg contents
    public void SetInteractMsg(string message)
    {
        msg = message;
    }

    //End of GUI Config --------------
    #endregion

    [PunRPC]
    public void DeactivateObject(int viewID)
    {
        //find photon view deactivating using view ID:
        PhotonView deactivatingView = PhotonView.Find(viewID);

        //deactivate gameobj:
        deactivatingView.gameObject.SetActive(false);

        Debug.Log(deactivatingView.gameObject.name + " set deactive");
    }

    /// <summary>
    /// Play sound FX player audio. Callable over RPC.
    /// </summary>
    /// <param name="audioClipName">Used to get the audio clip to play from the Audio Manager.</param>
    [PunRPC]
    public void PlaySoundFXAudioSource(string audioClipName)
    {
        Debug.Log("Play the player's sound FX audio source.");

        if (soundEffectsAudioSrc != null)
        {
            if (soundEffectsAudioSrc.isPlaying)
            {
                soundEffectsAudioSrc.Stop();
            }

            Sound desiredSound = AudioManager.instance.soundsDict[audioClipName];

            if(desiredSound != null)
            {
                //slide approp clip into player audio src
                soundEffectsAudioSrc.clip = desiredSound.source.clip;
            }
            else
            {
                Debug.LogWarning("Played sound FX audio src, but passed in clip not found.");
            }

            soundEffectsAudioSrc.Play();
        }
        else
        {
            Debug.LogWarning("Sound effects audio src needs filling.");
        }
    }
}

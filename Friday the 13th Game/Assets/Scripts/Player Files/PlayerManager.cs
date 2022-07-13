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
    private bool lostGame = false;
    private bool teleportPlayer = false;
    public Vector3 spectatorSpawn = new Vector3(0, 500, 0);
    private bool prevDead = false;

    private CharacterAnimator characterAnimator;
    [HideInInspector]
    public PausedUI pauseUI;

    #endregion

    #region Unity Methods

    private void Start()
    {
        //just incase disabled for some reason
        EnableCamControl();

        //if my photon view or not connected
        if(photonView.IsMine || !PhotonNetwork.IsConnected)
        {
            //cache UI overlay scripts
            loadingUI = topOverlayUIObject.GetComponent<Loading>();
            gameOver = topOverlayUIObject.GetComponent<GameOver>();
        }

        characterAnimator = GetComponent<CharacterAnimator>();

    }

    private void Update()
    {
        /*
        //if dead & any key pressed
        if( dead && Input.anyKeyDown )
        {
            //reactivate game over screen
            gameOver.gameOverPanel.SetActive(false);

            //no longer dead
            dead = false;

            //enable player cntrl
            EnablePlayerControl();

            //try to tele player
            teleportPlayer = true;

        }
        */
    }

    /// <summary>
    /// Fixed update used for respawning player
    /// ALWAYS USE FOR PHYSICS
    /// </summary>
    private void FixedUpdate()
    {
        //if lost game, should teleport player and a counselor
        if( lostGame && teleportPlayer && tag == "Player" )
        {
            //respawn player 
            RespawnPlayer();
        }
    }

    #endregion

    #region Setters

    public void SetLostGame(bool isGameLost)
    {
        lostGame = isGameLost;
    }

    public void SetTeleportPlayer( bool shouldTeleport)
    {
        teleportPlayer = shouldTeleport;
    }

    #endregion

    #region Scene Change Methods

    //Load the nxt scene:
    public void AdvanceScene()
    {
        //cut player motion + disable cam movement
        CutMotionControls();
        DisableCamControl();

        //asynchly load nxt scene
        //StartCoroutine(overlayUI.LoadLevelAsynch());
        StartCoroutine(loadingUI.LoadLevelAsynch());

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

    //let player control character:
    public void EnablePlayerControl()
    {
        EnableCamControl();

        ResumeMotionControls();

        //lock cursor and make invisible:
        LockCursor();
    }

    //dont let player control character:
    public void DisablePlayerControl()
    {
        CutMotionControls();

        DisableCamControl();

        //unlock cursor and make visible:
        UnlockCursor();
    }

    #endregion

    #region Small Player Control Methods
    private void LockCursor()
    {
        //lock cursor and make invisible:
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        //unlock cursor and make visible:
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

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

        Debug.Log("All items dropped in " + SceneManager.GetActiveScene().name);
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

        //allow player to die again
        SetLostGame(false);

        characterAnimator.SetAnimDead(false);

        //allow player control
        EnablePlayerControl();

        //tell to close all menus
        pauseUI.CloseAllChildrenPanels();
    }

    public void Lose()
    {
        //GameManager gameManager = FindObjectOfType<GameManager>();

        if (GameManager.Instance == null)
        {
            Debug.LogError("Game manager null, so lose() failed.");
            return;
        }

        //store that lost 
        lostGame = true;

        //if counselor loses
        if (tag == "Player")
        {
            Debug.Log("Counselor dead");

            //incr # of dead counselors (locally)
            GameManager.Instance.RPC_ChangeCounselorsDead(GameManager.Instance.deadCounselors + 1);

            //if on network
            if (PhotonNetwork.IsConnected)
            {
                //if all players besides 1 or actually all dead 
                if (GameManager.Instance.deadCounselors >= PhotonNetwork.CurrentRoom.PlayerCount - 1)
                {
                    //boot player back to main menu 
                }
                else
                {

                }
            }
            //not on network
            else
            {
                //boot player back to main menu 
            }

        }
        //if jason loses
        else if( tag == "Enemy" )
        {
            //tell all counselors they won
        }
    }

    public void Win()
    {

    }

    public void ShowGameOver(string gameOverTxt)
    {
        //not my photon view + connected to network
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            //game over not shown
            Debug.Log("game over not shown");
            return;
        }

        //reactivate game over screen
        gameOver.gameOverPanel.SetActive(true);

        //update title w/ game over txt
        gameOver.UpdateTitleText(gameOverTxt);

        //disable player control
        DisablePlayerControl();

    }

    #endregion

    #region Dead Methods

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

    #endregion Dead Methods

    #region GUI Config

    //change visibility of interaction msg 
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

}

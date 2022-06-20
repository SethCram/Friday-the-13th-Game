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
    
    public Transform jasonStart;
    public Transform counselorStart;
    public GameObject gameLevel;
    public GameObject lobbyLevel;
    

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
    public Loading loadingUI;

    [HideInInspector]
    public GameObject minimapUI;

    [HideInInspector]
    public int startVotes { private set; get; } = 0; //needa incr/decr over RPC
    public GameManager gameManager;

    //private bool startedGame = false;

    #endregion

    #region Methods

    #region Unity Methods

    private void Start()
    {
        //playerMovement = GetComponent<ThirdPersonMovement>();

        gameManager = FindObjectOfType<GameManager>();
        
        //only find fields if this photon view mine or connected to network
        if(photonView.IsMine || !PhotonNetwork.IsConnected)
        {
            //fill level fields
            gameLevel = GameObject.FindGameObjectWithTag("GameLevel");
            lobbyLevel = GameObject.FindGameObjectWithTag("LobbyLevel");
            if (gameLevel != null && lobbyLevel != null)
            {
                gameLevel.SetActive(false);
                lobbyLevel.SetActive(true);
            }

            //fill spawn fields:
            //jasonStart = GameObject.FindGameObjectWithTag("JasonStart").transform;
            //counselorStart = GameObject.FindGameObjectWithTag("CounselorStart").transform;
        }

        //just incase disabled for some reason
        EnableCamControl();

        //make sure cursor usable
        //UnlockCursor();
    }

    private void Update()
    {
        /*
        //vote key pressed and havent started game
        if (Input.GetKeyDown(KeyCode.V) && startedGame == false)
        {
            //teleport players to start
            TeleportPlayers();
        }
        */
    }

    #endregion

    #region Start Game Methods

    /// <summary>
    /// Teleport players to the needed locations if master client
    /// </summary>
    public void TeleportPlayers()
    {
        //pick rando # tween 1 and number of players in room
        //int enemyIndex = Random.Range(min: 0, max: PhotonNetwork.PlayerList.Length);

        //store players
        Player[] playerList = PhotonNetwork.PlayerList;

        int index = 0;

        //Debug.LogError("Enemy index = " + enemyIndex);

        //if master client 
        //if (PhotonNetwork.IsMasterClient )
        //{
            Debug.Log("Teleport players");

            foreach (Player pl in PhotonNetwork.PlayerList)
            {
                //if 1st player
                if(index == 0)
                {
                    //start player as Jason
                    photonView.RPC("StartPlayer", playerList[index], index, true);
                }
                //not 1st player
                else
                {
                    //start player as counselor
                    photonView.RPC("StartPlayer", playerList[index], index, false);
                }
                index++;
            }

            /*
            //walk thru player list
            for (int i = 0; i < playerList.Length; i++)
            {
                if( i == enemyIndex)
                {
                    //start given player as jason
                    photonView.RPC("StartPlayer", playerList[i], i, true);
                }
                else
                {
                    //start given player as counselor
                    photonView.RPC("StartPlayer", playerList[i], i, false);
                }
            }
            */
        //}
        //if not connected to network
        //else
        if (!PhotonNetwork.IsConnected)
        {
            //start local player as counselor
            StartPlayer(0);
        }
    }

    /*
    /// <summary>
    /// deactivate lobby + activate game approp levels
    /// </summary>
    public void ChangeLevels()
    {
        gameLevel.SetActive(true);

        lobbyLevel.SetActive(false);
    }
    */

    /// <summary>
    /// Spawn player and change tag according to whether jason or not.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="jason"></param>
    [PunRPC]
    private void StartPlayer(int index, bool jason = false)
    { 
        //if jason
        if(jason)
        {
            player.transform.position = new Vector3(515, 5, 121);
            player.tag = "Enemy";
        }
        //if not jason
        else
        {
            player.transform.position = new Vector3(547 + index * 2, 5, -343);
        }

        //show game level
        gameManager.ChangeLevels();

        //label game as started
        //startedGame = true;

        Debug.LogError("Player " + PhotonNetwork.NickName + " started at " + player.transform.position.ToString());
        Debug.LogError("Player is Jason? " + jason);
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

    //reset player to main menu:
    public void ResetToMainMenu()
    {
        //use escape panel's disconnect + load
        //EscapePanel escapePanel = GameObject.FindObjectOfType(typeof(EscapePanel), true); //GameObject.FindObjectOfType(typeof(EscapePanel), true);

        StartCoroutine(DisconnectAndLoad());
    }

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

    private void UnlockCursor()
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
    }

    #endregion

    #region Photon Methods

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

    #region Start Players

    public void StartJasonWrapper(Player photonPlayer)
    {
        photonView.RPC("RPC_StartJason", photonPlayer, jasonStart.position);
    }

    /// <summary>
    /// Start jason out at spawn point
    /// </summary>
    [PunRPC]
    private void RPC_StartJason(Vector3 startPos)
    {
        Debug.Log("Starting as Jason");

        //change tag
        player.tag = "Enemy";

        //teleport Jason to start
        player.transform.position = startPos; //jasonStart.position;
    }

    public void StartCounselorWrapper(Player photonPlayer)
    {
        photonView.RPC("RPC_StartCounselor", photonPlayer, counselorStart.position);
    }

    /// <summary>
    /// start counselors out at spawnpoint 
    /// </summary>
    [PunRPC]
    private void RPC_StartCounselor(Vector3 startPos)
    {
        Debug.Log("Starting as counselor");

        //teleport counselor to start
        player.transform.position = startPos; //counselorStart.position;
    }

    /// <summary>
    /// RPC to change start vote count.
    /// </summary>
    /// <param name="newVoteCount"></param>
    [PunRPC]
    public void RPC_ChangeVoteCount( int newVoteCount )
    {
        startVotes = newVoteCount;
    }

    #endregion

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

    #endregion
}

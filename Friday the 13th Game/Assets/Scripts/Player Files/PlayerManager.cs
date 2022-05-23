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
    public GameObject player;

    public GameObject thirdPersonCamController; //need to activate/deactivate camera controls

    public Inventory inventory;
    public EquipmentManager equipmentManager;

    //cache so dont use 'getcomp' more than once:
    private ThirdPersonMovement playerMovement;

    [HideInInspector]
    public bool paused = false;

    //private bool showInteractMsg = false;

        //for item replacement on leave
        //private PlayerManager[] playerManagers;
        //private int playerCount = 0;

    //private GUIStyle guiStyle;
    private string msg = "";

    public OverlayUI overlayUI;

    private void Start()
    {
        playerMovement = GetComponent<ThirdPersonMovement>();

        //just incase disabled for some reason
        EnableCamControl();

            //store player pos's every 3rd arg starting at 2nd arg
            // InvokeRepeating("StorePlayers", 1, 5);

        //setup GUI style settings for user prompts
        //setupGui();
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

    #region Action Method
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

    private void DisableCamControl()
    {
        //pause mouse controls:
        CinemachineCore.UniformDeltaTimeOverride = 0;
    }

    private void EnableCamControl()
    {
        //re-enable mouse controls:
        CinemachineCore.UniformDeltaTimeOverride = -1; //reset time value
    }

    private void CutMotionControls()
    {
        playerMovement.cutMotionControls = true;
    }

    private void ResumeMotionControls()
    {
        playerMovement.cutMotionControls = false;
    }
    #endregion

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

            Debug.LogError("successfully destroyed networked gameobj");
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

    [PunRPC]
    public void DeactivateObject(int viewID)
    {
        //find photon view deactivating using view ID:
        PhotonView deactivatingView = PhotonView.Find(viewID);

        //deactivate gameobj:
        deactivatingView.gameObject.SetActive(false);

        Debug.Log(deactivatingView.gameObject.name + " set deactive");
    }

    //drop all equipment and inventory items
    public void DropEverything()
    {        
        //wait till drop all inventory
        while(!inventory.DropInventory());

        //wait till drop all equipment
        while(!equipmentManager.DropEquipment() );
    }

    /*
    private void StorePlayers()
    {

        //stor all player manager's
        playerManagers = FindObjectsOfType<PlayerManager>();

        //if less player's than before
        if( playerManagers.Length < playerCount)
        {

        }

        //step thru all player manager's
        foreach (PlayerManager manager in playerManagers)
        {

        }
    }

    //respawn lost items
    private void RestoreLostPickups()
    {
        ItemPickup[] items = FindObjectsOfType<ItemPickup>();
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

    #region GUI Config

    /*
    private void Update()
    {
        //if showing msg
        if(showInteractMsg)
        {
            //make sure correct message shown
            overlayUI.interactTxt.text = msg;
        }
    }
    */

    //change visibility of interaction msg 
    //[PunRPC]
    public void SetInteractVisibility(bool state)
    {
        //showInteractMsg = state;

        //if txt showing
        if(state == true)
        {
            //make sure correct message shown
            overlayUI.interactTxt.text = msg;

            //invoke method incase txt doesnt dissapear in 5 seconds
            //TurnIntTextOffIn(5);
        }

        //change state of txt
        overlayUI.interactTxt.gameObject.SetActive(state);
    }

    /*
    private IEnumerator TurnIntTextOffIn(float delay)
    {
        yield return new WaitForSeconds(delay);

        //turn off interaction text
        overlayUI.interactTxt.gameObject.SetActive(false);
    }
    */

    //change msg contents
    public void SetInteractMsg(string message)
    {
        msg = message;
    }
    /*
    //configure the style of the GUI
    private void setupGui()
    {
        guiStyle = new GUIStyle();
        guiStyle.fontSize = 16;
        guiStyle.fontStyle = FontStyle.Bold;
        guiStyle.normal.textColor = Color.white;
        msg = "Press E/Fire1 to Interact";
    }

    void OnGUI()
    {
        Debug.Log("show interact msg: " + showInteractMsg);

        if (showInteractMsg)  //show on-screen prompts to user for guide.
        {
            GUI.Label(new Rect(50, Screen.height - 50, 200, 50), msg, guiStyle);
        }
    }
    */

    //End of GUI Config --------------
    #endregion
}

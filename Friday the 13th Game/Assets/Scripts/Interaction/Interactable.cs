using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// Superclass for all types of interactables (pickups and openings)
/// </summary>
//need photon view
[RequireComponent(typeof(PhotonView))]
public class Interactable : MonoBehaviourPunCallbacks
{
    #region Vars

    public float interactionRadius = 3f; // distance player has to be from obj to interact with it

    //public Transform interactionTransform; //if want interaction pnt to be not around obj, set this to desired pnt

    private SphereCollider sphereCollider; //to setup sphere trigger

    //to store all players able to interact with this obj:
    [HideInInspector]
    public List<Transform> interactablePlayers;

    //private bool showInteractMsg;
    //private GUIStyle guiStyle;
    private string msg;

    [HideInInspector]
    public bool isOpen = false;

    #endregion Vars

    #region Unity Methods

    public virtual void Start()
    {
        //setup sphere trigger

        sphereCollider = GetComponent<SphereCollider>();

        if( sphereCollider != null)
        {
            //set sphere's trigger radius to interaction radius:
            sphereCollider.radius = interactionRadius;
            sphereCollider.isTrigger = true;

        }

        //init list of transforms:
        interactablePlayers = new List<Transform>();

    }

    private void Update()
    {
        //if any interactable players:
        if (interactablePlayers.Count > 0)
        {
            //showInteractMsg = true;

            msg = getGuiMsg(isOpen);

            //if a player within radius pressed the 'Interact' button, call 'Interact()':
            foreach (Transform player in interactablePlayers)
            {
                //if player deleted:
                if (player == null)
                {
                    //remove player from being able to interact:
                    interactablePlayers.Remove(player);
                }
                //player not deleted
                else
                {
                    if (player.GetComponent<PlayerButtons>().playerInteract)
                    {
                        Interact(player);

                        //could add a private 'interacted' bool to make sure only 1 player picks up item, if becomes a problem 
                    }
                }
            }
        }
        else
        {
            //showInteractMsg = false;

        }
    }

    //add player to interactable players list w/ enter radius:
    public virtual void OnTriggerEnter(Collider other)
    {
        //only add an interactable player if tagged as playable character
        if( GameManager.Instance.TagIsPlayableCharacter(other.tag))
        {
            PlayerManager playerManager = other.gameObject.GetComponent<PlayerManager>();

            //update msg
            playerManager.SetInteractMsg(msg);

            //show interaction msg
            playerManager.SetInteractVisibility(true);

            interactablePlayers.Add(other.transform);

            //Debug.Log("Added: " + other.transform.name);

        }
    }

    //remove player from interactable players list w/ exit radius:
    public virtual void OnTriggerExit(Collider other)
    {
        //only add an interactable player if tagged as playable character
        if (GameManager.Instance.TagIsPlayableCharacter(other.tag))
        {
            //unshow interaction msg
            other.gameObject.GetComponent<PlayerManager>().SetInteractVisibility(false);

            interactablePlayers.Remove(other.transform);

            //Debug.Log("Removed: " + other.transform.name);
        }
    }

    private void OnDestroy()
    {
        /*
        //for every interactable player
        foreach (Transform player in interactablePlayers)
        {
            //make msgs dissapear
            player.GetComponent<PlayerManager>().SetInteractVisibility(false);
        }
        Debug.Log(interactablePlayers.ToString());
        */
    }

    #endregion Unity Methods

    //arg of transform is player interacting with this:
    public virtual void Interact(Transform playerInteracting) //'virtual' so this method can be overwritten in any children classes of this class (dif for each child class)
    {
        //this method meant to be overwritten

        Debug.Log("Interacting w/: " + transform.name);

        //update msg
        msg = getGuiMsg(!isOpen);
    }

    #region GUI Config

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
    */

    public virtual string getGuiMsg(bool isOpen)
    {
        string rtnVal;
        if (isOpen)
        {
            rtnVal = "Press E/Fire1 to Close";
        }
        else
        {
            rtnVal = "Press E/Fire1 to Open";
        }

        return rtnVal;
    }
    /*
    void OnGUI()
    {
        if (showInteractMsg)  //show on-screen prompts to user for guide.
        {
            GUI.Label(new Rect(50, Screen.height - 50, 200, 50), msg, guiStyle);
        }
    }
    */
    //End of GUI Config --------------
    #endregion

    //draw interaction radius
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}

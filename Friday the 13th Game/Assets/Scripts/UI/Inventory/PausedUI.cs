using UnityEngine;
using TMPro;

public class PausedUI : MonoBehaviour
{
    #region Variables 

    //init before instantiation:
    public GameObject inventoryPanel; //gameobj of displayed inventory panel
    public GameObject optionsPanel; //gameobj of displayed options panel
    public GameObject escapePanel; //gameobj of displayed escape panel
    
    public Transform txtSpawnPnt; //used by text spawning inventory slots

    public PlayerManager playerManager; //made public to init before its instantiation

    //public PlayerButtons playerButtons; //made public to init before its instantiation

    //for multi option menu:
    public Transform invenPanel;
    public GameObject multiOptionsUI;
    private GameObject optionsCopy;

    public bool UI_Accessible = true;

    //so dont open other UI w/ one UI active:
    private bool inventory_UI_Accessible = true;
    private bool escape_UI_Accessible = true;

    private int numOfChildren = 0;
    private int deactiveChildren = 0;

    #endregion

    #region Unity Methods

    // Start is called before the first frame update
    void Start()
    {
        //set children invisible by default:
        foreach (Transform child in transform)
        {
            //incr num of childs
            numOfChildren++;

            child.gameObject.SetActive(false);
        }
    }

    // Update pause UI if buttons pressed and not other UI active:
    void LateUpdate()
    {
        //loop thru each child obj
        foreach (Transform child in transform)
        {
            //if child deactive
            if( !child.gameObject.activeSelf )
            {
                //count it
                deactiveChildren++;
            }
        }

        //if all panels deactive
        if( deactiveChildren == numOfChildren )
        {
            //not paused
            playerManager.paused = false;
        }
        //a panel active
        else
        {
            //paused
            playerManager.paused = true;
        }

        //rst num of deactive children
        deactiveChildren = 0;

        //if press inventory button and its currently accessible:
        if(Input.GetButtonDown("Inventory") && inventory_UI_Accessible)
        {
            //if inventory open:
            if(inventoryPanel.activeSelf)
            {
                CloseInventory();
            }
            //if inventory closed:
            else
            {
                OpenInventory();
            }
        }
        //if pressed escape button, escape panel not active, its accessible, and options panel not active:
        else if(Input.GetButtonDown("Escape") && !(escapePanel.activeSelf) && escape_UI_Accessible && !(optionsPanel.activeSelf))
        {
            OpenEscapeMenu();
        }

        //if inventory open + left click anywhere, try and destroy the dropdown:
        if (inventoryPanel.activeSelf && Input.GetKeyDown(KeyCode.Mouse0))
        {
            //destroy options menu w/ a delay:
            DestroyMultiOptions();
        }
    }

    #endregion

    #region Multi Option Methods

    //called by Inventory Slot script w/ bag slot selected:
    public void MakeMultiOptions(InventorySlot slot)
    {
        //destroy options menu if another one present:
        DestroyMultiOptions();

        //instantiate options menu at mouse position and set passed in inventory panel as its parent:
        optionsCopy = Instantiate(multiOptionsUI, Input.mousePosition, Quaternion.identity, invenPanel);

        //make visible:
        optionsCopy.SetActive(true);

        //pass slot options created from into it:
        optionsCopy.GetComponent<MultiOptions>().slot = slot;
    }

    //destroy options menu w/ a delay:
    public void DestroyMultiOptions()
    {
        //destroy option menu if another one present:
        if (optionsCopy != null)
        {
            Destroy(optionsCopy, 0.2f); //delay destroy to allow buttons to be pressed
        }
    }

    #endregion

    #region Inventory Action Methods

    private void CloseInventory()
    {
        //close inventory:
        inventoryPanel.SetActive(false);

        //destroy any remaining multi option menus:
        DestroyMultiOptions();

        //re-enable player control:
        playerManager.EnablePlayerControl();

        //other UI now accessible:
        escape_UI_Accessible = true;
    }

    private void OpenInventory()
    {
        //open inventory:
        inventoryPanel.SetActive(true);

        //disable player control:
        playerManager.DisablePlayerControl();

        //other UI is not accessible:
        escape_UI_Accessible = false;
    }

    #endregion

    #region Escape Menu Action Methods

    private void OpenEscapeMenu()
    {
        //set escape panel active:
        escapePanel.SetActive(true);

        playerManager.DisablePlayerControl();

        //other UI not accessible:
        inventory_UI_Accessible = false;
    }

    //called by 'EscapePanel' 'Resume' button:
    public void CloseEscapeMenu()
    {
        escapePanel.SetActive(false);

        playerManager.EnablePlayerControl();

        inventory_UI_Accessible = true;
    }

    #endregion

    /*
    //pauses player controls to access menu:
    public void PauseUIInvoked()
    {
        if (playerManager.uiOpenedOrClosedCallback != null ) //if has any methods subscribed to it
        {
            playerManager.uiOpenedOrClosedCallback.Invoke(); //executes all methods subscribed to this callback by invoking it
        }
    }
    */
}

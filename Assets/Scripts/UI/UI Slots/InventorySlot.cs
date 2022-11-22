using UnityEngine;
using UnityEngine.UI; //needed for access to image comp on this slot
using TMPro;
using Photon.Pun;

/// <summary>
/// Slot variant meant for inventory slots 
/// </summary>
public class InventorySlot : Slot
{
    #region Vars

    //specify slot carry size:
    public CarrySize carrySize;

    public Item item { private set; get; } //keeps track of curr item in slot 

    public PausedUI pausedUI; //needed to create options

    public TMP_Text descriptionText;

    #endregion Vars

    public override void Start()
    {
        base.Start();
    }

    #region Addition Methods

    public void AddItemToSlot(Item newItem)
    {
        //set item:
        item = newItem;

        //update our icon:
        icon.sprite = item.icon; //set item icon to inventory icon

        //update icon's enabled + whether it's currently interactive:
        base.AddSomethingToSlot();
    }

    //called by multi options 'eaxmine' button:
    public void ShowItemDescription()
    {
        TMP_Text descriptionCopy;

        //get the curr middle bottom coord of player's screen: (doesn't work)
        //Vector3 middleBottomOfScreen = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height/4, 0));

        //create text obj at middle bottom of screen, and w/ the Canvas as parent so visible:
        //descriptionCopy = Instantiate(descriptionText, Input.mousePosition, Quaternion.identity, pausedUI.transform);
        descriptionCopy = Instantiate(descriptionText, pausedUI.txtSpawnPnt.position, Quaternion.identity, pausedUI.transform);

        //copy over item description to text obj:
        descriptionCopy.text = item.description;

        //activate instantiated obj:
        descriptionCopy.gameObject.SetActive(true);

        //destroy text obj in 3 seconds
        Destroy(descriptionCopy.gameObject, 3f);
    }

    #endregion Addition Methods

    #region Removal Methods

    //clear slot of item:
    public override void ClearSlot()
    {
        //clear curr item:
        item = null;

        base.ClearSlot();

        //disable showing stack amt:
        ShowStackAmt(false);
    }

    // w/ 'removeButton' clicked, remove item from this player's inventory:
    public override void OnRemoveButton() 
    {
        //remove item:
        inventoryAttachedTo.RemoveItemFromInventoryList(item);
    }

    //called by 'MultiOptions' script by 'drop' button:
    public void SlotDrop()
    {
        //drop item on ground, remove it from inventory, and clear slot

        //set player position:
        Vector3 playerPosition = pausedUI.playerManager.player.transform.position;

        //set vector as right in front of which way player facing: 
        Vector3 inFrontOfPlayer = pausedUI.playerManager.player.transform.forward.normalized; //use '.forwar' to get player's local coord syst, and normalized bc we only need direction not magnitude

        //set spawn pnt right in front of player, and up a bit bc otherwise spawns in ground:
        Vector3 spawnPnt = playerPosition + inFrontOfPlayer + Vector3.up;

        //if online:
        if (PhotonNetwork.IsConnected)
        {
            //reset item's delete field:
            //item.itemPickup.GetComponent<ItemPickup>().shouldDelete = false;

            //create for all players:
            GameObject createdObj = PhotonNetwork.Instantiate(item.itemPickup.name, spawnPnt, item.itemPickup.transform.rotation);
            //GameObject createdObj = PhotonNetwork.InstantiateRoomObject(item.itemPickup.name, spawnPnt, item.itemPickup.transform.rotation); //only works for master client

            //transfer dropped item's ownership to the scene: (scene's owner ID is 0?) (doesnt work)
            //createdObj.GetPhotonView().TransferOwnership(-1);

            //Debug.LogError("creted obj's owner is: " + createdObj.GetPhotonView().Owner);
        }
        else
        {
            //create item locally at player's feet:
            Instantiate(item.itemPickup, spawnPnt, item.itemPickup.transform.rotation);
        }

        //remove it from inventory (clears slot for us):
        inventoryAttachedTo.RemoveItemFromInventoryList(item);

    }

    #endregion Removal Methods

    //instantiate dropdown at mouse position w/ item selected:
    public void ItemSelected()
    {
        //create dropdown if item in slot:
        if (item != null)
        {
            pausedUI.MakeMultiOptions(this);
        }
    }

    //called by 'MultiOptions' scipt 'Use' button
    public void UseItem()
    {
        //use item within curr player's inventory:
        if (item != null) //if have an item
        {
            item.Use(inventoryAttachedTo);
        }
    }
}

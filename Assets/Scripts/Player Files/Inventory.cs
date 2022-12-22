using Photon.Pun;
using System.Collections;
using System.Collections.Generic; //needed for 'list' type
using UnityEngine;

public class Inventory : MonoBehaviour
{
    #region Vars

    public delegate void OnItemChanged(); //def delegate 'type'
    public OnItemChanged onItemChangedCallback; //implement delegate w/ a var (triggered everytime anything changes in our inventory) 

    //event to update amt UI, invoked w/ ever item amt changes: 
    public delegate void OnStackAmtChanged(Item item, Equipment equip, int newStackAmt);     //either item or equip must be null
    public OnStackAmtChanged OnStackAmtChangedCallback;

    //creates new lists of items that'll hold all items in our inventory:
    public List<Item> bagList = new List<Item>();
    public List<Item> tinyList = new List<Item>();

    //max slots: ****init in inspector****
    public int maxBagSlots = 2;
    public int maxTinySlots = 1;

    //public bool destroyed { get; private set; }

    //DropItemsOnDestroy[] dropItemsOnDestroys;

    #endregion Vars

    /*
    private void Start()
    {
        InvokeRepeating("StoreDropItems", 1, 5);
    }
    */


    #region Add to Stack

    private bool TryAddTinyStack(Item item)
    {
        //try to update tiny stack UI:
        //if there's a tiny item w/ the same 'name' and a non-full stack, add its stack to the tiny item's item amt and don't add it as a new inventory item: 
        foreach (Item tinyItem in tinyList)
        {
            //if there's an item w/ same name already in inventory, and max not reached yet:
            if (tinyItem.name == item.name && tinyItem.itemAmount < tinyItem.maxStack)
            {
                //add item's 'itemAmt' to the curr item amt of the inventory:
                tinyItem.itemAmount += item.itemAmount;                    //adds a dif amt than 1 if picking up a bundle of something

                //update item's text UI amt:
                if (OnStackAmtChangedCallback != null)
                {
                    OnStackAmtChangedCallback.Invoke(tinyItem, null, tinyItem.itemAmount);
                }

                print(item.name + "'s curr stack is: " + tinyItem.itemAmount);

                return true;
            }
        }

        return false;
    }

    private bool TryAddMedStack(Item item)
    {
        //try and update medium stack UI:
        //if there's a medium item w/ the same 'name' and a non-full stack, add its stack to the med item's item amt and don't add it as a new inventory item: 
        foreach (Item medItem in bagList)
        {
            if (medItem.name == item.name && medItem.itemAmount < medItem.maxStack)
            {
                //add item's 'itemAmt' to the curr item amt of the inventory:
                medItem.itemAmount += item.itemAmount;                    //adds a dif amt than 1 if picking up a bundle of something

                //update item's text UI amt:
                if (OnStackAmtChangedCallback != null)
                {
                    OnStackAmtChangedCallback.Invoke(medItem, null, medItem.itemAmount);
                }

                print(item.name + "'s curr stack is: " + medItem.itemAmount);

                return true;
            }
        }

        return false;
    }

    #endregion Add to Stack

    #region List Methods

    //add item to inventory list by verifying that it's not a default item and there's enough room:
    public bool AddItemToInventoryList(Item item)
    {
        //don't add item to inventory list w/ it's a default item:
        if (item.isDefaultItem)
        {
            Debug.Log("Item not added to inventory list bc it's a default item");

            return false;
        }
        //try and add non-default item to list:
        else
        {
            // make a copy of the item (do at top of else so don't return w/ inventory maxed but stack not):
            Item itemCopy = Instantiate(item);                      //need to 'instantiate' it so we make a copy of the current item and don't change the item's default 'itemAmt'

            //itemsList.Contains(item) //cant use this bc searches list for the og item, but we added a copy of the og item below w/ instantiating (each copy is dif)

            //try to add item to tiny stack:
            if (TryAddTinyStack(item))
            {
                return true;

            }
            //if fails, try and add item to medium stack:  (both could fail if item not stackable, then countinue down function)
            else if (TryAddMedStack(item))
            {
                return true;
            }

            //return false if no room to add item based off of it's carry size:
            if ((item.carrySize == CarrySize.Tiny && tinyList.Count >= maxTinySlots)
                || (item.carrySize == CarrySize.Medium && bagList.Count >= maxBagSlots))
            {
                Debug.Log("<color=red> Not enough room. </color>");

                return false;
            }

            //add item to specified list based off carry size:
            if (item.carrySize == CarrySize.Tiny)
            {
                tinyList.Add(itemCopy);
            }
            else
            {
                bagList.Add(itemCopy);
            }

            //invoke item changed callback:
            if (onItemChangedCallback != null) //if has any methods subscribed to it
            {
                onItemChangedCallback.Invoke(); //executes all methods subscribed to this callback by invoking it
            }

            //update stack txt amt w/ picked up item is bigger than a stack of 1:
            if (itemCopy.itemAmount > 1)
            {
                //update the stack txt amt:
                if (OnStackAmtChangedCallback != null)
                {
                    OnStackAmtChangedCallback.Invoke(itemCopy, null, itemCopy.itemAmount);
                }
            }

            return true;
        }
    }

    //remove item from inventory list:
    public void RemoveItemFromInventoryList(Item item)
    {

        //remove item to specified list based off carry size:
        if (item.carrySize == CarrySize.Tiny)
        {
            tinyList.Remove(item);
        }
        else
        {
            bagList.Remove(item);
        }

        //invoke item changed callback (what methods r subbed to this?):
        if (onItemChangedCallback != null) //if has any methods subscribed to it
        {
            onItemChangedCallback.Invoke(); //executes all methods subscribed to this callback by invoking it
        }
    }

    #endregion List Methods

    #region Drop Methods

    //drops item over photon network
    private void DropInventoryItem(GameObject spawnObj, Vector3 spawnPnt)
    {

        //if photon network connected:
        if (PhotonNetwork.IsConnected)
        {
            /*
            //create only 1 obj regardless of player count:
            PhotonNetwork.InstantiateRoomObject(numberModels[digit].name,
                spawnPoint,
                numberModels[digit].transform.rotation);
            */

            //create an obj for every new player joining, when they load in:
            PhotonNetwork.Instantiate(spawnObj.name, spawnPnt, spawnObj.transform.rotation);

            Debug.Log("Should have instantiated " + spawnObj.name);

        }
        //local gameplay
        else
        {
            //locally create item
            Instantiate(spawnObj, spawnPnt, spawnObj.transform.rotation);
        }
    }

    /// <summary>
    /// Drop all items in invetory, and remove them from the inventory list.
    /// </summary>
    public void DropInventory()
    {
        //set vector as right in front of which way player facing: 
        Vector3 inFrontOfPlayer = transform.forward.normalized; //use '.forwar' to get player's local coord syst, and normalized bc we only need direction not magnitude

        //set spawn pnt right in front of player, and up a bit bc otherwise spawns in ground:
        Vector3 spawnPnt = transform.position + inFrontOfPlayer + Vector3.up;

        //if bag not empty
        if (bagList.Count != 0)
        {
            // for each item
            //foreach (Item item in bagList)
            for(int i = 0; i < bagList.Count; i++)
            {
                Item item = bagList[i];

                Debug.LogWarning(item.name + " lost, so recreated.");

                //recreate item
                DropInventoryItem(item.itemPickup, spawnPnt);

                //rm from our list
                RemoveItemFromInventoryList(item);
            }
        }

        //if bag not empty
        if (tinyList.Count != 0)
        {
            // for each item
            //foreach (Item item in tinyList)
            for(int i = 0; i < tinyList.Count; i++)
            {
                Item item = tinyList[i];

                Debug.LogWarning(item.name + " lost.");

                //recreate item
                DropInventoryItem(item.itemPickup, spawnPnt);

                //rm from our list
                RemoveItemFromInventoryList(item);
            }
        }
    }



    #endregion Drop Methods

    //drop all items in inventory on ground
    private void OnDestroy()
    {
       // destroyed = true;
        /*
        //store items going to be destroyed
        foreach (DropItemsOnDestroy dropItemScript in dropItemsOnDestroys)
        {
            dropItemScript.GiveDestroyedItems(bagItems: bagList, tinyItems: tinyList);
        }

        //debug: Debug.Log("Destroy " + this.name);

        //fails: Debug.Log("Other inventory: " + FindObjectOfType<Inventory>().name);
        */
    }
}

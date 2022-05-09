using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItemsOnDestroy : MonoBehaviour
{
    private Inventory[] inventories;
    private EquipmentManager[] equipManagers;

    public bool destroyed { get; private set; }

    //curr items to restore
    List<Item> resBagItems; 
    List<Item> resTinyItems;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("StorePlayers", 1, 5);
    }

    void StorePlayers()
    {
        //store all player manager's
        inventories = FindObjectsOfType<Inventory>();

        //store all equip manager's
        equipManagers = FindObjectsOfType<EquipmentManager>();
    }

    //would have to turn adding item to inventory and
    // equipment manager into RPC to work
    // bc lists filled, but our version of other players
    // dont have items in inventory or equip managers
    // would have to make item removal rpc too
    // (not worth it)
    void Update()
    {
        //if there are inventories
        if( inventories != null && inventories.Length != 0)
        {
            //loop thru each one
            foreach (Inventory inventory in inventories)
            {
                //if inventory destroy flag raised
                /*
                if (inventory.destroyed)
                {
                    Debug.LogWarning("inventory destroyed");

                    //resBagItems
                }
                */
            }
        }

        //if items to restore
        if(resBagItems != null && resTinyItems != null)
        {
            Debug.LogWarning("Need to restore inventory items");

            //if bag not empty
            if (resBagItems.Count != 0)
            {
                // for each item
                foreach (Item item in resBagItems)
                {
                    Debug.LogWarning(item.name + " lost, so recreated it.");

                    //recreate over photon network
                    // could call to another PlayerManager to do so
                    DropOnDestroy(item.itemPickup);
                }
            }

            //if bag not empty
            if (resTinyItems.Count != 0)
            {
                // for each item
                foreach (Item item in resTinyItems)
                {
                    Debug.LogWarning(item.name + " lost.");

                    //recreate over photon network
                    // could call to another PlayerManager to do so
                    DropOnDestroy(item.itemPickup);
                }
            }

            //empty items
            resBagItems = null;
            resTinyItems = null;
        }

    }

    public void GiveDestroyedItems( List<Item> bagItems, List<Item> tinyItems )
    {
        Debug.Log("Items stored");

        resBagItems = bagItems;
        resTinyItems = tinyItems;
    }

    //drops item over photon network
    private void DropOnDestroy(GameObject spawnObj)
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
            PhotonNetwork.Instantiate(spawnObj.name, Vector3.up, spawnObj.transform.rotation);

            Debug.Log("Should have instantiated " + spawnObj.name);
        }
    }
}

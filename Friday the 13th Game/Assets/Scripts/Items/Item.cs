using UnityEngine;

//makes rlly easy to create new items/instances of this class:
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")] //when created from asset folder, auto adds item w/ filename 'New Item' from the 'Inventory' tab as the 'Item' option

//blueprint for all our 'scriptable' objs:
public class Item : ScriptableObject
{
    //all items need the properties of: a name, icon
    new public string name = "New Item"; //'new' makes this variable overwrite the old value of 'name' for this game obj

    public string description = "Coming Soon"; //examine button should call this description to be shown on screen

    public CarrySize carrySize;

    public int maxStack = 1; //max size of this item stack

    public int itemAmount = 1; //curr size of item stack

    public GameObject itemPickup; //world space item pickup

    public Sprite icon = null; // picture representation of item

    public bool isDefaultItem = false; //bc when replace default equipment don't want it cluttering up our inventory? 

    //use item in player's inventory:
    public virtual void Use(Inventory playerInventory) //use the item, planned to be overwritten by child classes
    {

        Debug.Log("Using " + name);
    }

    //remove this item from inventory: 
    public void RemoveFromInventory(Inventory playerInventory)
    {
        playerInventory.RemoveItemFromInventoryList(this);
    }
}

public enum CarrySize { Tiny, Medium}

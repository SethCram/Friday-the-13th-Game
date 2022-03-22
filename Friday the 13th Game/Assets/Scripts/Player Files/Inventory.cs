using System.Collections;
using System.Collections.Generic; //needed for 'list' type
using UnityEngine;

public class Inventory : MonoBehaviour
{

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

    //add item to inventory list by verifying that it's not a default item and there's enough room:
    public bool AddItemToInventoryList(Item item)
    {
        //don't add item to inventory list w/ it's a default item:
        if(item.isDefaultItem)
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
            if(item.carrySize == CarrySize.Tiny)
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

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/Consumable")]
public class Consumable : Item
{
    //will heal either hunger, thirst, or hp:
    public int healAmt;

    //specifies what we're healing:
    public ConsumableType consumableType;

    public override void Use(Inventory playerInventory)
    {
        base.Use(playerInventory);

        RemoveFromInventory(playerInventory);
    }
}

public enum ConsumableType { food, water, potion }

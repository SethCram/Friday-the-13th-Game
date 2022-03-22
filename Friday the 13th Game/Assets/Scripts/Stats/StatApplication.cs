using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatApplication : MonoBehaviour
{
    //event setup:
    public delegate void OnStatChanged(Stat changedStat);
    public OnStatChanged onStatChangedCallback;

    //fill in inspector:
    public PlayerStats playerStats; //could also use 'CharStats' but that doesnt add mods?
    public ThirdPersonMovement movement; //to change run speed and jump height

    //max of every stat:
    public int statMax = 10;

    //for calcing running speed:
    private float runLowerLimit = 7;
    private float runUpperLimit = 12;
    private float runDifference;

    //for calcing jump height:
    private float jmpLowerLimit = 1;
    private float jmpUpperLimit = 3;
    private float jmpDifference;

    //for calcing player health:
    public int hp_per_bulk = 5;       //bc each bulk pnt worth a specified number of hp
    private int bulkDifference; 
    private int lastSetBulkStat = 0;

    // Start is called before the first frame update
    void Start()
    {
        //calc run and jmp range tween upper and lower limit:
        runDifference = runUpperLimit - runLowerLimit;
        jmpDifference = jmpUpperLimit - jmpLowerLimit;

        //sub method for w/ stat changes:
        onStatChangedCallback += ApplyChangedStat;
    }

    //called everytime a stat is changed:
    private void ApplyChangedStat(Stat statChanged)
    {
        int statVal = statChanged.GetValue();

        switch (statChanged.name)
        {
            case "Melee":
                break;

            case "Ranged":
                break;

            case "Running":

                //find additional run speed to add:
                float addedRunSpeed = (statVal / (float)statMax) * runDifference;   //dont do float arith w/ an int or it'll approx

                //set additional run speed:
                movement.runSpeed = runLowerLimit + addedRunSpeed;

                break;

            case "Climbing":

                //find additional jmp height to add:
                float addedJumpHeight = (statVal / (float)statMax) * jmpDifference;   //dont do float arith w/ an int or it'll approx

                //set additional jmp height:
                movement.jumpHeight = jmpLowerLimit + addedJumpHeight;

                break;

            case "Swimming":
                break;

            case "Medical":
                break;

            case "Perception":
                break;

            case "Paranoia":
                break;

            case "Communication":
                break;

            case "Bulk":

                //max hp set to base hp w/ added amt calced from bulk stat:
                playerStats.maxHealth = playerStats.baseHealth + (hp_per_bulk * statVal);

                //calc bulk dif ((+) if increased, (-) if decreased):
                bulkDifference = statVal - lastSetBulkStat;

                //set curr hp to itself + or - the difference;
                playerStats.currHealth += hp_per_bulk * bulkDifference;

                //set what the prev bulk stat is for w/ we call this method again:
                lastSetBulkStat = statVal;

                break;

            case "Stealth":
                break;

            case "Agility":
                break;

            case "Unarmed":
                //dont need section
                break;

            case "Defense":
                //dont need section
                break;

            default:
                Debug.LogError("Can't apply stat that was changed bc cant find it");
                break;

        }
    }
}

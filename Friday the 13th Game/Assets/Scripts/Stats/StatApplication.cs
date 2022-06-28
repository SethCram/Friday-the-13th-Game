using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatApplication : MonoBehaviour
{
    #region Vars

    //event setup:
    public delegate void OnStatChanged(Stat changedStat);
    public OnStatChanged onStatChangedCallback;

    //fill in inspector:
    public PlayerStats playerStats; //could also use 'CharStats' but that doesnt add mods?
    public ThirdPersonMovement movement; //to change run speed and jump height
    public PlayerManager playerManager;

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

    //for dif minimap usage reqs
    public int minIconMinimap = 4;
    public int minRealMinimap = 7;
    public Cam_Instantiator cam_Instantiator;

    //for hp bar usage
    public int minHealthBar = 5;
    public OverlayUI overlayUI;

    #endregion

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

                //if perception too low for either minimap
                if (statVal < minIconMinimap && statVal < minRealMinimap)
                {
                    //deactivate minimap outline
                    playerManager.minimapUI.SetActive(false);

                    break;
                }
                else
                {
                    //make sure minimap outline active
                    playerManager.minimapUI.SetActive(true);
                }

                //check if real minimap reqs more points than icon minimap 
                if ( minRealMinimap < minIconMinimap )
                {
                    Debug.LogWarning("real minimap threshold shouldn't be less than the icon minimap threshold?");
                }

                //if icon minimap reqs more points
                if(minRealMinimap < minIconMinimap)
                {
                    //if perception high enough for icon minimap
                    if (statVal >= minIconMinimap)
                    {
                        cam_Instantiator.SpawnIconMinimap();
                    }
                    else if (statVal >= minRealMinimap)
                    {
                        cam_Instantiator.SpawnRealMinimap();
                    }

                }
                //real minimap reqs more or equal points
                else
                {
                    //if perception high enough for real minimap
                    if (statVal >= minRealMinimap)
                    {
                        cam_Instantiator.SpawnRealMinimap();
                    }
                    else if (statVal >= minIconMinimap)
                    {
                        cam_Instantiator.SpawnIconMinimap();
                    }
                }

                break;

            case "Paranoia":
                
                //if have enough points for health bar
                if( statVal >= minHealthBar)
                {
                    //activate it
                    overlayUI.healthSlider.gameObject.SetActive(true);
                }
                else
                {
                    //deactivate it
                    overlayUI.healthSlider.gameObject.SetActive(false);
                }
                
                break;

            case "Communication":
                break;

            case "Bulk":

                //max hp set to base hp w/ added amt calced from bulk stat:
                playerStats.maxHealth = playerStats.baseHealth + (hp_per_bulk * statVal);

                //calc bulk dif ((+) if increased, (-) if decreased):
                bulkDifference = statVal - lastSetBulkStat;

                //set curr hp to itself + or - the difference;
                // only need to do this when player first spawns in
                // otherwise, player's hp shouldnt change based on armor?
                playerStats.currHealth += hp_per_bulk * bulkDifference;

                //set what the prev bulk stat is for w/ we call this method again:
                lastSetBulkStat = statVal;

                //explicitly spawn numbers when bulk changes
                GetComponent<CharacterStats>().SpawnNumbers(playerStats.maxHealth, playerStats.currHealth);
                
                // GetComponent<CharacterStats>().OnHealthChangedCallback(playerStats.maxHealth, playerStats.currHealth);

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

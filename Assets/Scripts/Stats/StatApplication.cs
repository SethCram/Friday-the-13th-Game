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
    private float runLowerLimit = 3; //7
    private float runUpperLimit = 3.5f; //12
    private float runDifference;

    //for calcing walking speed
    private float walkLowerLimit = 2f;
    private float walkUpperLimit = 2.5f;
    private float walkDifference;
    
    //for calcing crouching speed
    private float crouchSpeedLowerLimit = 1;
    private float crouchSpeedUpperLimit = 1.5f;
    private float crouchSpeedDifference;

    //for calcing jump height:
    private float jmpLowerLimit = 0.8f; //1
    private float jmpUpperLimit = 1.5f; //3
    private float jmpDifference;

    //stamina regen calc
    private float regenStaminaDelayLowerLimit = 1.5f;
    private float regenStaminaDelayUpperLimit = 2.5f;
    private float regenStaminaDelayDifference;
    private float regenStaminaTickLowerLimit = 0.15f;
    private float regenStaminaTickUpperLimit = 0.25f;
    private float regenStaminaTickDifference;

    //for calcing player health:
    public int hp_per_bulk = 5;       //bc each bulk pnt worth a specified number of hp
    private int bulkDifference;

    public int stamina_per_point = 5;

    //for dif minimap usage reqs
    public int minIconMinimap; //4
    public int minRealMinimap; //7
    public Cam_Instantiator cam_Instantiator;

    //for hp bar usage
    public int minHealthBar; //5

    //for stamina bar usage
    public int minStaminaBar; //6

    public float boundModifier = 1;

    private float minSlopeLimit = 30;
    private float maxSlopeLimit = 50;

    [HideInInspector]
    public OverlayUI overlayUI;

    [HideInInspector]
    CharacterStats charStats;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        charStats = GetComponent<CharacterStats>();

        //make sure diffs init'd
        SetBoundModifier(boundModifier);

        //init movement vars 
        movement.walkSpeed = walkLowerLimit;
        movement.runSpeed = runLowerLimit;
        movement.crouchSpeed = crouchSpeedLowerLimit;

        //init player regen stamina vars
        playerStats.regenStaminaDelay = new WaitForSeconds(regenStaminaDelayUpperLimit);
        playerStats.regenStaminaTick = new WaitForSeconds(regenStaminaTickUpperLimit);

        //sub method for w/ stat changes:
        onStatChangedCallback += ApplyChangedStat;

        //randomly init min for UI
        minHealthBar = Random.Range(1, 9);
        minStaminaBar = Random.Range(1, 9);
        minIconMinimap = Random.Range(1, 4);
        minRealMinimap = Random.Range(6, 9);

        print($"min hp bar = {minHealthBar} (Paranoia)");
        print($"min stamina bar = {minStaminaBar} (Paranoia)");
        print($"min icon minimap = {minIconMinimap} (Perception)");
        print($"min real minimap = {minRealMinimap} (Perception)");
    }

    /// <summary>
    /// Sets bound modifier but also calcs difference tween bounds
    /// </summary>
    /// <param name="newBoundModifier"></param>
    public void SetBoundModifier(float newBoundModifier)
    {
        boundModifier = newBoundModifier;

        //change all bounds by the bounds modifier
        runLowerLimit *= boundModifier;
        runUpperLimit *= boundModifier;
        walkLowerLimit *= boundModifier;
        walkUpperLimit *= boundModifier;
        crouchSpeedLowerLimit *= boundModifier;
        crouchSpeedUpperLimit *= boundModifier;
        jmpLowerLimit *= boundModifier;
        jmpUpperLimit *= boundModifier;
        regenStaminaDelayLowerLimit /= boundModifier;
        regenStaminaDelayUpperLimit /= boundModifier;
        regenStaminaTickLowerLimit /= boundModifier;
        regenStaminaDelayUpperLimit /= boundModifier;
    }

    private float CalculateAdditionalAmount(float statVal, float lowerLimit, float upperLimit)
    {
        //find additional amt to add
        return (statVal / (float)statMax) * (upperLimit - lowerLimit);
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

            case "Speed":

                //find additional run/walk/crouch speed to add:
                float addedRunSpeed = CalculateAdditionalAmount(statVal, runLowerLimit, runUpperLimit);   //dont do float arith w/ an int or it'll approx
                float addedWalkSpeed = CalculateAdditionalAmount(statVal, walkLowerLimit, walkUpperLimit);
                float addedCrouchSpeed = CalculateAdditionalAmount(statVal, crouchSpeedLowerLimit, crouchSpeedUpperLimit);

                //set additional run/walk/crouch speed:
                movement.runSpeed = runLowerLimit + addedRunSpeed;
                movement.walkSpeed = walkLowerLimit + addedWalkSpeed;
                movement.crouchSpeed = crouchSpeedLowerLimit + addedCrouchSpeed;

                break;

            case "Climbing":

                //find additional jmp height to add:
                float addedJumpHeight = CalculateAdditionalAmount(statVal, jmpLowerLimit, jmpUpperLimit);   //dont do float arith w/ an int or it'll approx
                //set additional jmp height:
                movement.jumpHeight = jmpLowerLimit + addedJumpHeight;

                movement.groundedSlopeLimit = minSlopeLimit + CalculateAdditionalAmount(statVal, minSlopeLimit, maxSlopeLimit);

                break;

            case "Swimming":
                break;

            case "Medical":
                break;

            case "Perception":

                //Debug.Log($"Preception is {statVal}");

                //if perception too low for either minimap
                if (statVal < minIconMinimap && statVal < minRealMinimap)
                {
                    //deactivate minimap outline
                    playerManager.minimapUI.SetActive(false);

                    break;
                }
                //if perception high enough for a minimap
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

                //if have enough points for stamina bar
                if( statVal >= minStaminaBar)
                {
                    //activate it
                    overlayUI.staminaSlider.gameObject.SetActive(true);
                }
                else
                {
                    //deactivate it
                    overlayUI.staminaSlider.gameObject.SetActive(false);
                }
                
                break;

            case "Communication":
                break;

            case "Bulk":

                //max hp set to base hp w/ added amt calced from bulk stat:
                playerStats.maxHealth = playerStats.baseHealth + (hp_per_bulk * statVal);

                //tell char stats that bulk was changed
                charStats.InvokeCallback_OnHealthChangedCallback();

                break;

            case "Stealth":
                break;

            case "Endurance":

                playerStats.maxStamina = playerStats.baseStamina + (stamina_per_point * statVal);

                //tell char stats that stamina was changed
                charStats.InvokeCallback_OnStaminaChangedCallback();

                break;
            
            case "Dexterity":
                //vary stamina restore speed and delay by new dexterity stat

                float subtractedRegenStaminaDelay = CalculateAdditionalAmount(statVal, regenStaminaDelayLowerLimit, regenStaminaDelayUpperLimit);
                float subtractedRegenStaminaTick = CalculateAdditionalAmount(statVal, regenStaminaTickLowerLimit, regenStaminaTickUpperLimit);

                //print($"regen stamina tick = {regenStaminaTickUpperLimit - subtractedRegenStaminaTick}, regen stamina delay = {regenStaminaDelayUpperLimit - subtractedRegenStaminaDelay}");
                
                playerStats.regenStaminaDelay = new WaitForSeconds(regenStaminaDelayUpperLimit - subtractedRegenStaminaDelay);
                playerStats.regenStaminaTick = new WaitForSeconds(regenStaminaTickUpperLimit - subtractedRegenStaminaTick);

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

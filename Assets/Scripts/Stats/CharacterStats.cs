using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class CharacterStats : MonoBehaviourPun
{
    #region Vars

    //for making this char animate taking dmg:
    public CharacterAnimator charAnimator;  //******init in inspector**********

    public int maxHealth = 50; //not a stat bc don't want it affected by modifiers
    public int currHealth; //{ get; private set; } (needs to be settable from 'StatApplications' for bulk stat //can only set thru this class, but can be retrieved by any class 
    public int baseHealth = 50; //never changes for this char

    public int maxStamina = 50; //not a stat bc don't want it affected by modifiers
    public int currStamina; 
    public int baseStamina = 50; //never changes for this char

    public float staminaRegenRate = 0.5f;

    //public float deathAnimDelay = 2f; //time it takes for death anim to play out

    //all character stats filled from inspector here:
    public Stat[] allStats; //for setting stat names on character creation screen, and stat vals from it

    //event for w/ health changes (max and curr health needed):
    public event System.Action<int, int> OnHealthChangedCallback; //example of an 'event' that takes multiple args, should update healthUI w/ ever called

    //event for w/ stamina changes
    public event System.Action<int, int> OnStaminaChangedCallback;

    //declare a dict of index type string and return type Stat:
    public Dictionary<string, Stat> statDict;

    //number related
    public GameObject[] numberModels;
    public float numSpawnHeight = 2;
    public float numSpacing = 5; //2;

    //inited beforehand:
    public PlayerManager playerManager;

    //private int prevHP;
    [HideInInspector]
    public OverlayUI overlayUI;

    //event for w/ player death:
    public System.Action OnDeathCallback;

    public WaitForSeconds regenStaminaTick = new WaitForSeconds(0.2f);
    public float regenStaminaDelay = 2;

    public Coroutine degenStaminaCoroutineInstance;
    public Coroutine regenStaminaCoroutineInstance;
    public WaitForSeconds degenStaminaTick = new WaitForSeconds(0.2f);

    #endregion

    #region Unity Methods

    //need filling of 'allStats' array here bc it's used in another script's start
    private void Awake()
    {

        //init and popuate dict:   (now stat should be searchable thru its name as index)
        statDict = new Dictionary<string, Stat>();
        foreach (Stat stat in allStats)
        {
            statDict.Add(stat.name, stat);

            //print each stats' name to check:
               //print(statDict[stat.name].name);
        }

        SetHealthToMax();

        //subscribe number method
        OnHealthChangedCallback += SpawnNumbers;

        //update health slider using HP changed event
        OnHealthChangedCallback += CallHealthSlider;

        //update stamina slider using stamina changed event
        OnStaminaChangedCallback += CallStaminaSlider;

        //sub methods to death callback

        OnDeathCallback += AnimateAndSetDeath;

        //drop all player loot now that dead:
        OnDeathCallback += playerManager.DropEverything;

        OnDeathCallback += PlayDeathNoise;

        OnDeathCallback += DisableInteractability;
    }

    private void Update()
    {
        //if not my photon view + we're on the network
        if(!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            //dont deal dmg or die
            return;
        }

        //test keys
        if (Input.GetKeyDown(KeyCode.T))
        {
            //test taking dmg
            TakeHit(5);
        }
        if(Input.GetKeyDown(KeyCode.K))
        {
            //kill player

            //if any methods sub'd to death callback
            if (OnDeathCallback != null)
            {
                //invoke the sub'd methods
                OnDeathCallback();
            }
        }
        
    }

    /// <summary>
    /// Frame independant stamina restore
    /// </summary>
    private void FixedUpdate()
    {
        
    }

    #endregion

    #region Health Methods

    //call overlay UI health slider to update it
    private void CallHealthSlider(int maxHP, int currHP) //was previously public for some reason?
    {
        //if local play or my player
        if(!PhotonNetwork.IsConnected || photonView.IsMine)
        {
            //update hp slider
            overlayUI.UpdateHealthSlider(maxHP, currHP);
        }

    }

    /// <summary>
    /// Invokes the OnHealthChanged callback using CharacterStats.cs private health vars if any methods sub'd.
    /// </summary>
    public void InvokeCallback_OnHealthChangedCallback()
    {
        //health changed event:
        if (OnHealthChangedCallback != null)
        {
            OnHealthChangedCallback(maxHealth, currHealth);
        }
    }

    /// <summary>
    /// sets current HP to max HP
    /// </summary>
    public void SetHealthToMax()
    {
        currHealth = maxHealth;
    }

    #endregion Health Methods

    #region Stamina Methods

    /// <summary>
    /// Try to use passed in amt of stamina and returns whether successful or not and start regen.
    /// </summary>
    /// <param name="staminaAmt"></param>
    /// <returns></returns>
    public bool UseStamina(int staminaAmt)
    {
        //if enough stamina left, rm it
        int newCurrStamina = currStamina - staminaAmt;
        if( newCurrStamina >= 0)
        {
            currStamina = newCurrStamina;

            InvokeCallback_OnStaminaChangedCallback();

            //if couldn't stop stamina degen bc none occuring, start stamina regen
            if(!StopStaminaDegen())
            {
                StartStaminaRegen();
            }

            return true;
        }

        //if not enough stamina left, dont use any
        return false;
    }

    /// <summary>
    /// After regenStaminaDelay, start regening stamina per regenStaminaTick
    /// </summary>
    /// <returns></returns>
    private IEnumerator RegenStaminaCoroutine()
    {
        //do initial dely of regening
        yield return new WaitForSeconds(regenStaminaDelay);

        //while curr stamina in bounds
        while(currStamina < maxStamina)
        {

            currStamina += 1; //maxStamina / 100;

            //update UI
            InvokeCallback_OnStaminaChangedCallback();

            //wait a little
            yield return regenStaminaTick;
        }

        //clear regen stamina coroutine bc no longer regening stamina
        regenStaminaCoroutineInstance = null;

    }

    private IEnumerator DegenStaminaCoroutine(float delay = 0)
    {
        yield return new WaitForSeconds(delay);

        //while curr stamina in bounds
        while(currStamina > 0)
        {
            //decr stamina
            currStamina -= 1;

            //update UI
            InvokeCallback_OnStaminaChangedCallback();

            //wait a little
            yield return degenStaminaTick;
        }

        //clear stamina coroutine bc no longer degening stamina
        degenStaminaCoroutineInstance = null;
    }

    /// <summary>
    /// Return whether stamina degen able to start thru stopping any existing stamina changing + starting degen.
    /// </summary>
    /// <returns></returns>
    public bool StartStaminaDegen(float delay = 0)
    {
        if( currStamina >= 0)
        {
            //if already changing stamina, stop
            if( degenStaminaCoroutineInstance != null)
            {
                StopCoroutine(degenStaminaCoroutineInstance);
            }
            if( regenStaminaCoroutineInstance != null)
            {
                StopCoroutine(regenStaminaCoroutineInstance);
                regenStaminaCoroutineInstance = null;
            }

            //start degening stamina w/ optional delay
            degenStaminaCoroutineInstance = StartCoroutine(DegenStaminaCoroutine(delay: delay));

            return true;
        }

        //if not enough stamina left, dont use any
        return false;
    }

    /// <summary>
    /// Stops stamina degen if possible and returns result.
    /// </summary>
    /// <returns></returns>
    public bool StopStaminaDegen()
    {
        //if already changing stamina, stop
        if( degenStaminaCoroutineInstance != null)
        {
            StopCoroutine(degenStaminaCoroutineInstance);
            degenStaminaCoroutineInstance = null;

            return true;
        }

        return false;
    }

    /// <summary>
    /// Start regenerating stamina thru stopping any existing stamina changing + starting regen.
    /// </summary>
    public bool StartStaminaRegen()
    {
        if( maxStamina > currStamina)
        {
            //if already changing stamina, stop
            if( regenStaminaCoroutineInstance != null)
            {
                StopCoroutine(regenStaminaCoroutineInstance);
            }
            if( degenStaminaCoroutineInstance != null)
            {
                StopCoroutine(degenStaminaCoroutineInstance);
                degenStaminaCoroutineInstance = null;
            }

            //start regening stamina w/ delay
            regenStaminaCoroutineInstance = StartCoroutine(RegenStaminaCoroutine());

            return true;
        }

        return false;
    }

    //call overlay UI stamina slider to update it
    private void CallStaminaSlider(int maxStamina, int currStamina)
    {
        //if local play or my player
        if(!PhotonNetwork.IsConnected || photonView.IsMine)
        {
            //update stamina slider
            overlayUI.UpdateStaminaSlider(maxStamina, currStamina);
        }

    }

    /// <summary>
    /// Invokes the OnStaminaChanged callback using CharacterStats.cs private stamina vars if any methods sub'd.
    /// </summary>
    public void InvokeCallback_OnStaminaChangedCallback()
    {
        //health changed event:
        if (OnStaminaChangedCallback != null)
        {
            OnStaminaChangedCallback(maxStamina, currStamina);
        }
    }

    #endregion Stamina Methods

    private void DisableInteractability()
    {
        GetComponent<PlayerButtons>().interactableInaccesible = true;
    }

    #region Number Methods

    //spawn numbers above attached player  
    public void SpawnNumbers(int maxHP, int currHP)
    {
        //if networked + not my photon view
        if(PhotonNetwork.IsConnected && !photonView.IsMine)
        {
            //dont spawn numbers
            return;
        }

        Debug.Log($"<color=yellow>Numbers being spawned.</color>");

        //turn curr HP into an arr of ints
        int[] intArr = GetIntArray(currHP);
        //int[] intArr = GetIntArray(prevHP - currHP);

        //update prev HP for next spawn numbers
        //prevHP = currHP;

        //debug: Debug.Log(currHP.ToString() + " spawned" );

        float offset = 0;

        //calc offset based on player's curr local z
        Vector3 localXoffset = new Vector3(transform.localPosition.x * offset, 0, 0); //mult'd by offset before

        //set spawnpoint to above player + offset to the pos z
        Vector3 spawnPoint = transform.position + Vector3.up * numSpawnHeight + localXoffset; //+ Vector3.forward * offset;

        //for every digit in currHP
        foreach (int digit in intArr)
        {
            //if photon network connected:
            if (PhotonNetwork.IsConnected)
            {
                //debug: Debug.Log("Should have instantiated " + digit.ToString());

                //create only 1 obj regardless of player count:
                //PhotonNetwork.InstantiateRoomObject(numberModels[digit].name, 
                //    spawnPoint, 
                //    numberModels[digit].transform.rotation);

                //create an obj for every new player joining, when they load in:
                PhotonNetwork.Instantiate(numberModels[digit].name, 
                    spawnPoint,
                    Quaternion.Euler( numberModels[digit].transform.rotation.x, 
                        numberModels[digit].transform.rotation.y + transform.eulerAngles.y, 
                        numberModels[digit].transform.rotation.z ) 
                ); 
            }
            else
            {
                //create a local obj:
                Instantiate(numberModels[digit], 
                    spawnPoint,
                    numberModels[digit].transform.rotation); //use preset rot
            }

            //print(transform.rotation.ToString());

            //create new pos to spawn nxt number at
            offset += numSpacing;

            localXoffset += new Vector3( offset, 0, 0);

            //spawnPoint += Vector3.forward * offset;
            spawnPoint += localXoffset;
            //spawnPoint += new Vector3(offset)
        }
    }

    //split a number into individual digits
    int[] GetIntArray(int num)
    {
        List<int> listOfInts = new List<int>();
        while (num > 0)
        {
            listOfInts.Add(num % 10);
            num = num / 10;
        }
        listOfInts.Reverse();
        return listOfInts.ToArray();
    }

    #endregion

    #region Pain Methods


    /// <summary>
    /// play hit noise and take damage
    /// </summary>
    /// <param name="dmgDealt"></param>
    public void TakeHit(int dmgDealt)
    {
        bool notSupposedToTakeDmg;

        //set whether not supposed to take dmg based off if dead or not
        notSupposedToTakeDmg = playerManager.GetDead();

        //if not supposed to take dmg
        if( notSupposedToTakeDmg )
        {
            Debug.Log($"<color=yellow>Damage not taken because player dead.</color>");

            //dont take dmg
            return;
        }

        if (PhotonNetwork.IsConnected)
        {
            //play hurt audio
            photonView.RPC("PlayOrCreateAudioSource", RpcTarget.All, AudioManager.hurtAudioClipName);

            //tell all of these clients to take dmg
            photonView.RPC("RPC_TakeDamage", RpcTarget.Others, dmgDealt);
        }
        else
        {
            //play hurt audio
            playerManager.PlayOrCreateAudioSource(AudioManager.hurtAudioClipName);

            //take dmg locally
            RPC_TakeDamage( dmgDealt );
        }
    }

    /// <summary>
    /// Animate and apply damage. 
    /// </summary>
    /// <param name="dmgDealt"></param>
    [PunRPC]
    public void RPC_TakeDamage( int dmgDealt )
    {
        if (charAnimator != null)
        {
            //animate this char taking dmg:
            charAnimator.AnimateDmgTaken();

            //animate this char taking dmg across the network:
            //photonView.RPC("AnimateDmgTaken", RpcTarget.Others);
        }
        else
        {
            Debug.LogWarning("No CharAnim script filled for CharStats so cant animate being hit");
        }

        int damageTaken = dmgDealt;
        damageTaken = CalculateDamageUsingDefense( damageTaken );

        //makes sure damage doesn't go negative and heal:
        damageTaken = Mathf.Clamp(damageTaken, 1, int.MaxValue);

        //subtract damage amt from curr health and print it:
        currHealth -= damageTaken;
        Debug.LogWarning(transform.name + " takes " + damageTaken + " damage.");

        //health changed event:
        InvokeCallback_OnHealthChangedCallback();

        //if no more hp
        if (currHealth <= 0)
        {
            //if any methods sub'd to death callback
            if(OnDeathCallback != null)
            {
                //invoke the sub'd methods
                OnDeathCallback();
            }
            
        }
    }

    /// <summary>
    /// Calculate damage taken given the initial amt factored in with our defense.
    /// </summary>
    /// <param name="initialDamageTaken"></param>
    /// <returns></returns>
    public int CalculateDamageUsingDefense( int initialDamageTaken )
    {
        float damageFloatPH;

        damageFloatPH = initialDamageTaken;
        damageFloatPH -= damageFloatPH * (statDict["Defense"].GetValue() * 0.04f); //max damage blocked due to def should be 80%
        return (int)damageFloatPH;
    }

    /// <summary>
    /// Kill player thru animating death and setting player to dead
    /// </summary>
    public virtual void AnimateAndSetDeath()
    {
        //start death by animating death:
        if (charAnimator != null)
        {
            //anim dying over a time
            StartCoroutine( charAnimator.Die() );

            //disable player control (also unlocks cursor so bad)
            // playerManager.DisablePlayerControl();

        }
        else
        {
            Debug.LogWarning("There's no character animator to call");
        }

        Debug.Log($"<color=red>{transform.name} died. </color>");

        //delay scene reset by _ secs so player death anim can play out (done in char animator now)
        //playerManager.Invoke("ResetToMainMenu", 5); //within player manager

        //set player to dead
        playerManager.SetDead( true );

    }

    /// <summary>
    /// Play death noise using the player manager's audio src.
    /// </summary>
    private void PlayDeathNoise()
    {
        if(PhotonNetwork.IsConnected)
        {
            photonView.RPC("PlayOrCreateAudioSource", RpcTarget.All, AudioManager.dieAudioClipName);
        }
        else
        {
            playerManager.PlayOrCreateAudioSource(AudioManager.dieAudioClipName);
        }
    }

    #endregion
}

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

    //public float deathAnimDelay = 2f; //time it takes for death anim to play out

    //all character stats filled from inspector here:
    public Stat[] allStats; //for setting stat names on character creation screen, and stat vals from it

    //event for w/ health changes (max and curr health needed):
    public event System.Action<int, int> OnHealthChangedCallback; //example of an 'event' that takes multiple args, should update healthUI w/ ever called

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

        //sub methods to death callback

        OnDeathCallback += AnimateAndSetDeath;

        //drop all player loot now that dead:
        OnDeathCallback += playerManager.DropEverything;
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
            TakeDamage(5);
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

    #endregion

    /// <summary>
    /// sets current HP to max HP
    /// </summary>
    public void SetHealthToMax()
    {
        currHealth = maxHealth;
    }

    //call overlay UI health slider to update it
    public void CallHealthSlider(int maxHP, int currHP)
    {
        //if local play or my player
        if(!PhotonNetwork.IsConnected || photonView.IsMine)
        {
            //update hp slider
            overlayUI.UpdateHealthSlider(maxHP, currHP);
        }

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

        Debug.LogAssertion("Numbers being spawned.");

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


    //take damage based on enemy atk and my def:
    public void TakeDamage(int dmgDealt)
    {
        bool notSupposedToTakeDmg;

        //set whether not supposed to take dmg based off if dead or not
        notSupposedToTakeDmg = playerManager.GetDead();

        //if not supposed to take dmg
        if( notSupposedToTakeDmg )
        {
            Debug.LogAssertion("Damage not taken because " + "player dead.");

            //dont take dmg
            return;
        }

        if (PhotonNetwork.IsConnected)
        {
            //tell all of these clients to take dmg
            photonView.RPC("RPC_TakeDamage", RpcTarget.Others, dmgDealt);
        }
        else
        {
            //take dmg locally
            RPC_TakeDamage( dmgDealt );
        }
    }

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
        if (OnHealthChangedCallback != null)
        {
            OnHealthChangedCallback(maxHealth, currHealth);
        }

        //if no more hp
        if (currHealth <= 0)
        {
            //if any methods sub'd to death callback
            if(OnDeathCallback != null)
            {
                //invoke the sub'd methods
                OnDeathCallback();
            }
            
            //die locally
            AnimateAndSetDeath();
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

        Debug.Log(transform.name + "<color=red> died. </color>");

        //delay scene reset by _ secs so player death anim can play out (done in char animator now)
        //playerManager.Invoke("ResetToMainMenu", 5); //within player manager

        //set player to dead
        playerManager.SetDead( true );

    }

    #endregion
}

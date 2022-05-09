using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CharacterStats : MonoBehaviourPun
{
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
    public float numSpacing = 2;

    //inited beforehand:
    public PlayerManager playerManager;

    //awake happens before 'Start()', need filling of 'allStats' array here bc it's used in another script's start
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

        currHealth = maxHealth;

        //subscribe number method
        OnHealthChangedCallback += SpawnNumbers;

        //test out dying
        /*
        if (PhotonNetwork.IsConnected)
        {
            //tell all clients to die
            photonView.RPC("RPC_Die", RpcTarget.Other);
        }
        else
        {
            //die locally
            Invoke( "RPC_Die", 15);
        }
        */
    }

    //spawn numbers above attached player  
    public void SpawnNumbers(int maxHP, int currHP)
    {
        //turn curr HP into an arr of ints
        int[] intArr = GetIntArray(currHP);

        //debug: Debug.Log(currHP.ToString() + " spawned" );

        float offset = 0;

        //set spawnpoint to above player + offset to the pos z
        Vector3 spawnPoint = transform.position + Vector3.up * numSpawnHeight + Vector3.forward * offset;

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
                    numberModels[digit].transform.rotation);
            }
            else
            {
                //create a local obj:
                Instantiate(numberModels[digit], 
                    spawnPoint, 
                    numberModels[digit].transform.rotation);
            }

            //create new pos to spawn nxt number at
            offset += numSpacing;
            spawnPoint += Vector3.forward * offset;

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

        //public virtual void Die() 
    [PunRPC]
    public void RPC_Die()
    {
        //start death by animating death:
        if (GetComponent<CharacterAnimator>() != null)
        {
            //anim dying
            GetComponent<CharacterAnimator>().Die();

            //disable player control (also unlocks cursor so bad)
            // playerManager.DisablePlayerControl();

            //drop all player loot:
            playerManager.DropEverything();

        }
        else
        {
            Debug.LogWarning("There's no character animator to call");
        }

        Debug.Log( transform.name + "<color=red> died. </color>");

            //delay scene reset by _ secs so player death anim can play out (done in char animator now)
            //playerManager.Invoke("ResetToMainMenu", 5); //within player manager
    }
    
    //take damage based on enemy atk and my def:
    public void TakeDamage(int dmgDealt)
    {
        if(charAnimator != null)
        {
            //animate this char taking dmg:
            //charAnimator.AnimateDmgTaken();

            //animate this char taking dmg across the network:
            photonView.RPC("AnimateDmgTaken", RpcTarget.Others);
        }
        else
        {
            Debug.LogWarning("No CharAnim script filled for CharStats so cant animate being hit");
        }

        int damageTaken = dmgDealt;
        float damageFloatPH;

        //factor def into damage calculation:
        damageFloatPH = damageTaken;
        damageFloatPH -= damageFloatPH * (statDict["Defense"].GetValue() * 0.04f); //max damage blocked due to def should be 80%
        damageTaken = (int)damageFloatPH;


        //makes sure damage doesn't go negative and heal:
        damageTaken = Mathf.Clamp(damageTaken, 1, int.MaxValue);

        //subtract damage amt from curr health and print it:
        currHealth -= damageTaken;
        Debug.LogWarning(transform.name + " takes " + damageTaken + " damage.");

        //health changed event:
        if(OnHealthChangedCallback != null)
        {
            OnHealthChangedCallback(maxHealth, currHealth);
        }

        //if no more hp
        if(currHealth <= 0)
        {
            if( PhotonNetwork.IsConnected)
            {
                //tell all clients to die
                photonView.RPC("RPC_Die", RpcTarget.Others);
            }
            else
            {
                //die locally
                RPC_Die();
            }
        }
    }
    
}

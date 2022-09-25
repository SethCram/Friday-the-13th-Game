using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//create a mesh collider w/ script added:
[RequireComponent(typeof(MeshCollider))]    //*******make sure to init it as convex and a trigger*********
public class WeaponCollision : MonoBehaviour
{
    #region vars

    //fill in inspector:
    public CharacterCombat myPlayerCombat;
    public CharacterStats myCharStats;

    //private string prevDamagedObj = null; //so dont damage obj more than once every atk
    
    private int prevHitID = -1000; //view id should never be negative naturally

    #endregion

    #region Unity Methods

    // Update is called once per frame
    void Update()
    {
        //reset prev damaged obj so that can atk same target if already did so before:
        if (!myPlayerCombat.isAtking && prevHitID >= 0) //prevDamagedObj != null)
        {
            //reset dmged target:
            //prevDamagedObj = null;

            prevHitID = -1000;

            //check: Debug.LogError("Previously damaged obj reset.");
        }
    }

    //if there's a collision:
    private void OnTriggerEnter(Collider other)
    {
        //currently atking, and hit a playable char:
        if (myPlayerCombat.isAtking && 
            GameManager.Instance.TagIsPlayableCharacter(other.tag))
        {
            //cache hit obj stats:
            CharacterStats hitStats = other.GetComponent<CharacterStats>();

            //cache hit photon view id:
            int hitID = other.GetComponent<PhotonView>().ViewID;

            //dont damage myself:
            if (hitStats == myCharStats)
            {
                return;
            }

            //dont damage a previously damaged target:
            if( prevHitID == hitID )
            {
                //check: Debug.LogAssertion("dont damage a previously damaged target if already damaged them in this 1 attack");

                return;
            }

            Debug.Log("Weapon hit: " + other.name);

            //deal the dmg to passed in hit stats
            myPlayerCombat.DoDamage(hitStats);

            //play hit sound
            myPlayerCombat.PlayHitSound();

            //store previously hit person:
            prevHitID = hitID;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        print("hit " + collision.gameObject.name + " with " + gameObject.name);
    }

    #endregion
}

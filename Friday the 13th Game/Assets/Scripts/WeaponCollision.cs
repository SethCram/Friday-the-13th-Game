using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//create a mesh collider w/ script added:
[RequireComponent(typeof(MeshCollider))]    //*******make sure to init it as convex and a trigger*********

public class WeaponCollision : MonoBehaviour
{
    //fill in inspector:
    public CharacterCombat myPlayerCombat;
    public CharacterStats myCharStats;

    //private string prevDamagedObj = null; //so dont damage obj more than once every atk
    
    private int prevHitID = -1000; //view id should never be negative naturally

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
        //currently atking, and hit a player or enemy:
        if (myPlayerCombat.isAtking && (other.tag == "Player" || other.tag == "Enemy"))
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
                //if(prevDamagedObj == other.name)
            if( prevHitID == hitID )
            {
                //check: Debug.LogError("dont damage a previously damaged target if already damaged them in this 1 attack");

                return;
            }

            Debug.Log("Weapon hit: " + other.name);

            myPlayerCombat.DoDamage(hitStats);

            //store previously hit person:
                //prevDamagedObj = other.name;
            prevHitID = hitID;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        print("hit " + collision.gameObject.name + " with " + gameObject.name);
    }
}

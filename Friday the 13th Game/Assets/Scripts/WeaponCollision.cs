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

    private string prevDamagedObj = null; //so dont damage obj more than once every atk

    // Update is called once per frame
    void Update()
    {
        //reset prev damaged obj so that can atk same target if already did so before:
        if (!myPlayerCombat.isAtking && prevDamagedObj != null)
        {
            //reset dmged target:
            prevDamagedObj = null;

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

            //dont damage myself:
            if(hitStats == myCharStats)
            {
                return;
            }

            //dont damage a previously damaged target:
            if(prevDamagedObj == other.name)
            {
                //check: Debug.LogError("dont damage a previously damaged target if already damaged them in this 1 attack");

                return;
            }

            Debug.Log("Weapon hit: " + other.name);

            myPlayerCombat.DoDamage(hitStats);

            //store previously hit person:
            prevDamagedObj = other.name;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        print("Collided with" + collision.gameObject.name);
    }
}

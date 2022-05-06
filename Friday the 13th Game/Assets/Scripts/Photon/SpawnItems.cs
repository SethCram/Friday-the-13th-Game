﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnItems : MonoBehaviour
{
    public Transform spawnPnt;

    //public GameObject scimitar;

    public List<GameObject> spawnObjs;

    public float spawnRadius = 1;

    // Start is called before the first frame update
    void Start()
    {
        if(spawnObjs == null)
        {
            Debug.Log("No objects to spawn");

            return;
        }

        //spawn every obj
        foreach (GameObject spawnableObj in spawnObjs)
        {
            SpawnObjAtRandom(spawnableObj);
        }

        /*
        //if photon network connected:
        if (PhotonNetwork.IsConnected)
        {
            //create only 1 scimitar regardless of player count:
            PhotonNetwork.InstantiateRoomObject(scimitar.name, spawnPnt.position, scimitar.transform.rotation);

            //create a scimitar for every new player joining, when they load in:
            //PhotonNetwork.Instantiate(scimitar.name, spawnPnt.position, scimitar.transform.rotation);
        }
        else
        {
            //create a local scimitar:
            Instantiate(scimitar, spawnPnt.position, scimitar.transform.rotation);
        }
        */
    }

    //Spawn obj inside specified radius
    void SpawnObjAtRandom(GameObject spawnableObj)
    {
        //random pos around spawner obj
        Vector3 randomPos = transform.position + Random.insideUnitSphere * spawnRadius;

        print("Spawned " + spawnableObj.name + " at " + randomPos);

        //if photon network connected:
        if (PhotonNetwork.IsConnected)
        {
            //create only 1 scimitar regardless of player count:
            PhotonNetwork.InstantiateRoomObject(spawnableObj.name, randomPos, spawnableObj.transform.rotation);

            //create a scimitar for every new player joining, when they load in:
            //PhotonNetwork.Instantiate(scimitar.name, spawnPnt.position, scimitar.transform.rotation);
        }
        else
        {
            //create a local scimitar:
            Instantiate(spawnableObj, randomPos, spawnableObj.transform.rotation);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(this.transform.position, spawnRadius);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnItems : MonoBehaviour
{
    public Transform spawnPnt;

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
            SpawnObjAtRandom(transform.position, spawnableObj, spawnRadius);
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

    //Spawn only 1 obj regardless of player count inside specified radius 
    public void SpawnObjAtRandom(Vector3 spawnPos, GameObject spawnableObj, float radius)
    {
        //random pos around spawner obj
        Vector3 randomPos = spawnPos + Random.insideUnitSphere * radius;

        print("Spawned " + spawnableObj.name + " at " + randomPos);

        //if photon network connected:
        if (PhotonNetwork.IsConnected)
        {
            //create only 1 obj regardless of player count:
            PhotonNetwork.InstantiateRoomObject(spawnableObj.name, randomPos, spawnableObj.transform.rotation);

            //create an obj for every new player joining, when they load in:
            //PhotonNetwork.Instantiate(scimitar.name, spawnPnt.position, scimitar.transform.rotation);
        }
        else
        {
            //create a local obj:
            Instantiate(spawnableObj, randomPos, spawnableObj.transform.rotation);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(this.transform.position, spawnRadius);
    }
}

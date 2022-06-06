using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RandoSpawnPlayers : MonoBehaviour
{
    public GameObject playerPrefab;
    public float spawnRadius = 1;

    // Start is called before the first frame update
    void Start()
    {
        SpawnObjAtRandom(transform.position, playerPrefab, radius:spawnRadius);
    }


    //Spawn obj inside specified radius for every new player joining
    private void SpawnObjAtRandom(Vector3 spawnPos, GameObject spawnableObj, float radius)
    {
        //random pos around spawner obj
        Vector3 randomPos = spawnPos + Random.insideUnitSphere * radius;

        print("Spawned " + spawnableObj.name + " at " + randomPos);

        //if photon network connected:
        if (PhotonNetwork.IsConnected)
        {
            //create only 1 obj regardless of player count:
            //PhotonNetwork.InstantiateRoomObject(spawnableObj.name, randomPos, spawnableObj.transform.rotation);

            //create a obj for every new player joining, when they load in:
            PhotonNetwork.Instantiate(spawnableObj.name, randomPos, spawnableObj.transform.rotation);

            //print("Photon obj inst'd");
        }
        else
        {
            //create a local scimitar:
            Instantiate(spawnableObj, randomPos, spawnableObj.transform.rotation);

            //print("local obj inst'd");
        }
    }
    

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(this.transform.position, spawnRadius);
    }
}

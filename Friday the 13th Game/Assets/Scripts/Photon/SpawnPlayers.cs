using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject playerPrefab;
    public float spawnRadius = 1;

    //boundaries of spawn area:
    //public float minX;
    //public float maxX;
    //public float minY;
    //public float maxY;
    //public float minZ;
    //public float maxZ;

    //public GameObject spawnPlatform;

    // Start is called before the first frame update
    void Start()
    {
        /*
        //store spawn info:
        Vector3 spawnPnt = spawnPlatform.transform.position;
        float scaleX = spawnPlatform.transform.lossyScale.x;
        float scaleZ = spawnPlatform.transform.lossyScale.z;

        //set possible spawn coords based on spawn platform:
        minX = spawnPnt.x - scaleX / 2;
        maxX = spawnPnt.x + scaleX / 2;

        minZ = spawnPnt.z - scaleZ / 2;
        maxZ = spawnPnt.z + scaleZ / 2;

        //rando position within spawn area boundaries:
        Vector3 randomPosition = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), Random.Range(minZ, maxZ));

        //if photon network connected:
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Instantiated in photon network");

            //spawn player at random position w/ curr rot in the server:
            PhotonNetwork.Instantiate(playerPrefab.name, randomPosition, playerPrefab.transform.rotation); //use photon instantiate so players can see other players independent of game instances
        }
        else
        {
            //spawn player at rando positon locally:
            Instantiate(playerPrefab, randomPosition, playerPrefab.transform.rotation);
        }
        */

            //use SpawnItems script
            // to spawn in players randomly within sphere
            //SpawnItems spawnScript = FindObjectOfType<SpawnItems>();
            //spawnScript.SpawnObjAtRandom(transform.position, playerPrefab, spawnRadius);

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

            print("Photon obj inst'd");
        }
        else
        {
            //create a local scimitar:
            Instantiate(spawnableObj, randomPos, spawnableObj.transform.rotation);

            print("local obj inst'd");
        }
    }
    

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(this.transform.position, spawnRadius);
    }
}

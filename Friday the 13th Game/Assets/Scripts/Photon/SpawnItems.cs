using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnItems : MonoBehaviour
{
    public Transform spawnPnt;

    public GameObject scimitar;

    // Start is called before the first frame update
    void Start()
    {
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
    }
}

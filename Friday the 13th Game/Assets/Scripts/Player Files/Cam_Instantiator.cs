using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Photon.Pun;

public class Cam_Instantiator : MonoBehaviour
{
    //*******init in inspector*****:
    public GameObject mainCam;
    public GameObject camController;
    public GameObject lookAtPnt;
    public PhotonView photonView;

    public ThirdPersonMovement movement; //for moving based on camera
    public PlayerManager playManager;

    //to setup cam controller:
    private CinemachineFreeLook freeLookComp;

    //for minimap
    public GameObject minimapCamPrefab;

    // Start is called before the first frame update
    void Start()
    {
        //if this obj isnt mine and we connected to the photon network:
        if (!(photonView.IsMine) && PhotonNetwork.IsConnected)
        {
            //dont execute any movement code:
            return;
        }

        //instantiate main cam w/ settings:
        GameObject camCopy = Instantiate(mainCam);

        //instantiate cam controller w/ settings:
        freeLookComp = camController.GetComponent<CinemachineFreeLook>();
        freeLookComp.Follow = transform;
            //freeLookComp.LookAt = transform.GetChild(0); //'lookat' transform needs to be 1st child of player to work
        freeLookComp.LookAt = lookAtPnt.transform;
        GameObject camControlCopy = Instantiate(camController);

        //fill out player scripts:
        movement.playerCam = camCopy.transform;
        playManager.thirdPersonCamController = camControlCopy;

        //MINIMAP CAM

        //create minimap cam
        GameObject minimapCamCopy = Instantiate(minimapCamPrefab);

        //fill player field
        minimapCamCopy.GetComponent<MiniMap>().player = transform;
    }

}

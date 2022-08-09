using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Photon.Pun;

public class Cam_Instantiator : MonoBehaviour
{
    #region Vars

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
    public GameObject minimapIconCamPrefab;
    public GameObject minimapCamPrefab;
    private GameObject minimapIconCamCopy;
    private GameObject minimapCamCopy;

    #endregion Vars

    // Start is called before the first frame update
    void Start()
    {
        //if this obj isnt mine and we connected to the photon network:
        if (!(photonView.IsMine) && PhotonNetwork.IsConnected)
        {
            //dont spawn any cams for them:
            return;
        }

        //find cam already in scene
        Camera preExistingCamera = FindObjectOfType<Camera>();
        //if already a camera in the scene
        if ( preExistingCamera != null )
        {
            //destroy it
            Destroy(preExistingCamera.gameObject);
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
    }

    #region Minimap Spawn Methods

    public void SpawnRealMinimap()
    {
        //destroy icon minimap cam if already spawned
        if(minimapIconCamCopy != null)
        {
            Destroy(minimapIconCamCopy);
        }

        //instantiate real minimap cam

        //create minimap cam
        minimapCamCopy = Instantiate(minimapCamPrefab);

        //fill player field
        minimapCamCopy.GetComponent<MiniMap>().player = transform;
    }

    public void SpawnIconMinimap()
    {
        //destroy real minimap cam if already spawned
        if(minimapCamCopy != null)
        {
            Destroy(minimapCamCopy);
        }

        //instantiate icon minimap cam

        //create minimap cam
        minimapIconCamCopy = Instantiate(minimapIconCamPrefab);

        //fill player field
        minimapIconCamCopy.GetComponent<MiniMap>().player = transform;
    }

    #endregion Minimap Spawn Methods
}

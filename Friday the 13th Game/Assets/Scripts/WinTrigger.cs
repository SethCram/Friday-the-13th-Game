using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    public int zChange = 5;
    private bool shouldLaunchPlayerBack = false;
    private bool shouldLaunchPlayerForward = false;
    private Transform collidingWithTransform;

    /// <summary>
    /// When counselor triggers, tele them forward and tell them they win.
    /// When jason triggers, tele them backward.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        //store player manager and colliding w/ transform
        PlayerManager playerManager = null; 
        collidingWithTransform = other.GetComponent<Transform>();

        //if colliding w/ jason or counselor or player
        if (GameManager.Instance.TagIsCounselor(other.tag) 
            || GameManager.Instance.TagIsJason(other.tag))
        {
            //store player manager
            playerManager = other.GetComponent<PlayerManager>();

        }

        //if counselor colliding + player not dead
        if (GameManager.Instance.TagIsCounselor(other.tag) && !playerManager.GetDead() )
        { 
            shouldLaunchPlayerForward = true;

            Debug.Log("Player should launch forward and win.");

            //Debug.LogAssertion($"{other.name} is the collider name.");

            //if other photon view is mine or not connected to network
            if (other.GetComponent<PhotonView>().IsMine || !PhotonNetwork.IsConnected)
            {
                playerManager.CounselorDied(localPlayerWon: true);
            }

        }
        //if jason colliding
        else if(GameManager.Instance.TagIsJason(other.tag))
        {
            shouldLaunchPlayerBack = true;

            Debug.LogAssertion("Player should launch back.");
        }
    }

    /// <summary>
    /// Launch player in the desired direction
    /// </summary>
    private void FixedUpdate()
    {
        //if should launch player back
        if(shouldLaunchPlayerBack)
        {
            //don't launch again
            shouldLaunchPlayerBack = false;

            LaunchPlayer(
                new Vector3(collidingWithTransform.position.x, 
                    collidingWithTransform.position.y, 
                    collidingWithTransform.position.z - zChange) 
            );
        }
        //if should launch player forward
        else if(shouldLaunchPlayerForward)
        {
            //don't launch again
            shouldLaunchPlayerForward = false;

            LaunchPlayer(
                new Vector3(collidingWithTransform.position.x,
                    collidingWithTransform.position.y,
                    collidingWithTransform.position.z + zChange)
            );
        }
    }

    /// <summary>
    /// Launch player to given position.
    /// MUST be called in FixedUpdated() bc Physics
    /// </summary>
    /// <param name="newPosition"></param>
    private void LaunchPlayer(Vector3 newPosition)
    {
        Debug.Log("Player launched.");

        //set position to setBack on the z
        collidingWithTransform.position = newPosition;

        Debug.LogAssertion("Player transform should be " + collidingWithTransform.position.ToString());
    }
}

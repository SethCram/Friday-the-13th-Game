using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    public int zChange = 5;
    private bool shouldLaunchPlayerBack = false;
    private bool shouldLaunchPlayerForward = false;
    private Transform collidingWithTransform;

    private void OnTriggerEnter(Collider other)
    {
        //store player manager and colliding w/ transform
        PlayerManager playerManager = other.GetComponent<PlayerManager>();
        collidingWithTransform = other.GetComponent<Transform>();

        //if couneslor colliding
        if ( other.tag == "Player")
        {
            shouldLaunchPlayerForward = true;

            Debug.Log("Player should launch forward and win.");

            //set player as dead
            playerManager.SetDead(true);

            GameManager.Instance.GlobalIncrCounselorsDead();

            //cause counselor to win: gameover based on if all counselors dead
            playerManager.Win( isGameOver: GameManager.Instance.CheckAllCounselorsDead() );

            //if all counselors won or lost, need to tell jason generic game over 

        }
        //if jason colliding
        else if( other.tag ==  "Enemy" )
        {
            shouldLaunchPlayerBack = true;

            Debug.Log("Player should launch back.");
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

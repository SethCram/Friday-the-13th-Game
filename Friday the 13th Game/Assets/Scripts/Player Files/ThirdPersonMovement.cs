using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ThirdPersonMovement : MonoBehaviour
{
    #region Variables

    private CharacterController controller;
    public CharacterCombat combat; //init in inspector
    public CharacterAnimator charAnimator; //*********init in inspector***
        //public PlayerButtons playerButtons; //init inspector
    public PhotonView photonView;   //*********init in inspector***
    public PlayerManager playerManager;

    //cam following player:     (drop)
    public Transform playerCam;

    [HideInInspector]
    public bool cutMotionControls = false; //invert w/ UI opened/closed callback invoked

    //different speeds:
    public float crouchSpeed = 2f;
    public float walkSpeed = 5f;
    public float runSpeed = 7f; //ranges tween 7 and 12
    private float charSpeed;

    //time taken to smooth out turning:
    public float turnSmoothTime = 0.3f;
    private float turnSmoothVelocity;

    //to check if grounded:
    public Transform groundCheck; //for checking if grounded
    public float groundSphereRadius = 0.4f; //sphere drawn around 'groundCheck' obj
    public LayerMask groundMask; //layers sphere is checking for
    public bool isGrounded { private set; get; } //restricts changing var to this class only

    //gravity application:
    public float verticalVelocity { private set; get; } //we only use its y-coord
    public float gravity = -20f; //default gravity in m/s (changing to -20f usually feels better)

    //jumping variables:
    public float jumpHeight = 1; //height want player to be able to jump
    //public float jumpCooldown = 2;
    public float midAirSlopeLimit = 90;
    public float midAirStepOffset = 0.1f;
    public float midAirHeight = 1;
    //private float jumpTime;

    //grounded reset for after jumping:
    public float groundedSlopeLimit = 45;

    //reset for after jumping and crouching:
    public float walkingStepOffset = 0.4f;

    //crouching- controller collider settings:
    public Vector3 crouchedCenter = new Vector3(0f, 0.6f, 0f);
    private Vector3 ogCenter;
    public float crouchedHeight = 1;
    private float ogHeight;
    private bool stuckCrouched = false;

    //this downward velocity makes sure player rlly on ground:
    float groundedDownwardVelocity = -2f;

    [HideInInspector]
    public bool isDodging = false; //to cut off UI access w/ dodging

    //vars needed by 'CharacterAnimator' to `animate:
    public bool running { get; private set; }
    public bool crouched { get; private set; }
    public bool jump { get; private set; }

    #endregion

    #region Unity Methods

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();

        //store curr collider settings:
        ogCenter = controller.center;
        ogHeight = controller.height;

        //start char speed out at walk speed:
        charSpeed = walkSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        //if this obj isnt mine and we connected to the photon network:
        if(!(photonView.IsMine) && PhotonNetwork.IsConnected)
        {
            //dont execute any movement code:
            return;
        }

        /*
        //make sure motion controls cut if atking or dodging
        if(isDodging || combat.isAtking)
        {
            //cutMotionControls();
            cutMotionControls = true;
        }
        */

        //set if currently grounded:
        isGrounded = Physics.CheckSphere(groundCheck.position, groundSphereRadius, groundMask);

        //if player grounded, apply a grounded reset:
        if (isGrounded && verticalVelocity < 0)        //isn't second check redundant? (if grounded, vert velocity should be around -2)
        {
            GroundedReset();
        }

        //debug: Debug.Log("cutMotionControls: " + cutMotionControls.ToString() );

        //apply current motion controls if we don't want to cut them:
        if (cutMotionControls == false)
        {
            applyMotion();
        }
        else
        {
            //make sure not running anymore:
            running = false;
        }

        //apply gravity every second in freefall:
        verticalVelocity += gravity * Time.deltaTime;
        controller.Move(new Vector3(0, verticalVelocity * Time.deltaTime, 0));

        //reset y-coord velocity if controller collides w/ something above it (need to be below all 'Move()' calls bc they ret collision flags):
        if ((controller.collisionFlags & CollisionFlags.Above) != 0)
        {
            verticalVelocity = groundedDownwardVelocity;

            //check: Debug.Log("Vertical velocity reset bc of head collision");
        }
    }

    #endregion

    #region Aerial Methods

    //reset y velocity, slope limit, and step offset:
    private void GroundedReset()
    {
        //reset slope limit and step offset w/ land:
        controller.slopeLimit = groundedSlopeLimit;
        controller.stepOffset = walkingStepOffset;

        //reset collider height w/ land:
        //controller.height = ogHeight;

        verticalVelocity = groundedDownwardVelocity; //not zero just incase player not totally on the grnd yet, this will force him to the grnd
    }



    /*
    public void LandedHeight()
    {
        Invoke(ResetControllerHeight(), 3);
    }
    */

    #endregion Aerial Methods

    /*
    private string ResetControllerHeight()
    {
        Debug.Log("Resetting controller height");

        controller.height = ogHeight;

        return "ResetControllerHeight";
    }
    */

    //includes walking, rotating, running, jumping, and crouching:
    private void applyMotion()
    {
        //reset 'jump' command:
        jump = false;

        //if grounded, check if want  to run or crouch:
        if (isGrounded)
        {
            //set crouching before running bc dont want crouch-running

            //crouch by decreasing player speed and making the character controller collider smaller:
            if (Input.GetButton("Crouch"))
            {
                Crouch();
            }
            //Running- set char speed to 'runSpeed':
            else if (Input.GetButton("Run")) //'Fire3' = axis for left shift, (mouse 2?), and controller something
            {
                //Debug.Log("Started Running");

                charSpeed = runSpeed;

                running = true;
            }
            //if not running or crouching, char's speed is its walking speed:
            else
            {
                charSpeed = walkSpeed;
            }
        }
        else
        {
            //reset 'running' so dont try to use run anim w/ midair:
            running = false;
        }

        //try and uncrouch if crouch button released:
        if (Input.GetButtonUp("Crouch"))
        {
            if (UnCrouch())
            {
                //stand player up:
                crouched = false;
            }
            else
            {
                Debug.Log("Couldn't uncrouch here");

            }
        }

        //stop running if run button released:
        if(Input.GetButtonUp("Run"))
        {
            //check: Debug.Log("Stopped running");

            charSpeed = walkSpeed;

            running = false;
        }

        //if stuck in crouch, check if can uncrouch yet:
        if (stuckCrouched)
        {
            if (UnCrouch())
            {
                //stand player up:
                crouched = false;

                Debug.Log("Uncrouched");
            }
        }

        //all crouching must be done before player motion, so player speed applied correctly

        //control basic character movement:
        WalkAndRotate();

        //if currently grounded and didn't jump w/ crouched, can jump or attack or dodge:
        if (isGrounded && !crouched)
        {
            //if pressed the button and jump cooldown is up:
            if (Input.GetButtonDown("Jump") ) //&& Time.time - jumpTime > jumpCooldown)
            {
                //checks: print("Curr time: " + Time.time); print("Time jumped at: " + jumpTime);

                Jump();
            } 
            //if pressed an attack button:
            else if(Input.GetButtonDown("Attack0"))
            {
                //activate the 0th attack anim:
                combat.ActivateAttack(0);  
            }
            else if(Input.GetButtonDown("Attack1"))
            {
                //activate the 1st attack anim:
                combat.ActivateAttack(1);
            }
            else if(Input.GetButtonDown("Dodge"))
            {
                Dodge();
            }
        }
    }

    #region ActionMethods

    //crouch by decreasing player speed and making the character controller collider smaller:
    private void Crouch()
    {
        charSpeed = crouchSpeed;

        //move character controller's center to char's knees:
        controller.center = crouchedCenter;                      //controller.center.y = -0.5f; //doesnt work?

        //decrease controller's height to crouching height:
        controller.height = crouchedHeight;

        //clear step offset to allow char fitting:
        controller.stepOffset = 0;

        //tell player to crouch:
        crouched = true;
    }

    //reset collider and check if we can stand, if not re-shrink collider:
    private bool UnCrouch()
    {
        float controllerRadius = controller.radius;

        //cast a capsule based on step offset, player size, and radius to check if can stand (since collision flags wont work) (only detects grnd layer):
                                     //if( Physics.CheckCapsule(transform.position + new Vector3(0, walkingStepOffset, 0), transform.position + new Vector3(0, 2, 0), controller.radius, groundMask)) //doesnt work bc it constructs capsule as spheres around feet and head w/ given radius
        if (Physics.CheckCapsule(transform.position + Vector3.up * (controllerRadius + Physics.defaultContactOffset + 0.1f),          //add '0.1f' to account for moving char controller's center up 0.1 meter
            transform.position + Vector3.up * (2 + walkingStepOffset - controllerRadius), 
            controllerRadius - Physics.defaultContactOffset, groundMask, QueryTriggerInteraction.Ignore))
        {
            //dont stand

            //set character crouch speed to override constantly setting of walk speed:
            charSpeed = crouchSpeed;

            stuckCrouched = true;

            return false;
        }
        else
        {
            //stand

            //reset controller collider settings to og:
            controller.center = ogCenter;
            controller.height = ogHeight;
            controller.stepOffset = walkingStepOffset;

            stuckCrouched = false;

            return true;
        }
    }

    //apply movement and rotation to char controller based on input:
    private void WalkAndRotate()
    {
        //get input:
        float horizontalInput = Input.GetAxisRaw("Horizontal"); //uses 'Raw' axis so no smoothing is applied (for suddent stopping)
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontalInput, 0f, verticalInput).normalized; //'.normalized' added on so max val is 1, even w/ both vertical and horizontal input are maxed out

        //if want to move in any direction, tell player controller to move in that direction:
        if (direction.magnitude >= 0.1f)
        {
            //calc rads player needs to rot based on direction want to move:
            float targetAngle = Mathf.Atan2(direction.x, direction.z);

            //convert radians to degrees:
            targetAngle = targetAngle * Mathf.Rad2Deg;

            //make the target angle dependent on the cam's rotation:
            float camPointingTargetAngle = targetAngle + playerCam.eulerAngles.y;

            //smooth our target angle using 'turnSmoothTime' to turn slower:
            float smoothedAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, camPointingTargetAngle, ref turnSmoothVelocity, turnSmoothTime);

            //set player's rot equal to smoothed angle:
            transform.rotation = Quaternion.Euler(0f, smoothedAngle, 0f); //use 'Eulers' instead of 'Rotation/Rotate' to match vals in inspector

            //calc new move direction based off the target angle calced based off the camera's rotation:
            Vector3 moveDirection = Quaternion.Euler(0f, camPointingTargetAngle, 0f) * Vector3.forward; //turn a rotation into a direction by multing by 'Vector3.forward'

            //normalized so never greater than 1:
            moveDirection = moveDirection.normalized;

            //tell player controller to move in this direction:
            controller.Move(moveDirection * charSpeed * Time.deltaTime); //mult by 'Time.deltaTime' to make it framerate independant (bc we in update() and not fixedupdate())
        }
    }

    //jump and change slope limit, step offset:
    private void Jump()
    {
        //increase slope limit to avoid jitter w/ mid-air
        controller.slopeLimit = midAirSlopeLimit;

        //lower step offset so dont land on surfaces higher than feet:
        controller.stepOffset = midAirStepOffset;

        //decrease size of collider to land properly:
        //controller.height = crouchedHeight;
        //controller.center = crouchedCenter;

        //set vertical velocity based on jump height want to jump:
        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity); // v = sqrt(h * -2 * g) is the velocity req'd to jump certain height

        //check for head collision done in 'update()'

        //make player jump:
        jump = true;

        //set time w/ char jumped:
        //jumpTime = Time.time;
    }

    //dodge character in direction facing:
    private void Dodge()
    {
        //set dodging:
        isDodging = true;

        //animate dodging:
        charAnimator.AnimateDodge();

        //decrease size of controller:
        controller.height = crouchedHeight;
        controller.center = crouchedCenter;
    }
    #endregion

    /*
    //continue curr motion for 'time':
    public void CountinueMotion(float time)
    {
        print("Char controller velocity to continue: " + controller.velocity);

        controller.Move(controller.velocity * Time.deltaTime);
    }
    */

    //invoked by 'CharAnimator' w/ a delay to let atk clip play out:
    public void AttackFinished()
    {
        //resume motion controls:
        //cutMotionControls = false;
        ResumeMotionCntrls();

        //check: print("Reconnecting motion controls");

        //no longer atking:
        combat.isAtking = false;
    }

    //invoked by 'CharAnimator' w/ a delay to let dodge clip play out:
    public void DodgeFinished()
    {
        //reset size of controller: (Uncrouch() does so if successful)
        //controller.height = ogHeight;
        //controller.center = ogCenter;

        //try to stand:
        if(!(UnCrouch()))
        {
            Debug.Log("Couldnt stand here");

            //start crouching:
            Crouch();

        }

        //resume motion controls:
        //cutMotionControls = false;
        ResumeMotionCntrls();

        //no longer dodging:
        isDodging = false;
    }

    //invoked by 'CharAnimator' w/ a delay to let hurt clip play out:
    public void HurtFinished()
    {
        ResumeMotionCntrls();

        //set 'isBeingHurt' to false so cant open UI?
    }

    //enable motion controls if no menus open
    private void ResumeMotionCntrls()
    {
        //print(playerButtons.paused);

        //if player not paused:
        if (!playerManager.paused)
        {
            //resume motion control:
            cutMotionControls = false;
        }
    }

    //draw black wire sphere around 'groundCheck' obj when player obj is selected:
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(groundCheck.position, groundSphereRadius);
    }
}

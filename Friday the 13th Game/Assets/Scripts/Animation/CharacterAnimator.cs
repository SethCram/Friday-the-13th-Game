using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CharacterAnimator : MonoBehaviour
{
    #region vars

    const float locomotionAnimationSmoothTime = 0.1f; //const bc it never changes

    protected Animator animator; //'protected' so accessable to inherited classes
    protected CharacterCombat combat;
    protected AnimatorOverrideController animatorOverrideController; //to override anims and replace them
    
    //to keep track of all our atk anims:
    protected AnimationClip[] currAtkAnimSet;
    public AnimationClip[] defaultAtkAnimSet; //drag+dropped into
    public AnimationClip replaceableAtkAnim; //drag+dropped into

    public AnimationClip dodgeClip; //*****init in inspector*******

    public AnimationClip hurtClip; //*******init in inspector*******

    public AnimationClip deathClip; //*******init in inspector*******

    //inited beforehand:
    public PlayerManager playerManager;

    //vars to find player's velocity:
    Vector3 previousPos;
    Vector3 velocity;

    private ThirdPersonMovement movement; //needed to get walk speed and whether running

    public PhotonView photonView;
    private bool dying = false;

    //private bool groundedPreviously = false;
    //private bool landing = false;

    #endregion

    #region Unity Methods

    // Start is called before the first frame update
    protected virtual void Awake()
    {
        //agent = GetComponent<NavMeshAgent>();

        animator = GetComponentInChildren<Animator>();

        combat = GetComponent<CharacterCombat>();

        //override controller linked to scene animator controller:
        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController); //create a new instance of the animator constroller used during runtime 
        animator.runtimeAnimatorController = animatorOverrideController;

        //initialize current atk anim set:
        currAtkAnimSet = defaultAtkAnimSet;

        //subscribe atttack method to it's event:
        combat.OnAttackCallback += AnimateAttack;

        //needed to get the walk speed:
        movement = GetComponent<ThirdPersonMovement>();

        //Debug.LogError("Char Animator Awake finished");

    }

    // Update is called once per frame
    protected virtual void Update()
    {

        //if this obj isnt mine and we connected to the photon network:
        if (!(photonView.IsMine) && PhotonNetwork.IsConnected)
        {
            //dont execute any movement code:
            return;
        }

        //if dying or dead
        if( dying || playerManager.GetDead() )
        {
            //cut motion cntrls
            movement.cutMotionControls = true;
        }

        //dont divide by 0 (happens if time is paused):
        if (Time.deltaTime != 0)
        {
            //find crude velocity of player:
            velocity = (transform.position - previousPos) / Time.deltaTime;
        }

        previousPos = transform.position;

        //transition tween idle and walking:
        float speedPercent;

        //set 'speedPercent' based on whether crouched or not:
        if (movement.crouched)
        {
            //set to idle and crawling based on speed of travel:
            speedPercent = velocity.magnitude / movement.crouchSpeed;
        }
        else if (movement.running)
        {
            //set to idle and run based on speed of travel:
            speedPercent = velocity.magnitude / movement.runSpeed;

        }
        else 
        { 
            //set to idle and walking based on speed of travel:
            speedPercent = velocity.magnitude / movement.walkSpeed; //curr speed / max speed
        }

        //clamp value bc should be tween 0 and 1:
        speedPercent = Mathf.Clamp(speedPercent, 0, 1);

        animator.SetFloat("speedPercent", speedPercent, locomotionAnimationSmoothTime, Time.deltaTime); //dampTime = time taken to smooth tween two values ('locomotionAnimationSmoothTime' here)

        //update whether in combat:
        //animator.SetBool("inCombat", combat.inCombat);

        //update whether running:
        animator.SetBool("running", movement.running);

        //update whether crouching:
        animator.SetBool("crouching", movement.crouched);

        //tell w/ to jump (should set to false frame after 'jump' button pressed):
        animator.SetBool("jump", movement.jump);


        /*
        //if landing:
        if (!groundedPreviously && movement.isGrounded)
        {
            Debug.Log("Invoke landing anim in 1 sec");

            //reset height:
            //movement.LandedHeight();
        }
        

        //set landing as false w/ landing anim finishes

        //groundedPreviously = movement.isGrounded;

        //SetAnimatorMidair();
        */

        //tell w/ player animator in the air:
        animator.SetBool("midair", !(movement.isGrounded));
    }

    #endregion

    #region Action Methods

    /*
    private void SetAnimatorMidair()
    {
        //tell w/ player animator in the air:
        animator.SetBool("midair", !(movement.isGrounded));
    }
    */

    #region Atk Methods

    //activated by the 'onAtkCallback' in 'CharacterCombat' script:
    protected virtual void AnimateAttack(int atkIndex)
    {
        float atkAnimLength;

        //set 'attack' trigger param to true:
        animator.SetTrigger("attack");

        //make sure there's atleast 2 anims if use 'attack1' axis, if not uses attack0 anim:
        if (atkIndex > currAtkAnimSet.Length - 1)      //need to subtract 1 bc index starts at 0, but that's equiv length is 1
        {
            animatorOverrideController[replaceableAtkAnim.name] = currAtkAnimSet[atkIndex - 1]; //minus 1 so if not an atk in index 1, defaults to atk in index 0 
        }
        else
        {
            //swap out default atk anim for an anim in curr atk set:
            animatorOverrideController[replaceableAtkAnim.name] = currAtkAnimSet[atkIndex];
        }

        //store atk anim length:
        atkAnimLength = animatorOverrideController[replaceableAtkAnim.name].length;

        //continue at curr speed for duration of atk: 
        //movement.CountinueMotion(atkAnimLength);

        animator.applyRootMotion = true;

        //enable player motion:
        movement.cutMotionControls = true;

        //check: print("Cut motion controls");

        //check: Debug.LogError("Atk animation is: " + animatorOverrideController[replaceableAtkAnim.name].name);

        //reconnect player control after atk anim done:
        movement.Invoke("AttackFinished", atkAnimLength);    //can use 'class obj'.Invoke to delay calling a funct in another class

        //disable root motion after atk clip plays:
        Invoke("DisableRootMotion", atkAnimLength);

    }

    [PunRPC]
    public void RPC_AnimateAttack(int atkIndex)
    {
        float atkAnimLength;

        //set 'attack' trigger param to true:
        animator.SetTrigger("attack");

        //make sure there's atleast 2 anims if use 'attack1' axis, if not uses attack0 anim:
        if (atkIndex > currAtkAnimSet.Length - 1)      //need to subtract 1 bc index starts at 0, but that's equiv length is 1
        {
            animatorOverrideController[replaceableAtkAnim.name] = currAtkAnimSet[atkIndex - 1]; //minus 1 so if not an atk in index 1, defaults to atk in index 0 
        }
        else
        {
            //swap out default atk anim for an anim in curr atk set:
            animatorOverrideController[replaceableAtkAnim.name] = currAtkAnimSet[atkIndex];
        }

        //store atk anim length:
        atkAnimLength = animatorOverrideController[replaceableAtkAnim.name].length;

        animator.applyRootMotion = true;

        //check: Debug.LogError("Applied root motion");

        //enable player motion:
        movement.cutMotionControls = true;

        //check: print("Cut motion controls");

        //reconnect player control after atk anim done:
        movement.Invoke("AttackFinished", atkAnimLength);    //can use 'class obj'.Invoke to delay calling a funct in another class

        //disable root motion after atk clip plays:
        Invoke("DisableRootMotion", atkAnimLength);

    }

    #endregion Atk Methods

    //disable root motion:
    private void DisableRootMotion()
    {
        animator.applyRootMotion = false;

        //check: Debug.LogError("Disabled root motion");
    }

    //activated by 'Dodge()' in '3rdPersonMove':
    public void AnimateDodge()
    {
        //set dodge clip length:
        float dodgeTime = dodgeClip.length;

        //tell animator to dodge:
        animator.SetTrigger("dodge");

        //cut motion controls:
        movement.cutMotionControls = true;

        //apply root motion:
        animator.applyRootMotion = true;

        //tell w/ dodge finished:
        movement.Invoke("DodgeFinished", dodgeTime);

        //disable root motion after dodge clip finished:
        Invoke("DisableRootMotion", dodgeTime);
    }

    //activated by 'TakeDmg()' in 'CharStats':
    public void AnimateDmgTaken()
    {
        Debug.Log("Animate taking dmg");

        //set time taking dmg for:
        float takeDmgTime = hurtClip.length;

        //tell animator we're hurt:
        animator.SetTrigger("hurt");

        //cut motion controls:
        movement.cutMotionControls = true;

        //activate w/ dmg taken finished:
        movement.Invoke("HurtFinished", takeDmgTime);
    }
    /// <summary>
    /// kills player thru animation, sets death colliders, shows game over UI
    /// called by 'CharacterStats' 
    /// </summary>
    /// <returns></returns>
    public IEnumerator Die()
    {
        //set dodge clip length:
        float deathClipLen = deathClip.length;

        dying = true;

        Debug.Log("Player should animate dying.");

        //cut motion controls (keeps cutting till dying false)
        movement.cutMotionControls = true;

        //apply root motion + set trigger to anim
        animator.applyRootMotion = true;
        animator.SetTrigger("dead");

        //wait till death clip is played
        yield return new WaitForSeconds(deathClipLen + 0.3f);

        //AFTER PLAYER DONE DEATH ANIMATING

        //unapply root motion
        animator.applyRootMotion = false;

        //no longer dying
        dying = false;

        //setup dead colliders

        //network connected
        if(PhotonNetwork.IsConnected)
        {
            movement.photonView.RPC("DeadSetColliders", RpcTarget.AllBuffered);
        }
        //local play
        else
        {
            movement.DeadSetColliders();
        }

        //if in the game scene
        if( GameManager.Instance.currentScene == GameManager.CurrentScene.GAME)
        {
            //if lost or won game already
            if( GameManager.Instance.GetWonGame() || GameManager.Instance.GetLostGame() )
            {
                //show generic death screen only locally
                playerManager.GenericDeathScreen();
            }
            //if haven't died once yet
            else
            {
                //if counselor loses
                if (tag == "Player")
                {
                    CounselorDied();
                }
                //if Jason died
                else if (tag == "Enemy")
                {
                    JasonDied();
                }
            }
        }
        //if in game lobby scene
        else if ( GameManager.Instance.currentScene == GameManager.CurrentScene.GAME_LOBBY )
        {
            //show generic death screen 
            playerManager.GenericDeathScreen();
        }
    }

    /// <summary>
    /// When counselor dies, incr dead counselors and lose. 
    /// Gameover if all counselors dead.
    /// </summary>
    private void CounselorDied()
    {
        Debug.Log("Counselor dead");

        GameManager.Instance.GlobalIncrCounselorsDead();

        //set game as lost by local dead counselor
        GameManager.Instance.SetLostGame(true);
        
        //check if all counselors dead
        Debug.Log( "Check for if all counselors dead resulted in: " + 
            GameManager.Instance.CheckAllCounselorsDead(localLose: true) );
    }
    

    /// <summary>
    /// When jason dies, tell counselors they won and I lost + game over for everyone
    /// </summary>
    private void JasonDied()
    {
        Debug.Log("Jason dead");

        //if on network
        if( PhotonNetwork.IsConnected)
        {
            //walk thru room players
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                //if player isn't local (isn't jason)
                if(!player.IsLocal)
                {
                    //tell counselor game over and who lost lose and who won win
                    GameManager.Instance.TellCounselorGameOver(player);
                }
            }
        }

        //make jason player lose bc died + game over
        playerManager.Lose(isGameOver: true);

        //should do this in update incase jason leaves
    }

    /// <summary>
    /// set the animator whether currently dead
    /// </summary>
    /// <param name="isDead"></param>
    public void SetAnimDead( bool isDead)
    {
        animator.SetBool("dead", isDead);
    }

    #endregion
}

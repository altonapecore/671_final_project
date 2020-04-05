using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationControl : MonoBehaviour
{
    // Inspector Assigned Variables
    [SerializeField] [Range(0.05f, 0.1f)] float _attackDuration = 0.05f;
    // Public Variables
    [HideInInspector] public PlayerControl playerControl;
    [HideInInspector] public PlayerInput playerInput;
    [HideInInspector] public PlayerItemControl playerItemControl;
    [HideInInspector] public Animator anim;
    [HideInInspector] public bool isStrafing = false;
    [HideInInspector] public bool isBlocking = false;
    [HideInInspector] public bool canAction = true;
    public float animationSpeed = 1;
    public GameObject target;
    public bool doingDamage = false;

    [Header("Sounds")]
    [Space(10)]
    public AudioClip playerAttack;

    private void Awake()
    {
        // Cache required components
        playerControl = GetComponent<PlayerControl>();
        playerInput = GetComponent<PlayerInput>();
        anim = GetComponent<Animator>();
        if (playerControl.currentItem == CharacterItem.Sword)
        {
            // Tell the animator to hold the sword
            anim.SetInteger("Weapon", 1);
        }
        else
        {
            // Set the default item to the animator
            anim.SetInteger("Weapon", 0);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Switch player collision on
        playerControl.SwitchCollisionOn();
    }

    // Update is called once per frame
    void Update()
    {
        // Update Animator Speed
        UpdateAnimatorSpeed();
        // Make sure the player can do actions
        if (canAction)
        {
            // Check if the player is trying to attack
            if (playerControl.currentItem == CharacterItem.Sword && playerInput.inputAttack)
            {
                // Do a left hand attack by default
                Attack(1);
            }
        }
    }

    /// <summary>
    /// Sets the animation speed value in the animator
    /// </summary>
    void UpdateAnimatorSpeed()
    {
        // Set the animation speed value in the animator
        anim.SetFloat("AnimationSpeed", animationSpeed);
    }

    #region Player Movement-Related Functionality
    public IEnumerator _Turning(int direction)
    {
        if (direction == 1)
        {
            LockPlayer(true, true, true, 0, 0.55f);
            anim.SetTrigger("TurnLeftTrigger");
        }
        if (direction == 2)
        {
            LockPlayer(true, true, true, 0, 0.55f);
            anim.SetTrigger("TurnRightTrigger");
        }
        yield return null;
    }
    #endregion

    #region Player Combat-Related Functionality
    /// <summary>
    /// Handles Player Attacking
    /// 0 = No side, 1 = Left, 2 = Right
    /// </summary>
    /// <param name="attackSide">Side at which the player is attacking from</param>
    public void Attack(int attackSide)
    {
        // Before anything, make sure the player that they has the Sword and can do actions
        if (playerControl.currentItem == CharacterItem.Sword && canAction)
        {
            int attackNumber = 0;
            // Check if this is a stationary attack
            if (!playerControl.isMoving)
            {
                // Set the maximum number of attacks
                int maxAttacks = 6;
                // Generate a random attack
                attackNumber = Random.Range(1, maxAttacks);
                // Lock the player in place
                LockPlayer(true, true, true, 0, 0.85f);
                //GameVars.instance.audioManager.PlaySFX(playerAttack, 0.2f, gameObject.transform.position);
            }

            //Trigger the animation.
            anim.SetInteger("Action", attackNumber);
            if (attackSide == 3)
            {
                anim.SetTrigger("AttackDualTrigger");
            }
            else
            {
                anim.SetTrigger("AttackTrigger");
            }         
        }
    }

    void HandleStrafing()
    {
        if (playerInput.inputStrafe)
        {
            if (playerControl.currentItem != CharacterItem.None)
            {
                anim.SetBool("Strafing", true);
                isStrafing = true;
            }
        }
        else
        {
            isStrafing = false;
            anim.SetBool("Strafing", false);
        }
    }

//Handle Hit
/*
    public void HandleHit(float damage = 0.0f)
    {
        // First off, make sure the player is alive
        if (playerControl.isAlive)
        {
            // Decrement health
            playerControl.Health -= damage;
            // Check if health is below zero
            if (playerControl.Health <= 0 && playerControl.isAlive)
            {
                // Set health to 0
                playerControl.Health = 0.0f;
                // Call the death functionality
                KillPlayer();
            }
            else
            {
                int hits = 5;
                if (isBlocking)
                {
                    hits = 2;
                }
                int hitNumber = Random.Range(1, hits + 1);
                anim.SetInteger("Action", hitNumber);
                anim.SetTrigger("GetHitTrigger");
                LockPlayer(true, true, true, 0.1f, 0.4f);
                if (isBlocking)
                {
                    StartCoroutine(playerControl._Knockback(-transform.forward, 3, 3));
                    return;
                }
                //Apply directional knockback force.
                if (hitNumber <= 1)
                {
                    StartCoroutine(playerControl._Knockback(-transform.forward, 8, 4));
                }
                else if (hitNumber == 2)
                {
                    StartCoroutine(playerControl._Knockback(transform.forward, 8, 4));
                }
                else if (hitNumber == 3)
                {
                    StartCoroutine(playerControl._Knockback(transform.right, 8, 4));
                }
                else if (hitNumber == 4)
                {
                    StartCoroutine(playerControl._Knockback(-transform.right, 8, 4));
                }
            }
            
        }
        
    }
*/

    public void KillPlayer()
    {
        playerControl.isAlive = false;
        anim.SetTrigger("Death1Trigger");
        LockPlayer(true, true, false, 0.1f, 0f);
    }
    #endregion

    #region Player Action-Related Functionality
    /// <summary>
    /// Keep player from doing actions.
    /// </summary>
    void LockAction()
    {
        canAction = false;
    }

    /// <summary>
    /// Let character move and act again.
    /// </summary>
    void UnLock(bool movement, bool actions)
    {
        if (movement)
        {
            playerControl.UnlockMovement();
        }
        if (actions)
        {
            canAction = true;
        }
    }
    #endregion

    #region Helper Methods/Miscellaneous Functionality
    /// <summary>
    /// Lock character movement and/or action, on a delay for a set time.
    /// </summary>
    /// <param name="lockMovement">If set to <c>true</c> lock movement.</param>
    /// <param name="lockAction">If set to <c>true</c> lock action.</param>
    /// <param name="timed">If set to <c>true</c> timed.</param>
    /// <param name="delayTime">Delay time.</param>
    /// <param name="lockTime">Lock time.</param>
    public void LockPlayer(bool lockMovement, bool lockAction, bool timed, float delayTime, float lockTime)
    {
        StopCoroutine("_Lock");
        StartCoroutine(_Lock(lockMovement, lockAction, timed, delayTime, lockTime));
    }

    //Timed -1 = infinite, 0 = no, 1 = yes.
    public IEnumerator _Lock(bool lockMovement, bool lockAction, bool timed, float delayTime, float lockTime)
    {
        if (delayTime > 0)
        {
            yield return new WaitForSeconds(delayTime);
        }
        if (lockMovement)
        {
            playerControl.LockMovement();
        }
        if (lockAction)
        {
            LockAction();
        }
        if (timed)
        {
            if (lockTime > 0)
            {
                yield return new WaitForSeconds(lockTime);
            }
            UnLock(lockMovement, lockAction);
        }
    }


    //Placeholder functions for Animation events.
    public void Hit()
    {
        if (!playerControl.playerDamageTrigger.isActiveAndEnabled)
        {
            StartCoroutine(HandleMeleeHitbox());
        }
    }

    public void WeaponSwitch()
    {
    }
    public void Shoot()
    {
    }

    public void FootR()
    {
    }

    public void FootL()
    {
    }

    public void Land()
    {
    }

    IEnumerator _GetCurrentAnimationLength()
    {
        yield return new WaitForEndOfFrame();
        float f = (float)anim.GetCurrentAnimatorClipInfo(0).Length;
    }
    #endregion

   public IEnumerator HandleMeleeHitbox()
   {
        playerControl.playerDamageTrigger.gameObject.SetActive(true);
        yield return new WaitForSeconds(_attackDuration);
        playerControl.playerDamageTrigger.currentIntersectingEnemies.Clear();
        playerControl.playerDamageTrigger.gameObject.SetActive(false);
   }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;
using UnityEngine.UI;





public enum CharacterState
{
    Idle = 0,
    Move = 1,
    Jump = 2,
    DoubleJump = 3,
    Fall = 4,
    Block = 6,
    Roll = 8
}

public enum CharacterItem { None, Sword, Navigator };

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerAnimationControl))]

public class PlayerControl : SuperStateMachine
{
    [Header("References")]
    // Inspector Assigned Variables
    [SerializeField] PlayerControl otherPlayer;
    [SerializeField] Camera playerCamera;
    [SerializeField] PlayerDamageTrigger _playerDamageTrigger;
    [SerializeField] Color flashColour = new Color(1f, 0f, 0f, 0.1f);
    [SerializeField] float flashSpeed = 0.5f;
    [SerializeField] Image damageOverlay;
    [SerializeField] [Range(1.0f, 100.0f)] float _damage = 50.0f;
    // Public Variables
    public PlayerInput playerInput;
    public SuperCharacterController superCharacterController;
    public PlayerAnimationControl playerAnimationControl;
    public CharacterState playerCharacterState;
    public CharacterItem currentItem;
    public GameObject swordItem;

    //[Header("Sounds")]
    //[Space(10)]
    //public AudioClip playerTakeDamage;
    //public AudioClip playerDie;

    [Header("Settings")]
    [Space(10)]
    public float movementAcceleration = 90.0f;
    public float walkSpeed = 4f;
    public float runSpeed = 6f;

    [HideInInspector] public Vector3 currentVelocity;
    [HideInInspector] public bool isMoving = false;
    [HideInInspector] public bool canMove = true;
    [HideInInspector] public Vector3 lookDirection { get; private set; }
    [HideInInspector] public bool isKnockback;
    public float knockbackMultiplier = 1f;

    // Private variables
    private ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
    private Transform m_Cam;                  // A reference to the player's camera transform
    private Vector3 m_CamForward;             // The current forward direction of the camera
    private Vector3 m_Move;

    private float rotationSpeed = 40f;
    public float groundFriction = 50f;
    private Animator anim;
    private Rigidbody rb;
    private bool damaged = false;

    // Audio variables
    [FMODUnity.EventRef]
    public string playerHurt;

    [FMODUnity.EventRef]
    public string playerDie;

    [FMODUnity.EventRef]
    public string inputSound = "";

    // Public Properties
    public bool isAlive { get; set; }
    public Camera Camera { get { return playerCamera; } set { playerCamera = value; } }
    public Transform CameraTransform { get { return m_Cam; } }
    public Vector3 LocalMovement { get { return playerInput.moveInput; } }
    public PlayerDamageTrigger playerDamageTrigger { get { return _playerDamageTrigger; } }
    public float Damage { get { return _damage; } set { _damage = value; } }

    private void Awake()
    {
        // Cache input and animator control
        playerInput = GetComponent<PlayerInput>();
        playerAnimationControl = GetComponent<PlayerAnimationControl>();
        superCharacterController = GetComponent<SuperCharacterController>();
        // Cache animator and rigidbody
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        // Get the camera transform
        m_Cam = playerCamera.transform;
        /*
        // Get third person character reference
        m_Character = GetComponent<ThirdPersonCharacter>();
        */
        // Set the current state to idle
        currentState = CharacterState.Idle;
        playerCharacterState = CharacterState.Idle;

        if (currentItem == CharacterItem.Sword)
        {
            anim.SetInteger("Weapon", 1);
            anim.SetTrigger("WeaponUnsheathTrigger");
        }
        //Idle_EnterState();
        // Make it live!
        isAlive = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        /*
        // Cache animator and rigidbody
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        // Get third person character reference
        m_Character = GetComponent<ThirdPersonCharacter>();
        // Set the current state to idle
        currentState = CharacterState.Idle;
        // Make it live!
        isAlive = true;
        */

        InvokeRepeating("CallFootsteps", 0, 0.5f);
    }

    private void Update()
    {
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("PlayerHealth", HealthMeter.Health / 10);

        if (damageOverlay && isAlive)
        {
            if (damaged)
            {
                damageOverlay.color = flashColour;
            }
            else
            {
                damageOverlay.color = Color.Lerp(damageOverlay.color, Color.clear, flashSpeed * Time.deltaTime);
            }
            damaged = false;
        }
    }
    /*
    void FixedUpdate()
    {
        // read inputs
        float h = Input.GetAxis(playerInput.horizontalAxis);
        float v = Input.GetAxis(playerInput.verticalAxis);
        bool crouch = Input.GetKey(KeyCode.C);


        // We use world-relative directions in the case of no main camera
        m_Move = v * Vector3.forward + h * Vector3.right;
        // calculate move direction to pass to character
        if (m_Cam != null)
        {
            // calculate camera relative direction to move:
            m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
            m_Move = v * m_CamForward + h * m_Cam.right;
        }
        else
        {
            // we use world-relative directions in the case of no main camera
            m_Move = v * Vector3.forward + h * Vector3.right;
        }

        // Pass all parameters to the character control script
        m_Character.Move(m_Move);
    }
    */
    public void ToggleSword(bool toggle, CharacterItem newItem = CharacterItem.None)
    {
        if (toggle)
        {
            // Check current item
            if (currentItem != CharacterItem.Sword)
            {
                // Set the current item
                currentItem = CharacterItem.Sword;
                // Tell the animator to hold the sword
                anim.SetInteger("Weapon", 1);
                anim.SetTrigger("WeaponUnsheathTrigger");
            }
            swordItem.SetActive(true);
        }
        else
        {
            // Check current item
            if (currentItem == CharacterItem.Sword)
            {
                // Set the current item
                currentItem = newItem;
                // Set the default item to the animator
                anim.SetInteger("Weapon", 0);
                anim.SetTrigger("WeaponSheathTrigger");
            }
            swordItem.SetActive(false);
        }
    }
    #region Update-Related Functionality
    /// <summary>
    /// Called after a state's update function
    /// </summary>
    protected override void LateGlobalSuperUpdate()
    {
        //Move the player by our velocity every frame.
        transform.position += currentVelocity * superCharacterController.deltaTime;
        //If alive and is moving, set animator.
        if (isAlive && canMove)
        {
            // Check if 
            if (currentVelocity.magnitude > 0 && playerInput.HasMoveInput())
            {
                isMoving = true;
                anim.SetBool("Moving", true);
                anim.SetFloat("Velocity Z", currentVelocity.magnitude);
            }
            else
            {
                isMoving = false;
                anim.SetBool("Moving", false);
            }
        }
        // Check if the player is strafing
        if (!playerAnimationControl.isStrafing)
        {
            // Check if we have movement input and that we can move
            if (playerInput.HasMoveInput() && canMove)
            {
                // Rotate towards the movement direction
                RotateTowardsMovementDir();
            }
        }
        else
        {
            // Handle strafing towards the target
            HandleStrafing(playerAnimationControl.target.transform.position);
        }
    }
    #endregion


    #region Movement/Rotation-Related Functionality
    //Keep character from moving.
    public void LockMovement()
    {
        // Disable movement
        canMove = false;
        // Tell the animator to stop moving
        anim.SetBool("Moving", false);
        // Apply root motion
        anim.applyRootMotion = true;
        // Zero out the current velocity
        currentVelocity = new Vector3(0, 0, 0);
    }

    public void UnlockMovement()
    {
        // Enable movement
        canMove = true;
        // Stop applying root motion
        anim.applyRootMotion = false;
    }

    void HandleStrafing(Vector3 targetPosition)
    {
        // Set X and Z velocities in the animator
        anim.SetFloat("Velocity X", transform.InverseTransformDirection(currentVelocity).x);
        anim.SetFloat("Velocity Z", transform.InverseTransformDirection(currentVelocity).z);
        // Rotate towards the target position
        RotateTowardsTarget(targetPosition);
    }

    /// <summary>
    /// Rotates the player towards their movement direction
    /// </summary>
    void RotateTowardsMovementDir()
    {
        // Make sure the player has movement input
        if (playerInput.moveInput != Vector3.zero)
        {
            // Slerp the player rotation to look at the movement direction
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(playerInput.moveInput), Time.deltaTime * rotationSpeed);
        }
    }

    /// <summary>
    /// Rotates the player to look at target position
    /// </summary>
    /// <param name="targetPosition"></param>
    void RotateTowardsTarget(Vector3 targetPosition)
    {
        // Calculate the target rotation based on the vector between the player position and target position
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - new Vector3(transform.position.x, 0, transform.position.z));
        // Set the player euler angles
        transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, (rotationSpeed * Time.deltaTime) * rotationSpeed);
    }
    #endregion

    #region Collision-Related Functionality
    public void SwitchCollisionOn()
    {
        // Enable movement
        canMove = true;
        // Enable character controller
        superCharacterController.enabled = true;
        // Stop applying root motion
        anim.applyRootMotion = false;
        // Make sure the rigidbody is kinematic
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    public void SwitchCollisionOff()
    {
        // Disable movement
        canMove = false;
        // Disable character controller
        superCharacterController.enabled = false;
        // Apply root motion
        anim.applyRootMotion = true;
        // Make sure rigidbody is set to being non kinematic
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }



    public IEnumerator _Knockback(Vector3 knockDirection, int knockBackAmount, int variableAmount)
    {
        isKnockback = true;
        StartCoroutine(_KnockbackForce(knockDirection, knockBackAmount, variableAmount));
        yield return new WaitForSeconds(.1f);
        isKnockback = false;
    }

    IEnumerator _KnockbackForce(Vector3 knockDirection, int knockBackAmount, int variableAmount)
    {
        while (isKnockback)
        {
            rb.AddForce(knockDirection * ((knockBackAmount + Random.Range(-variableAmount, variableAmount)) * (knockbackMultiplier * 10)), ForceMode.Impulse);
            yield return null;
        }
    }
    #endregion

    #region State-Related Functionality

    //Below are the state functions. Each one is called based on the name of the state, so when currentState = Idle, we call Idle_EnterState. If currentState = Jump, we call Jump_SuperUpdate()
    void Idle_EnterState()
    {
        superCharacterController.EnableSlopeLimit();
        superCharacterController.EnableClamping();
    }

    //Run every frame we are in the idle state.
    void Idle_SuperUpdate()
    {
        // Check if there is movement input and that the player can move
        if (playerInput.HasMoveInput() && canMove)
        {
            currentState = CharacterState.Move;
            playerCharacterState = CharacterState.Move;
            return;
        }
        //Apply friction to slow to a halt.
        currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, groundFriction * superCharacterController.deltaTime);
    }

    void Idle_ExitState()
    {
        //Run once when exit the idle state.
    }

    void Move_SuperUpdate()
    {
        // Check if there is movement input and that the player can move
        if (playerInput.HasMoveInput() && canMove)
        {
            //Keep strafing animations from playing.
            anim.SetFloat("Velocity X", 0F);

            /*
            // Check if the player is strafing or walking
            if (playerAnimationControl.isStrafing)
            {
                currentVelocity = Vector3.MoveTowards(currentVelocity, LocalMovement * walkSpeed, movementAcceleration * Time.deltaTime);
                // Check if we have an active item
                if (currentItem != CharacterItem.None)
                {
                    // Handle strafing
                    HandleStrafing(playerAnimationControl.target.transform.position);
                }
                if (rpgCharacterController.weapon != Weapon.RELAX)
                {
                    Strafing(rpgCharacterController.target.transform.position);
                }
                return;
            }
            */
            // Handle Running
            currentVelocity = Vector3.MoveTowards(currentVelocity, LocalMovement * runSpeed, movementAcceleration * superCharacterController.deltaTime);
        }
        // Otherwise, we are not moving
        else
        {
            // Go into the idle state
            currentState = CharacterState.Idle;
            playerCharacterState = CharacterState.Idle;
            return;
        }
    }
    #endregion

    public void TakeDamage(float amount)
    {
        // First off make sure we are alive
        if (isAlive && !damaged && canMove)
        {
            // Play damaged player sound
            FMODUnity.RuntimeManager.PlayOneShot(playerHurt, transform.position);

            // Decrement health
            HealthMeter.Health -= amount;
            damaged = true;

            
            //GameVars.instance.audioManager.PlaySFX(playerTakeDamage, 0.5f, gameObject.transform.position);
            // Check if we're below zero
            if (HealthMeter.Health < 0.0f)
            {
                // Play dead player sound
                FMODUnity.RuntimeManager.PlayOneShot(playerDie, transform.position);

                // Clamp the health at zero
                HealthMeter.Health = 0.0f;
                // Kill the player
                KillPlayer();
            }
        }
    }
    
    public void KillPlayer()
    {
        if (isAlive)
        {
            //GameVars.instance.audioManager.PlaySFX(playerDie, 0.5f, gameObject.transform.position);
            // Set isAlive to false
            isAlive = false;
            // Tell animator to make me the big dead
            anim.SetTrigger("Death1Trigger");
            // Disable input
            playerInput.allowedInput = false;
            // Send a message to the core game manager
            CoreGameManager.Instance.LoadGameOverScreen();
        }
    }

    public void CallFootsteps()
    {
        if (isMoving)
        {
            FMODUnity.RuntimeManager.PlayOneShot(inputSound);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIStateType { None, Idle, Pursuit, Attack, Stunned };
public enum AITriggerEventType { Enter, Stay, Exit }
public abstract class AIStateMachine : MonoBehaviour
{
    // Protected Variables
    protected AIState _currentState = null;
    protected Dictionary<AIStateType, AIState> _states = new Dictionary<AIStateType, AIState>();

    [SerializeField] PlayerControl _targetPlayer;
    protected int _rootPositionRefCount = 0;
    protected int _rootRotationRefCount = 0;
    protected bool _isTargetReached = false;

    // Animation Layer Manager
    protected Dictionary<string, bool> _animLayersActive = new Dictionary<string, bool>();

    // Protected Inspector Assigned Variables
    [SerializeField] protected AIStateType _currentStateType = AIStateType.Idle;
    [SerializeField] [Range(0.0f, 100.0f)] protected float _health = 10.0f;
    [SerializeField] GameObject deathEffectPrefab;

    [SerializeField]
    [Range(0, 15)]
    protected float _stoppingDistance = 1.0f;

    // Layered Audio Control
    //protected ILayeredAudioSource _layeredAudioSource = null;

    // Component Cache
    protected Animator _animator = null;
    protected UnityEngine.AI.NavMeshAgent _navAgent = null;
    protected Collider _collider = null;
    protected Transform _transform = null;

    [FMODUnity.EventRef]
    public string enemyDie = "";

    // Public Properties
    public bool isAlive { get; set; }
    public float Health { get { return _health; } set { _health = value; } }
    public bool isTargetReached { get { return _isTargetReached; } set { _isTargetReached = value; } }
    public AIStateType currentStateType { get { return _currentStateType; } }
    public bool canDamagePlayer = false;
    public bool inMeleeRange = false;
    public Animator animator { get { return _animator; } }
    public NavMeshAgent navAgent { get { return _navAgent; } }
    public bool useRootPosition { get { return _rootPositionRefCount > 0; } }
    public bool useRootRotation { get { return _rootRotationRefCount > 0; } }
    public PlayerControl targetPlayer { get { return _targetPlayer; } }
    public Transform targetPlayerTransform { get { return _targetPlayer.transform; } }
    public Vector3 targetPlayerPosition { get { return _targetPlayer.transform.position; } }
    public float targetDistance { get; set; }
    public bool isLeftMapEnemy { get; set; }

    public AudioClip aiTakeDamage;

    /// <summary>
    /// Caches all components of the state machine
    /// </summary>
    protected virtual void Awake()
    {
        //Debug.Log("AI State Machine Awake");
        // Cache all frequently accessed components
        _transform = transform;
        _animator = GetComponent<Animator>();
        _navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        _collider = GetComponent<Collider>();

        // Cache Audio Source Reference for Layered AI Audio
        AudioSource audioSource = GetComponent<AudioSource>();

        // Do we have a valid Game Scene Manager
        if (AIManager.Instance != null)
        {
            // Register State Machines with Scene Database
            if (_collider) AIManager.Instance.RegisterAIStateMachine(_collider.GetInstanceID(), this);
        }
    }


    // Start is called before the first frame update
    protected virtual void Start()
    {
        // Fetch all states on this game object
        AIState[] states = GetComponents<AIState>();

        // Loop through all states and add them to the state dictionary
        foreach (AIState state in states)
        {
            if (state != null && !_states.ContainsKey(state.GetStateType()))
            {
                // Add this state to the state dictionary
                _states[state.GetStateType()] = state;

                // And set the parent state machine of this state
                state.SetStateMachine(this);
            }
        }

        // Set the current state
        if (_states.ContainsKey(_currentStateType))
        {
            _currentState = _states[_currentStateType];
            _currentState.OnEnterState();
        }
        else
        {
            _currentState = null;
        }

        // Fetch all AIStateMachineLink derived behaviours from the animator and set their State Machine references to this state machine
        if (_animator)
        {
            AIStateMachineLink[] scripts = _animator.GetBehaviours<AIStateMachineLink>();
            foreach (AIStateMachineLink script in scripts)
            {
                script.stateMachine = this;
            }
        }
    }



    /// <summary>
    /// Called by Unity each frame, gives the current state a chance to update itself and perform transitions
    /// </summary>
    protected virtual void Update()
    {
        //Debug.Log("Root Position Ref Count "+_rootPositionRefCount);

        if (_currentState == null) return;

        AIStateType newStateType = _currentState.OnUpdate();
        if (newStateType != _currentStateType)
        {
            AIState newState = null;
            if (_states.TryGetValue(newStateType, out newState))
            {
                _currentState.OnExitState();
                newState.OnEnterState();
                _currentState = newState;
            }
            else
            if (_states.TryGetValue(AIStateType.Idle, out newState))
            {
                _currentState.OnExitState();
                newState.OnEnterState();
                _currentState = newState;
            }

            _currentStateType = newStateType;
        }
    }

    /// <summary>
    /// Clears the audio and visual threats with each tick of the Physics system
    /// Re-calculates distance to the current target
    /// </summary>
    protected virtual void FixedUpdate()
    {
        if (_targetPlayer && _targetPlayer.isAlive)
        {
            targetDistance = HelperMethods.QuickDistance(_transform.position, targetPlayerPosition);
        }

        _isTargetReached = false;
    }

    /// <summary>
    /// Allows any external method to force the AI out of its current state and into the specified state
    /// </summary>
    /// <param name="state"></param>
    public void SetStateOverride(AIStateType state)
    {
        // Set the current state
        if (state != _currentStateType && _states.ContainsKey(state))
        {
            if (_currentState != null)
                _currentState.OnExitState();

            _currentState = _states[state];
            _currentStateType = state;
            _currentState.OnEnterState();
        }
    }
    /// <summary>
    /// Sets an animation layer as active or inactive
    /// </summary>
    /// <param name="layerName"></param>
    /// <param name="active"></param>
    public void SetLayerActive(string layerName, bool active)
    {
        _animLayersActive[layerName] = active;
        /*
        if (active == false && _layeredAudioSource != null)
            _layeredAudioSource.Stop(_animator.GetLayerIndex(layerName));
        */
    }

    /// <summary>
    /// Checks whether the specified animation layer is active
    /// </summary>
    /// <param name="layerName"></param>
    /// <returns></returns>
    public bool IsLayerActive(string layerName)
    {
        bool result;
        if (_animLayersActive.TryGetValue(layerName, out result))
        {
            return result;
        }
        return false;
    }

    /// <summary>
    /// Called by Unity after root motion has been evaluated, but not yet applied to object
    /// Allows us to determine via code what to do with root motion information
    /// </summary>
    protected virtual void OnAnimatorMove()
    {
        if (_currentState != null)
            _currentState.OnAnimatorUpdated();
    }

    /// <summary>
    /// Called by Unity just before IK system is updated
    /// Gives us a change to setup IK Targets and weights
    /// </summary>
    /// <param name="layerIndex"></param>
    protected virtual void OnAnimatorIK(int layerIndex)
    {
        if (_currentState != null)
            _currentState.OnAnimatorIKUpdated();
    }

    /// <summary>
    /// Configures the NavMeshAgent to enable/disable auto updates of position/rotation to our transform
    /// </summary>
    /// <param name="positionUpdate"></param>
    /// <param name="rotationUpdate"></param>
    public void NavAgentControl(bool positionUpdate, bool rotationUpdate)
    {
        if (_navAgent)
        {
            _navAgent.updatePosition = positionUpdate;
            _navAgent.updateRotation = rotationUpdate;
        }
    }

    /// <summary>
    /// Called by the State Machine Behaviours to Enable/Disable root motion
    /// </summary>
    /// <param name="rootPosition"></param>
    /// <param name="rootRotation"></param>
    public void AddRootMotionRequest(int rootPosition, int rootRotation)
    {
        _rootPositionRefCount += rootPosition;
        _rootRotationRefCount += rootRotation;

        //Debug.Log("Adding Root Motion Request "+rootPosition+"   and    "+rootRotation);
    }


    public void AssignPlayerTarget(PlayerControl playerTarget)
    {
        if (!_targetPlayer)
        {
            _targetPlayer = playerTarget;
        }
    }
    
    public void TakeDamage(float _damage)
    {
        // First, make sure we are alive
        if (isAlive)
        {
            // Decrement health
            _health -= _damage;
            //GameVars.instance.audioManager.PlaySFX(aiTakeDamage, 0.5f, gameObject.transform.position);
            // Check if we're below 0 health
            if (_health <= 0.0f)
            {
                // Clamp health
                _health = 0.0f;
                // Kill the enemy
                Kill();
            }
        }
    }

    public void Kill()
    {
        FMODUnity.RuntimeManager.PlayOneShot(enemyDie);
        // Set isAlive to false
        isAlive = false;
        if (deathEffectPrefab)
        {
            // Instantiate the death effect
            GameObject deathEffect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        if (AIManager.Instance)
        {
            AIManager.Instance.UnregisterAIStateMachine(_collider.GetInstanceID());

            if (isLeftMapEnemy)
            {
                AIManager.Instance.leftSideEnemies.Remove(gameObject);
            }
            else
            {
                AIManager.Instance.rightSideEnemies.Remove(gameObject);
            }
        }
        // Destroy this game object
        Destroy(gameObject);
    }
}

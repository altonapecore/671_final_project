using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIWerewolfStateMachine : AIStateMachine
{
    // Inspector Assigned Variables
    [SerializeField] AIDamageTrigger _damageTrigger;
    [SerializeField] [Range(1.0f, 100.0f)] float _damage = 20.0f;
    // Private Variables
    private int _seeking = 0;
    private int _attackType = 0;
    private float _speed = 0.0f;
    private float _canDamage = 0.0f;
    // Animator Hashes
    private int _speedHash = Animator.StringToHash("Speed");
    private int _seekingHash = Animator.StringToHash("Seeking");
    private int _attackHash = Animator.StringToHash("Attack");
    private int _alarmingHash = Animator.StringToHash("Alarming");
    private int _alarmHash = Animator.StringToHash("Alarm");
    private int _stateHash = Animator.StringToHash("State");
    private int _canDamageHash = Animator.StringToHash("AttackDamage");
    private int _upperBodyLayer = -1;
    private int _lowerBodyLayer = -1;

    // Public Properties
    public AIDamageTrigger damageTrigger { get { return _damageTrigger; } }
    public float damage { get { return _damage; } set { _damage = value; } }
    public int attackType { get { return _attackType; } set { _attackType = value; } }
    public int seeking { get { return _seeking; } set { _seeking = value; } }
    public float speed
    {
        get { return _speed; }
        set { _speed = value; }
    }
    public float canDamageValue { get { return _animator.GetFloat(_canDamageHash); } }

    // Start is called before the first frame update
    void Start()
    {
        // Call base Start functionality
        base.Start();

        // Set IsAlive to true by default
        isAlive = true;

        // Set player in melee range to false by default
        canDamagePlayer = false;

        // Make sure animator is valid
        if (_animator != null)
        {
            // Cache Layer Indices
            _lowerBodyLayer = _animator.GetLayerIndex("Lower Body");
            _upperBodyLayer = _animator.GetLayerIndex("Upper Body");
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        // Make sure the animator is valid, and if so, update animator with state machine values
        if (_animator != null && isAlive)
        {
            _animator.SetFloat(_speedHash, _speed);
            _animator.SetInteger(_seekingHash, _seeking);
            _animator.SetInteger(_attackHash, _attackType);
            _animator.SetInteger(_stateHash, (int)_currentStateType);

            /*
            // Are we alarming or not
            _isAlarming= IsLayerActive("Cinematic") ? 0.0f : _animator.GetFloat(_alarmingHash);
            */

        }
    }
}

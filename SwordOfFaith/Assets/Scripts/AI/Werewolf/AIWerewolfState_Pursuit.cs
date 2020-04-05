using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIWerewolfState_Pursuit : AIWerewolfState
{
    // Inspector Assigned Variables
    [SerializeField] [Range(0, 10)] public float _speed = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _lookAtWeight = 0.7f;
    [SerializeField] [Range(0.0f, 90.0f)] float _lookAtAngleThreshold = 15.0f;
    [SerializeField] private float _slerpSpeed = 5.0f;
    [SerializeField] private float _repathDistanceMultiplier = 0.035f;
    [SerializeField] private float _repathMinDuration = 0.05f;
    [SerializeField] private float _repathMaxDuration = 5.0f;
    [SerializeField] private float _maxDuration = 40.0f;


    // Private Variables
    private float _timer = 0.0f;
    private float _repathTimer = 0.0f;
    private float _currentLookAtWeight = 0.0f;

    /// <summary>
    /// Returns the type of this state
    /// </summary>
    /// <returns></returns>
    public override AIStateType GetStateType()
    {
        return AIStateType.Pursuit;
    }

    /// <summary>
    /// Initializes state machine for Pursuit state
    /// </summary>
    public override void OnEnterState()
    {
        base.OnEnterState();
        if (_werewolfStateMachine == null)
            return;

        //Debug.Log("Entered pursuit");
        // Configure State Machine
        _werewolfStateMachine.NavAgentControl(true, false);
        _werewolfStateMachine.seeking = 0;
        _werewolfStateMachine.attackType = 0;

        // Guards will only pursue for so long before breaking off
        _timer = 0.0f;
        _repathTimer = 0.0f;


        // Set path
        _werewolfStateMachine.navAgent.SetDestination(_werewolfStateMachine.targetPlayerPosition);
        _werewolfStateMachine.navAgent.isStopped = false;

        _currentLookAtWeight = 0.0f;
    }

    // Update is called once per frame
    public override AIStateType OnUpdate()
    {
        _timer += Time.deltaTime;
        _repathTimer += Time.deltaTime;

        if (!_werewolfStateMachine) return AIStateType.Idle;

        // IF we are chasing the player and have entered the melee trigger then attack
        if (_werewolfStateMachine.targetPlayer && _werewolfStateMachine.inMeleeRange)
        {
            //Debug.Log("Going into attack state");
            return AIStateType.Attack;
        }

        if (_werewolfStateMachine.navAgent.pathPending)
            _werewolfStateMachine.speed = 0;
        else
        {
            //Debug.Log("Reached here");
            _werewolfStateMachine.speed = _speed;
            /*
            // If we are close to the target that was a player and we still have the player in our vision then keep facing right at the player
            if (!_werewolfStateMachine.useRootRotation && _werewolfStateMachine.targetPlayer)
            {
                Vector3 targetPos = _werewolfStateMachine.targetPlayerPosition;
                targetPos.y = _werewolfStateMachine.transform.position.y;
                Quaternion newRot = Quaternion.LookRotation(targetPos - _werewolfStateMachine.transform.position);
                _werewolfStateMachine.transform.rotation = newRot;

            }
            */
            // Slowly update our rotation to match the nav agents desired rotation BUT only if we are not pursuing the player and are really close to them
            if (!_stateMachine.useRootRotation)
            {
                // Generate a new Quaternion representing the rotation we should have
                Quaternion newRot = Quaternion.LookRotation(_werewolfStateMachine.navAgent.desiredVelocity);

                // Smoothly rotate to that new rotation over time
                _werewolfStateMachine.transform.rotation = Quaternion.Slerp(_werewolfStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
            }
        }

        

        // Do we have a visual threat that is the player
        if (_werewolfStateMachine.targetPlayer && _werewolfStateMachine.targetPlayer.isAlive)
        {
            _werewolfStateMachine.navAgent.SetDestination(_werewolfStateMachine.targetPlayerPosition);
            _repathTimer = 0.0f;
            /*
            float targetDistanceRepathModifier = _werewolfStateMachine.targetDistance * _repathMaxDuration;
            float repathProgress = Mathf.Clamp(targetDistanceRepathModifier, _repathMinDuration, _repathMaxDuration);
            // Repath more frequently as we get closer to the target (try and save some CPU cycles)
            if (targetDistanceRepathModifier < _repathTimer)
            {
                Debug.Log("Repathing agent");
                // Repath the agent
                _werewolfStateMachine.navAgent.SetDestination(_werewolfStateMachine.targetPlayerPosition);
                _repathTimer = 0.0f;
            }
            */
            // Remain in pursuit state
            return AIStateType.Pursuit;
        }

        // By default, remain in pursuit state
        return AIStateType.Pursuit;
    }

    public override void OnAnimatorIKUpdated()
    {
        if (_werewolfStateMachine == null)
            return;

        if (Vector3.Angle(_werewolfStateMachine.transform.forward, _werewolfStateMachine.targetPlayerPosition - _werewolfStateMachine.transform.position) < _lookAtAngleThreshold)
        {
            _werewolfStateMachine.animator.SetLookAtPosition(_werewolfStateMachine.targetPlayerPosition + Vector3.up);
            _currentLookAtWeight = Mathf.Lerp(_currentLookAtWeight, _lookAtWeight, Time.deltaTime);
            _werewolfStateMachine.animator.SetLookAtWeight(_currentLookAtWeight);
        }
        else
        {
            _currentLookAtWeight = Mathf.Lerp(_currentLookAtWeight, 0.0f, Time.deltaTime);
            _werewolfStateMachine.animator.SetLookAtWeight(_currentLookAtWeight);
        }
    }
}

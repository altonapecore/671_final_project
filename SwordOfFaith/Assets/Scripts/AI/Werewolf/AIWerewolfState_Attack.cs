using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIWerewolfState_Attack : AIWerewolfState
{
    // Inspector Assigned Variables
    [SerializeField] [Range(0, 10)] public float _speed = 0.0f;
    [SerializeField] float _stoppingDistance = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _lookAtWeight = 0.7f;
    [SerializeField] [Range(0.0f, 90.0f)] float _lookAtAngleThreshold = 15.0f;
    [SerializeField] float _slerpSpeed = 5.0f;

    // Private Variables
    private float _currentLookAtWeight = 0.0f;

    /// <summary>
    /// Returns the type of this state
    /// </summary>
    /// <returns></returns>
    public override AIStateType GetStateType()
    {
        return AIStateType.Attack;
    }

    // Default Handlers
    public override void OnEnterState()
    {
        base.OnEnterState();
        if (base._werewolfStateMachine == null)
            return;

        // Configure State Machine
        _werewolfStateMachine.NavAgentControl(true, false);
        _werewolfStateMachine.attackType = Random.Range(1, 75);
        _werewolfStateMachine.speed = _speed;
        _currentLookAtWeight = 0.0f;
    }

    public override void OnExitState()
    {
        base._werewolfStateMachine.attackType = 0;
    }

    public override AIStateType OnUpdate()
    {
        Vector3 targetPos;
        Quaternion newRot;

        if (HelperMethods.QuickDistance(_werewolfStateMachine.transform.position, _werewolfStateMachine.targetPlayerPosition) < _stoppingDistance)
        {
            _werewolfStateMachine.speed = 0;
        }
        else
        {
            _werewolfStateMachine.speed = _speed;
        }

        // Check if we have a valid player
        if (_werewolfStateMachine.targetPlayer && _speed != 0)
        {
            // Check if that player is alive
            if (_werewolfStateMachine.targetPlayer.isAlive)
            {
                // If we are not in melee range any more than fo back to pursuit mode
                if (!_werewolfStateMachine.inMeleeRange)
                {
                    return AIStateType.Pursuit;
                }

                if (_werewolfStateMachine.canDamagePlayer && _werewolfStateMachine.canDamageValue > 0.9f)
                {
                    // Play werewolf hit sound
                    FMODUnity.RuntimeManager.PlayOneShot("Enemy Sounds/Enemy Swing and Hit");

                    _werewolfStateMachine.targetPlayer.TakeDamage(_werewolfStateMachine.damage);
                }
       

                if (!_werewolfStateMachine.useRootRotation)
                {
                    // Keep the werewolf facing the player at all times
                    targetPos = _werewolfStateMachine.targetPlayerPosition;
                    targetPos.y = _werewolfStateMachine.transform.position.y;
                    newRot = Quaternion.LookRotation(targetPos - _werewolfStateMachine.transform.position);
                    _werewolfStateMachine.transform.rotation = Quaternion.Slerp(_werewolfStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
                }

                _werewolfStateMachine.attackType = Random.Range(1, 100);

                return AIStateType.Attack;
            }
            else
            {
                // Go into Idle
                return AIStateType.Idle;
            }
        }
        else
        {
            return AIStateType.Idle;
        }
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

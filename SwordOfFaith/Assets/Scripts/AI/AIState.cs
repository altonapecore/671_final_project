using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIState : MonoBehaviour
{
    // Public Method
    // Called by the parent state machine to assign its reference
    public virtual void SetStateMachine(AIStateMachine stateMachine) { _stateMachine = stateMachine; }

    // Default Handlers
    public virtual void OnEnterState() { }
    public virtual void OnExitState() { }
    public virtual void OnAnimatorIKUpdated() { }
    public virtual void OnTriggerEvent(AITriggerEventType eventType, Collider other) { }
    public virtual void OnDestinationReached(bool isReached) { }

    // Abtract Methods
    public abstract AIStateType GetStateType();
    public abstract AIStateType OnUpdate();

    // Protected Fields
    protected AIStateMachine _stateMachine;
    public virtual void OnAnimatorUpdated()
    {
        // Get the number of meters the root motion has updated for this update and
        // divide by deltaTime to get meters per second. We then assign this to
        // the nav agent's velocity.
        if (Time.deltaTime > 0 && _stateMachine.useRootPosition)
        {
            _stateMachine.navAgent.velocity = _stateMachine.animator.deltaPosition / Time.deltaTime;
        }


        // Grab the root rotation from the animator and assign as our transform's rotation.
        if (_stateMachine.useRootRotation)
            _stateMachine.transform.rotation = _stateMachine.animator.rootRotation;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIWerewolfState : AIState
{
    // Private Variables
    protected int _playerLayerMask;
    protected AIWerewolfStateMachine _werewolfStateMachine = null;
    // Properties
    public AIWerewolfStateMachine werewolfStateMachine { get { return _werewolfStateMachine; } }

    /// <summary>
    /// Checks for type compliance and stores the reference as the derived type
    /// </summary>
    /// <param name="stateMachine"></param>
    public override void SetStateMachine(AIStateMachine stateMachine)
    {
        if (stateMachine.GetType() == typeof(AIWerewolfStateMachine))
        {

            base.SetStateMachine(stateMachine);
            _werewolfStateMachine = (AIWerewolfStateMachine)stateMachine;

        }
    }
}

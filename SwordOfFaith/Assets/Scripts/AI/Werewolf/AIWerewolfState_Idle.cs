using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIWerewolfState_Idle : AIWerewolfState
{

    /// <summary>
    /// Returns the type of the state
    /// </summary>
    /// <returns></returns>
    public override AIStateType GetStateType()
    {
        return AIStateType.Idle;
    }
    /// <summary>
    /// Initializes state machine for Idle state
    /// </summary>
    public override void OnEnterState()
    {
        base.OnEnterState();
        //Debug.Log("Entered idle");
        // Make sure the state machine is valid
        if (_werewolfStateMachine == null)
            return;
        // Configure state machine
        _werewolfStateMachine.NavAgentControl(true, false);
        _werewolfStateMachine.speed = 0;
        _werewolfStateMachine.seeking = 0;
        _werewolfStateMachine.attackType = 0;
    }

    /// <summary>
    /// Essentially the engine of the state, performs all checks on each update frame
    /// </summary>
    /// <returns></returns>
    public override AIStateType OnUpdate()
    {
        if(_werewolfStateMachine.speed != 0)
        {
            // Make sure there is a valid state machine
            if (_werewolfStateMachine == null)
                return AIStateType.Idle;

            // Check if there is still a player target
            if (_werewolfStateMachine.targetPlayer && _werewolfStateMachine.targetPlayer.isAlive)
            {
                // Put Werewolf into Pursuit State
                return AIStateType.Pursuit;
            }
        }
        // By default, keep Werewolf in Idle State
        return AIStateType.Idle;

    }
}

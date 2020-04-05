using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMeleeZoneTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider col)
    {
        // Get the AIStateMachine
        AIStateMachine _stateMachine = AIManager.Instance.GetAIStateMachine(col.GetInstanceID());
        //AIStateMachine _stateMachine = col.gameObject.GetComponent<AIStateMachine>();
        // Make sure the state machine is valid
        if (_stateMachine)
        {
            if (_stateMachine.isAlive)
            {
                _stateMachine.inMeleeRange = true;
                //_guardStateMachine.SetStateOverride(AIStateType.Attack);
                //PlayerVars.instance.ResetToCheckPoint(10);
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        // Get the AIStateMachine
        //AIStateMachine _stateMachine = col.gameObject.GetComponent<AIStateMachine>();
        AIStateMachine _stateMachine = AIManager.Instance.GetAIStateMachine(col.GetInstanceID());
        // Make sure the state machine is valid
        if (_stateMachine)
        {
            if (_stateMachine.isAlive)
            {
                _stateMachine.inMeleeRange = false;
                //_guardStateMachine.SetStateOverride(AIStateType.Attack);
                //PlayerVars.instance.ResetToCheckPoint(10);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDamageTrigger : MonoBehaviour
{
    // Inspector Variables
    [SerializeField] AIStateMachine _stateMachine = null;
    [SerializeField] string _parameter = "AttackDamage";
    [SerializeField] float _damageAmount = 20.0f;

    // Private Variables
    int _parameterHash = -1;
    private bool _firstContact = false;

    // Properties
    public bool firstContact { get { return _firstContact; } set { _firstContact = false; } }

    void OnTriggerEnter(Collider col)
    {
        if (!_stateMachine.isAlive)
            return;

        if (col.gameObject.CompareTag("Player") && !_stateMachine.canDamagePlayer)
        {
            _stateMachine.canDamagePlayer = true;
            _firstContact = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && _stateMachine.canDamagePlayer)
        {
            _stateMachine.canDamagePlayer = false;
        }
    }
}

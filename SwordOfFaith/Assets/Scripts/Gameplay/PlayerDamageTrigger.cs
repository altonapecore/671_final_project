using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamageTrigger : MonoBehaviour
{
    // Inspector Assigned Variables
    [SerializeField] PlayerControl _playerControl;

    // Public Variables
    public List<AIStateMachine> currentIntersectingEnemies;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (AIManager.Instance)
        {
            // Try to get an AI State Machine from the AI Manager
            AIStateMachine _stateMachine = AIManager.Instance.GetAIStateMachine(other.GetInstanceID());
            // Make sure this state machine is valid, and that the enemy is alive
            if (_stateMachine && _stateMachine.isAlive)
            {
                //Debug.Log("Found an enemy, about to add them to the list");
                // Check if we already contain this state machine
                if (!currentIntersectingEnemies.Contains(_stateMachine))
                {
                    currentIntersectingEnemies.Add(_stateMachine);
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (AIManager.Instance)
        {
            if (currentIntersectingEnemies.Count > 0)
            {
                // Try to get an AI State Machine from the AI Manager
                AIStateMachine _stateMachine = AIManager.Instance.GetAIStateMachine(other.GetInstanceID());
                if (currentIntersectingEnemies.Contains(_stateMachine))
                {
                    // Do damage to the enemy
                    _stateMachine.TakeDamage(_playerControl.Damage);
                    // Remove it from the list
                    currentIntersectingEnemies.Remove(_stateMachine);
                    // Check if we're back at 0
                    if (currentIntersectingEnemies.Count == 0)
                    {
                        // Disable this game object
                        gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}

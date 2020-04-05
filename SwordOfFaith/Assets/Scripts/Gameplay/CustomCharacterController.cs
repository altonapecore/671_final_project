using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCharacterController : MonoBehaviour
{
    // Inspector Assigned Variables
    [SerializeField] Vector3 debugMove = Vector3.zero;
    [SerializeField] QueryTriggerInteraction triggerInteraction;
    [SerializeField] bool fixedTimeStep;
    [SerializeField] int fixedUpdatesPerSecond;
    // Public Variables
    public LayerMask WalkableLayer;
    // Properties
    public float deltaTime { get; private set; }
}

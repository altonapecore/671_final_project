using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class PlayerInputMap
{
    public string horizontalAxis = "Horizontal1";
    public string verticalAxis = "Vertical1";
    public KeyCode actionKey;
    public KeyCode useKey;
    public List<KeyCode> eventKeys;
}

public class PlayerInput : MonoBehaviour
{
    // Public Variables
    public PlayerInputMap playerInputMap;
    [HideInInspector] public bool inputAttack;
    [HideInInspector] public bool inputSwitchItem;
    [HideInInspector] public bool inputUse;
    [HideInInspector] public float inputAimVertical = 0;
    [HideInInspector] public float inputAimHorizontal = 0;
    [HideInInspector] public float inputHorizontal = 0;
    [HideInInspector] public float inputVertical = 0;
    [HideInInspector] public bool inputStrafe;
    [HideInInspector] public bool inputRoll;
    [HideInInspector] public bool allowedInput = true;
    [HideInInspector] public Vector3 moveInput;
    [HideInInspector] public Vector2 aimInput;
    // Private Variables
    private PlayerControl playerControl;

    private void Awake()
    {
        // Allow input by default
        allowedInput = true;
        // Cache player control variable
        playerControl = GetComponent<PlayerControl>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Cache player control variable
        playerControl = GetComponent<PlayerControl>();
    }

    // Update is called once per frame
    void Update()
    {
        // First off, check if input is allowed
        if (allowedInput)
        {
            // Check inputs
            CheckInputs();
            // Get movement relative to camera
            moveInput = CameraRelativeInput(inputHorizontal, inputVertical);
            //Debug.Log("Current Move Input: " + moveInput.ToString());
        }
    }

    /// <summary>
    /// Uses the player input mapping to check various player inputs and set booleans
    /// </summary>
    void CheckInputs()
    {
        // Check for movement input
        inputHorizontal = Input.GetAxisRaw(playerInputMap.horizontalAxis);
        //Debug.Log("Input Horizontal: " + inputHorizontal);
        inputVertical = Input.GetAxisRaw(playerInputMap.verticalAxis);
        //Debug.Log("Input Vertical: " + inputVertical);
        // Check for attack and use input
        inputAttack = Input.GetKeyDown(playerInputMap.actionKey);
        inputUse = Input.GetKeyDown(playerInputMap.useKey);
    }

    /// <summary>
    /// Movement based off camera facing.
    /// </summary>
    Vector3 CameraRelativeInput(float inputX, float inputZ)
    {
        //Forward vector relative to the camera along the x-z plane   
        Vector3 forward = playerControl.CameraTransform.TransformDirection(Vector3.forward);
        //Debug.Log("Forward: " + forward.ToString());
        forward.y = 0;
        forward = forward.normalized;
        //Right vector relative to the camera always orthogonal to the forward vector.
        Vector3 right = new Vector3(forward.z, 0, -forward.x);
        Vector3 relativeVelocity = inputHorizontal * right + inputVertical * forward;
        //Reduce input for diagonal movement.
        if (relativeVelocity.magnitude > 1)
        {
            relativeVelocity.Normalize();
        }
        return relativeVelocity;
    }

    public bool HasAnyInput()
    {
        if (allowedInput && moveInput != Vector3.zero)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool HasMoveInput()
    {
        if (allowedInput && moveInput != Vector3.zero)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

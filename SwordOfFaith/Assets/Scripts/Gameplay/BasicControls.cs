using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicControls : MonoBehaviour
{
    public KeyCode MOVE_UP, MOVE_DOWN, MOVE_LEFT, MOVE_RIGHT;
    public float playerSpeed = 8;
    public Rigidbody rigidbody;
    public bool isMovable = true;

    private Quaternion initialRotation;

    private void Start()
    {
        initialRotation = transform.rotation;

        if(rigidbody == null)
        {
            rigidbody.GetComponent<Rigidbody>();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        transform.rotation = initialRotation;
        if (isMovable)
        {
            Vector3 currentDirection = Vector3.zero;
            if (Input.GetKey(MOVE_UP))
            {
                currentDirection += transform.forward;
            }

            if (Input.GetKey(MOVE_DOWN))
            {
                currentDirection += -transform.forward;
            }

            if (Input.GetKey(MOVE_RIGHT))
            {
                currentDirection += transform.right;
            }

            if (Input.GetKey(MOVE_LEFT))
            {
                currentDirection += -transform.right;
            }

            if (rigidbody.velocity.magnitude < playerSpeed)
            {
                rigidbody.velocity = currentDirection.normalized * playerSpeed;
            }
        }
    }
}

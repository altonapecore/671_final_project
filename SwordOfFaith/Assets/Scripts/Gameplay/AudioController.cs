using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string inputSound;
    bool playerIsMoving;
    float velX;
    float velZ;

    Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
        velX = anim.GetFloat("Velocity X");
        velZ = anim.GetFloat("Velocity Z");

        InvokeRepeating("CallFootsteps", 0, 0.5f);
    }

    private void Update()
    {
        if(velX >= 0.1f || velZ >= 0.1f || velX <= -0.1 || velZ <= -0.1)
        {
            playerIsMoving = true;
        }

        else if(velX == 0 || velZ == 0)
        {
            playerIsMoving = false;
        }

    }

    void CallFootsteps()
    {
        if (playerIsMoving)
        {
            FMODUnity.RuntimeManager.PlayOneShot(inputSound);
        }
    }

    

    private void OnDisable()
    {
        playerIsMoving = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItemControl : MonoBehaviour
{
    // Public Variables
    [HideInInspector] public PlayerControl playerControl;
    [HideInInspector] public PlayerInput playerInput;
    [HideInInspector] public Animator anim;
    public GameObject swordObject;

    private void Awake()
    {
        // Cache required components
        playerControl = GetComponent<PlayerControl>();
        playerInput = GetComponent<PlayerInput>();
        anim = GetComponent<Animator>();
        // Set the default item to the animator
        anim.SetInteger("Weapon", 1);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

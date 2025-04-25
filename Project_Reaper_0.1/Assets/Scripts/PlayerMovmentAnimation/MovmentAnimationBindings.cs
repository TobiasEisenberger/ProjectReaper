using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovmentAnimationBindings : MonoBehaviour
{
    private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //Moving Forward Animation
        if (Input.GetKeyDown(KeyCode.W))
        {
            animator.SetBool("runningForward", true);
        }

        if (Input.GetKeyUp(KeyCode.W))
        {
            animator.SetBool("runningForward", false);
        }

        //Moving Backward Animation
        if (Input.GetKeyDown(KeyCode.S))
        {
            animator.SetBool("runningBackward", true);
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            animator.SetBool("runningBackward", false);
        }

        //Moving Left Side
        if (Input.GetKeyDown(KeyCode.A))
        {
            animator.SetBool("Left", true);
        }

        if (Input.GetKeyUp(KeyCode.A))
        {
            animator.SetBool("Left", false);
        }

        //Moving Right
        if (Input.GetKeyDown(KeyCode.D))
        {
            animator.SetBool("Right", true);
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            animator.SetBool("Right", false);
        }

        //Jumping Animation
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetBool("jump", true);
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            animator.SetBool("jump", false);
        }

        //Sprinting Animation
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            animator.SetBool("sprint", true);
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            animator.SetBool("sprint", false);
        }

    }

    
}

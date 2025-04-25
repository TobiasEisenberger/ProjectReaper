using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BevelController : MonoBehaviour
{
    public bool isOn;
    public Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void togglBevel()
    {
        if (!isOn)
        {
            isOn = true;
            Debug.Log("Schalter aktiviert");
            animator.SetBool("IsOn", isOn);
        }
        else
        {
            isOn=false;
            Debug.Log("Schalter NICHT aktiviert");
            animator.SetBool("IsOn", isOn);
        }
    }
}

using UnityEngine;

public class MovementAnimationBindings : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerController controller;
    private bool[] old_inputs;

    // Start is called before the first frame update
    private void Start()
    {
        animator = GetComponent<Animator>();
        Debug.Log($"animator gefunden{animator}");
    }

    // Update is called once per frame
    private void Update()
    {
        old_inputs = controller.Inputs;

        animator.SetBool("runningForward", controller.Inputs[0]);
        Debug.Log($"lauf nach vorne{controller.Inputs[0]}");
        animator.SetBool("runningBackward", controller.Inputs[1]);
        Debug.Log($"lauf nach vorne{controller.Inputs[1]}");
    }

    //public void AnimateInputs()
    //{
    //    animator.SetBool("runningForward", controller.Inputs[0]);
    //    Debug.Log($"lauf nach vorne{controller.Inputs[0]}");
    //    animator.SetBool("runningBackward", controller.Inputs[1]);
    //    Debug.Log($"lauf nach vorne{controller.Inputs[1]}");
    //}
}
using UnityEngine;

public class SpikeBallTrap : Trap
{
    private float speed = 80f;
    private Quaternion targetRotation;

    private void Start()
    {
        targetRotation = Quaternion.Euler(transform.localRotation.x, transform.localRotation.y, 40);
    }

    private void Update()
    {
        if (isActive)
        {

            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, speed * Time.deltaTime);
            if (transform.localRotation == targetRotation)
                targetRotation = Quaternion.Inverse(transform.localRotation);
        }
    }
}
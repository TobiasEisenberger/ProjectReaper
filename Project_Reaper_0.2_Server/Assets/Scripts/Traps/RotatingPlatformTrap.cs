using UnityEngine;

public class RotatingPlatformTrap : Trap
{
    [SerializeField]
    private float rotationAngle = 90f;

    private void Update()
    {
        if (isActive)
        {
            transform.Rotate(Vector3.up, rotationAngle);
            rotationAngle *= -1;
            deactivate();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSpeedBoost : MonoBehaviour
{
    private PlayerMovement playerMovementScript;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerMovement>(out playerMovementScript))
            {
                playerMovementScript.ActivateBoostedSprintSpeed();
            }
            
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerMovement>(out playerMovementScript))
            {
                playerMovementScript.DeactivateBoostedSprintSpeed();
            }
            
        }
    }
}

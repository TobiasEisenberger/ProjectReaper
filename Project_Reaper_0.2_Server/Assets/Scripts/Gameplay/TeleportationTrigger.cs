using UnityEngine;

public class TeleportationTrigger : MonoBehaviour
{
    [SerializeField] private Transform targetLocation;
    [SerializeField] private bool isPortal;
    [SerializeField] private GameObject otherPortal;
    private PlayerMovement playerMovementScript;
    private Quaternion portalRelationship;

    private void Start()
    {
        if (isPortal)
        {
            portalRelationship = GetPortalRelationship(transform.rotation, otherPortal.transform.rotation);
        }
    }

    // Function to calculate portalRelationship
    private Quaternion GetPortalRelationship(Quaternion portalA, Quaternion portalB)
    {
        // Calculate the relative rotation axis
        Vector3 axis = Vector3.Cross(portalA * Vector3.forward, portalB * Vector3.forward);

        // Handle potential issue with near-zero axis vectors (avoid division by zero)
        if (axis.sqrMagnitude < Mathf.Epsilon)
        {
            // In this case, portals might be facing the same direction (identity rotation)
            return Quaternion.identity;
        }
        // Normalize the axis
        axis.Normalize();

        // Calculate the angle of rotation
        float angle = Quaternion.Angle(portalA, portalB);

        // Combine axis and angle to create the relative rotation
        return Quaternion.AngleAxis(angle, axis);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player touched teleportation trigger");
            playerMovementScript = other.GetComponent<PlayerMovement>();
            if (playerMovementScript != null)
            {
                if (isPortal)
                {
                    targetLocation = otherPortal.transform.GetChild(0); 
                    AlignPlayerOrientation();
                    ChangeVelocity();
                }
                if (targetLocation != null)
                {
                    playerMovementScript.Teleport(targetLocation.transform.position);
                }
            }
            else
            {
                Debug.Log("OnTriggerEnter: No movement component found");
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player touched teleportation trigger");
            playerMovementScript = other.GetComponent<PlayerMovement>();
            if (playerMovementScript != null)
            {
                if (isPortal)
                {
                    targetLocation = otherPortal.transform.GetChild(0);
                    AlignPlayerOrientation();
                    ChangeVelocity();
                }
                if (targetLocation != null)
                {
                    playerMovementScript.Teleport(targetLocation.transform.position);
                }
            }
            else
            {
                Debug.Log("OnTriggerEnter: No movement component found");
            }
        }
    }

    private void ChangeVelocity()
    {
        Vector3 relativeVelocity = transform.InverseTransformDirection(playerMovementScript.RB.velocity);
        relativeVelocity = portalRelationship * relativeVelocity; // Transform velocity
        // Optionally adjust velocity direction to match exit portal forward
        relativeVelocity = Quaternion.Inverse(portalRelationship) * relativeVelocity;

        playerMovementScript.RB.velocity = transform.TransformDirection(relativeVelocity);
    }

    private void AlignPlayerOrientation()
    {
        // Get the desired forward direction based on the exit portal
        Vector3 newForward = otherPortal.transform.forward;
        // Adjust for 90-degree offset (modify as needed for left or right)
        newForward = Quaternion.Euler(0, -90f, 0) * newForward;
        // Calculate the rotation difference between current and desired orientation
        playerMovementScript.CamProxy.transform.forward = newForward;
        Debug.Log("Sending orient");
        playerMovementScript.SendNewCameraRotation();
    }
}
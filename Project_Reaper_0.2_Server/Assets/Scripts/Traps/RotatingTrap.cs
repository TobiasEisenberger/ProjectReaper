using UnityEngine;

public class RotatingTrap : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovementScript;
    [SerializeField] private Rigidbody trapRB;
    public float rotationSpeed = 2.5f;
    public float launchForce = 100f;
    private Vector3 forceDirection;

    // Start is called before the first frame update
    private void Start()
    {
        trapRB = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        trapRB.angularVelocity = Vector3.up * rotationSpeed;
    }

    private void HandleCollision()
    {
        Debug.Log("collsion player");
        Vector3 appliedForce = forceDirection * launchForce;
        // limit upward force when player jumps on top of the spinning trap
        if (appliedForce.y >= 5)
            appliedForce.y = 5;
        playerMovementScript.RB.AddForce(appliedForce, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Debug.Log("collsion onCollsionEnter");
            playerMovementScript = collision.collider.GetComponent<PlayerMovement>();
            if (playerMovementScript != null)
            {
                forceDirection = (collision.impulse / Time.fixedDeltaTime).normalized;
                Debug.Log($"Collision Impulse:{collision.impulse} Normalized Force Direction:{forceDirection}");
                HandleCollision();
            }
            else
                Debug.Log("on collsion enter no movement component found");
        }
    }
}
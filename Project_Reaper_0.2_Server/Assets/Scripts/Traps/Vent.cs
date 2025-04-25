using UnityEngine;

public class Vent : Trap
{
    [SerializeField] private PlayerMovement playerMovementScript;
    [SerializeField] private Rigidbody trapRB;
    [SerializeField] private float defaultRotationSpeed = 2.5f;
    private float activatedRotationSpeed;
    private float rotationSpeed;
    [SerializeField] private float defaultForce = 50f;
    [SerializeField] private float activatedForceMultiplier = 2f;
    [SerializeField] private bool isActiveByDefault = false;
    [SerializeField] private float activationDuration = 5f; // Time in seconds the trap stays active
    private float activationTimer = 0f;
    [SerializeField] private float cooldownDuration = 10f; // Time in seconds before trap can be reactivated
    private bool isOnCooldown = false;
    [SerializeField] bool useDistanceBasedForce = true;
    [SerializeField] float maxForceDistance = 20f;

    // Start is called before the first frame update
    private void Start()
    {
        trapRB = GetComponent<Rigidbody>();
        rotationSpeed = defaultRotationSpeed;
        activatedRotationSpeed = defaultRotationSpeed * activatedForceMultiplier;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (isActive || isActiveByDefault)
        {
            if (isActive)
            {
                activationTimer += Time.deltaTime;

                rotationSpeed = activatedRotationSpeed;
                if (activationTimer >= activationDuration)
                {
                    deactivate();
                }
            }
            else
            {
                rotationSpeed = defaultRotationSpeed;
            }
            trapRB.angularVelocity = transform.up * rotationSpeed;
        }
        else
        {
            trapRB.angularVelocity = transform.up * 0;
        }
    }

    public override void activate()
    {
        Debug.Log($"isOnCooldownActivate: {isOnCooldown}");
        if (!isOnCooldown)
            base.activate();
    }

    public override void deactivate()
    {
        base.deactivate();
        isOnCooldown = true;
        activationTimer = 0f;
        Invoke(nameof(ResetTrap), cooldownDuration);
    }

    private void ResetTrap()
    {
        Debug.Log("CooldownOver");
        isOnCooldown = false;
    }

    private void ApplyForceToPlayer(Collider playerCollider)
    {
        playerMovementScript = playerCollider.GetComponent<PlayerMovement>();
        if (playerMovementScript != null)
        {
            float distance = Vector3.Distance(transform.position, playerCollider.transform.position);
            float forceMultiplier = isActive ? activatedForceMultiplier : 1;
            float forceToApply;
            if ( useDistanceBasedForce)
            {
                forceToApply = Mathf.Lerp(defaultForce * forceMultiplier, 0, distance / maxForceDistance);
            }
            else
            {
                forceToApply = defaultForce * forceMultiplier;
            }      
            playerMovementScript.RB.AddForce(transform.up * forceToApply, ForceMode.Force);
        }
        else
        {
            Debug.LogWarning("Vent no player movement found");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActive || isActiveByDefault)
        {
            if (other.CompareTag("Player"))
            {
                ApplyForceToPlayer(other);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isActive || isActiveByDefault)
        {
            if (other.CompareTag("Player"))
            {
                ApplyForceToPlayer(other);
            }
        }
    }
}
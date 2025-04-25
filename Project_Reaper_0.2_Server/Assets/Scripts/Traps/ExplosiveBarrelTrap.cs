using UnityEngine;

public class ExplosiveBarrelTrap : Trap
{
    [SerializeField]
    private BoxCollider playerRangeHitbox;

    [SerializeField]
    private float explosionForce = 100.0f;

    [SerializeField] private bool IsDynamicBarrel;
    private Rigidbody rb;

    private void Start()
    {
        if (IsDynamicBarrel)
            rb = GetComponent<Rigidbody>();
    }

    public override void activate()
    {
        base.activate();
        if (rb != null)
            rb.isKinematic = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            SpawnDynamicObjects.DisableObject(gameObject);
            gameObject.SetActive(false);
            //Destroy(gameObject);

            if (playerRangeHitbox != null)
                HandlePlayerKnockback();
        }
    }

    private void HandlePlayerKnockback()
    {
        Collider[] colliders = Physics.OverlapBox(playerRangeHitbox.bounds.center, playerRangeHitbox.bounds.extents);
        Debug.Log($"Found {colliders.Length} colliders");
        foreach (Collider hit in colliders)
        {
            var player = hit.GetComponent<Player>();
            if (hit.gameObject.CompareTag("Player") && !player.isReaper)
            {
                if (hit.TryGetComponent<PlayerMovement>(out PlayerMovement playerMovementScript))
                {
                    playerMovementScript.RB.AddExplosionForce(explosionForce, transform.position, playerRangeHitbox.bounds.extents.magnitude, 1f, ForceMode.Impulse);
                }
            }
        }
    }
}
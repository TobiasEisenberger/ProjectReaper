using UnityEngine;

public class FlySwatterTrap : Trap
{
    private float speed = 30f;
    private Quaternion targetRotation;

    [SerializeField] private int damage;

    private void Start()
    {
        targetRotation = Quaternion.Euler(0, 0, -90);
    }

    private void Update()
    {
        if (isActive)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, speed * Time.deltaTime);

            if (transform.rotation == targetRotation)
                deactivate();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isActive)
        {
            Debug.Log("Collision with Fly Swatter Trap triggered");
            if (collision.gameObject.GetComponent<Player>())
            {
                if (collision.gameObject.GetComponent<Player>().isReaper)
                    return;
                if (collision.gameObject.GetComponent<Player>().IsInDmgCooldown)
                    return;
                if (collision.gameObject.GetComponent<Player>().dead)
                    return;
                else
                {
                    collision.gameObject.GetComponent<Player>().LowerHealth(damage);
                    collision.gameObject.GetComponent<Player>().StartDamageCooldown();
                }
            }
        }
    }
}
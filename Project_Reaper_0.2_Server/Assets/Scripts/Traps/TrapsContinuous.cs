using UnityEngine;

public class TrapsContinuous : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private bool velocityBasedTrap;


    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision with trap triggered");
        Player playerScript = collision.gameObject.GetComponent<Player>();
        if (playerScript)
        {
            if (playerScript.isReaper || playerScript.IsInDmgCooldown || playerScript.IsInvincible)
                return;
            else
            {
                if (velocityBasedTrap)
                {
                    Rigidbody rb = gameObject.GetComponent<Rigidbody>();
                    if(rb != null)
                    {
                        // check if velocity would be high enough for damage
                        if(rb.velocity.magnitude > .5f)
                        {
                            collision.gameObject.GetComponent<Player>().LowerHealth(damage);
                            collision.gameObject.GetComponent<Player>().StartDamageCooldown();
                        }
                    }
                }
                else
                {
                    collision.gameObject.GetComponent<Player>().LowerHealth(damage);
                    collision.gameObject.GetComponent<Player>().StartDamageCooldown();
                }
                
            }
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        Debug.Log("Collision with trap triggered");
        Player playerScript = collision.gameObject.GetComponent<Player>();
        if (playerScript)
        {
            if (playerScript.isReaper || playerScript.IsInDmgCooldown || playerScript.IsInvincible)
                return;
            else
            {
                if (velocityBasedTrap)
                {
                    Rigidbody rb = gameObject.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        // check if velocity would be high enough for damage
                        if (rb.velocity.magnitude > .5f)
                        {
                            collision.gameObject.GetComponent<Player>().LowerHealth(damage);
                            collision.gameObject.GetComponent<Player>().StartDamageCooldown();
                        }
                    }
                }
                else
                {
                    collision.gameObject.GetComponent<Player>().LowerHealth(damage);
                    collision.gameObject.GetComponent<Player>().StartDamageCooldown();
                }

            }
        }
    }
}
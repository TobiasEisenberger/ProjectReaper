using UnityEngine;

public class DisableOnCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        
        if(collision.collider.CompareTag("Player") || collision.collider.CompareTag("DisableOnContact"))
        {
            gameObject.SetActive(false);
            SpawnDynamicObjects.DisableObject(gameObject);
        }
        
    }
}

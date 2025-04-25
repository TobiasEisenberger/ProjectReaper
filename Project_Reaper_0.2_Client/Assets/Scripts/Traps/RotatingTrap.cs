using UnityEngine;

public class RotatingTrap : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public float launchForce = 10000f;

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        //rotate trap
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        // Check for collisions with child objects
        CheckChildCollisions();
    }

    private void CheckChildCollisions()
    {
        // Iterate through all child objects
        foreach (Transform child in transform)
        {
            CheckCollision(child.gameObject);
        }
    }

    private void CheckCollision(GameObject childObject)
    {
        // Perform your collision check logic here
        // For example, checking collision with a player
        Collider childCollider = childObject.GetComponent<Collider>();
        if (childCollider != null)
        {
            // Check for collisions with the player or other objects
            Collider[] colliders = Physics.OverlapBox(childCollider.bounds.center, childCollider.bounds.extents, Quaternion.identity);
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Player"))
                {
                    HandleCollision();
                }
            }
        }
    }

    private void HandleCollision()
    {
        Debug.Log("collsion other");
        // Common logic for collision with player
        // Change the direction of the launch vector based on the rotation direction
        Vector3 launchDirection = (rotationSpeed > 0) ? Vector3.forward : Vector3.back;

        // Apply the launch force to the player
        GetComponent<Rigidbody>().AddForce(launchDirection * launchForce, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Prüfe, ob der Spieler kollidiert ist
        if (other.CompareTag("Player"))
        {
            Debug.Log("collsion ontriggerenter");
            // Wechsle die Richtung des Launch-Vektors basierend auf der Drehrichtung
            Vector3 launchDirection = (rotationSpeed > 0) ? Vector3.forward : Vector3.back;

            // Wende die Launch-Kraft auf den Spieler an
            other.GetComponent<Rigidbody>().AddForce(launchDirection * launchForce, ForceMode.Impulse);
        }
    }
}
using UnityEngine;

public class ExplosiveBarrelController : MonoBehaviour
{

    [SerializeField] private ParticleSystem explosionParticles;

    private void OnTriggerEnter(Collider other)
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        explosionParticles.Play();
        Destroy(gameObject, explosionParticles.main.duration);
    }

}

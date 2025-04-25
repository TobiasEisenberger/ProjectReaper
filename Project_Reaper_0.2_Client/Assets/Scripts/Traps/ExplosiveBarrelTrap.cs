using UnityEngine;

public class ExplosiveBarrelTrap : Trap, IDespawnable
{
    [SerializeField]
    private ParticleSystem explosionParticles;
    [SerializeField] AudioSource explosionSound;

    public float OnDespawn()
    {
        activate();
        return explosionParticles.main.duration;
    }

    public override void activate()
    {
        base.activate();
        
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        if (explosionParticles != null) 
        { explosionParticles.Play(); }
        if (explosionSound != null)
        {
            explosionSound.Play();
        }
        
    }
}
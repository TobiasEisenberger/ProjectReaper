using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class PowerUp : MonoBehaviour
{
    [SerializeField] private ParticleSystem pickupEffect;
    [SerializeField] private GameObject parentIcon;
    [SerializeField] private AudioSource pickUpSoundEffect;
    private ParticleSystem instantiatedParticles;

    private void Awake()
    {
        if (pickupEffect != null)
        {
            instantiatedParticles = Instantiate(pickupEffect, transform.position, transform.rotation, transform);
            StopParticles(true);
        }
        else
        {
            Debug.LogWarning("Pickup effect particle system is not assigned.");
        }
    }

    private void StopParticles(bool withChildren)
    {
        
        instantiatedParticles.Stop(withChildren);
      

    }

    private void ActivateParticles(bool withChildren)
    {
        instantiatedParticles.gameObject.SetActive(true);
        instantiatedParticles.Play(withChildren);
        //ameObject light = instantiatedParticles.transform.GetChild(1).gameObject;
       
    }

    private void ActivatePickUpSound()
    {
        pickUpSoundEffect.Play();
    }
    public void PickUp()
    {
        
        GetComponent<MeshRenderer>().enabled = false;
        parentIcon.GetComponent<MeshRenderer>().enabled = false;
        if(pickUpSoundEffect != null)
            ActivatePickUpSound();
        if (instantiatedParticles != null)
        {
         
            ActivateParticles(true);
         
        }
        else
        {
            Debug.LogWarning("Particles are null and can not be playerd.");
        }
    }



    public void OnExpiration()
    {
        if (instantiatedParticles != null)
        {
            StopParticles(true);
        }
        else
        { 
            Debug.LogWarning("cant stop particles as they are null"); 
        }
        GetComponent<MeshRenderer>().enabled = true;
        parentIcon.GetComponent<MeshRenderer>().enabled = true;
    }

}

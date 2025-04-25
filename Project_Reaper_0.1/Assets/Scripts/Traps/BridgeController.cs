using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeController : MonoBehaviour
{

    [SerializeField]
    private ParticleSystem triggerParticle;
    
    [SerializeField]
    private List<GameObject> destructablePaths = new List<GameObject>();

    private bool isBroken = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isBroken || !other.CompareTag("Player"))
            return;

        isBroken = true;
        triggerParticle.Play();
        foreach (GameObject path in destructablePaths)
        {
            path.transform.Rotate(Vector3.forward, 5.0f);
        }
        StartCoroutine(TriggerPhysics());
    }

    private IEnumerator TriggerPhysics()
    {
        yield return new WaitForSeconds(0.2f);
        foreach (GameObject path in destructablePaths)
        {
            Rigidbody rb = path.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.isKinematic = false;
                rb.AddTorque(Vector3.forward, ForceMode.Impulse);
            }
                
            Destroy(path, 3.0f);
        }
    }

}

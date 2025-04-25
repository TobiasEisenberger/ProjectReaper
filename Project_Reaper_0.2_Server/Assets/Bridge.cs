using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge : Trap
{

    [SerializeField]
    private BoxCollider boxCollider;
    private bool isBroken = false;

    private void Start()
    {
        isBroken = false;
        boxCollider.enabled = false;
    }
    public override void activate()
    {
       boxCollider.enabled = true;
       base.activate();
    }

    public override void deactivate()
    {
        base.deactivate();
        boxCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(isActive)
        {
            if (isBroken || !other.CompareTag("Player"))
                return;

            isBroken = true;
            boxCollider.enabled = false;
        }
        
        StartCoroutine(TriggerPhysics());
    }

    private IEnumerator TriggerPhysics()
    {
        yield return new WaitForSeconds(0.2f);
        SpawnDynamicObjects.DisableObject(gameObject);
    }
}

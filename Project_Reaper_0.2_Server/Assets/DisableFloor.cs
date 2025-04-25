using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableFloor : Trap
{
    public override void activate()
    {
        base.activate();
        gameObject.SetActive(false);
        SpawnDynamicObjects.DisableObject(gameObject);
    }

}

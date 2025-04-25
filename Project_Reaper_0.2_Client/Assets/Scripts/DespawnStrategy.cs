using System.Collections;
using UnityEngine;

public class DespawnStrategy : MonoBehaviour
{
    private IDespawnable despawnStrategy;

    private void Start()
    {
        despawnStrategy = gameObject.GetComponent<IDespawnable>();
    }

    public void DisableGameObject()
    {
        float delay = 0.0f;
        if (despawnStrategy != null)
            delay = GetStrategy().OnDespawn();
        StartCoroutine(DisableAfterDelay(delay));
    }

    private IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    private IDespawnable GetStrategy()
        {
            return despawnStrategy;
        }
}
using UnityEngine;

public class SpikeController : MonoBehaviour
{

    [SerializeField]
    private GameObject spikeTrap;
    
    [SerializeField]
    private float speed = 1.0f;

    [SerializeField]
    private Vector3 facingDirection = Vector3.right;

    bool isActive = false;

    void Update()
    {
        if (isActive)
        {
            spikeTrap.transform.localPosition += facingDirection * speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActive)
            return;

        isActive = true;
        gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

}

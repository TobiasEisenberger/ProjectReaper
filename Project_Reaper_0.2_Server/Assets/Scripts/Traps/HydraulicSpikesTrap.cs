using System.Collections;
using UnityEngine;

public class HydraulicSpikesTrap : Trap
{
    [SerializeField] private float targetHeight = 1f;
    [SerializeField] private float meterPerSeconds = 15f;

    private Vector3 pausePoint;
    public bool isOnCooldown;

    private void Start()
    {
        pausePoint = transform.localPosition + Vector3.up * targetHeight;
    }

    private void Update()
    {
        if (isActive)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.localPosition, pausePoint, meterPerSeconds * Time.deltaTime);
            transform.localPosition = newPosition;
            if (newPosition == pausePoint)
            {
                deactivate();
                StartCoroutine(ResetTrap());
            }
        }
    }

    public override void activate()
    {
        if (!isOnCooldown)
            base.activate();
    }

    private IEnumerator ResetTrap()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(2);
        transform.localPosition -= Vector3.up * targetHeight;
        isOnCooldown = false;
    }
}
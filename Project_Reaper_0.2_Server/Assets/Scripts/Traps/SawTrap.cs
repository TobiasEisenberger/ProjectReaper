using UnityEngine;

public class SawTrap : Trap
{
    private Vector3 startPoint;
    private Vector3 turningPoint;

    [SerializeField]
    private Vector3 endPoint;

    [SerializeField]
    private float meterPerSeconds = 5f;

    [SerializeField] private float cooldownDuration = 5f; // Time in seconds before trap can be reactivated
    private bool isOnCooldown = false;

    private void Start()
    {
        startPoint = transform.localPosition;
        turningPoint = endPoint;
    }

    private void FixedUpdate()
    {
        if (isActive)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.localPosition, turningPoint, meterPerSeconds * Time.deltaTime);
            transform.localPosition = newPosition;

            if (isTurningPointReached(newPosition))
            {
                swapTurningPoint();
                deactivate();
            }
        }
    }

    public override void activate()
    {
        Debug.Log($"isOnCooldownActivate: {isOnCooldown}");
        if (!isOnCooldown)
            base.activate();
    }

    public override void deactivate()
    {
        base.deactivate();
        isOnCooldown = true;
        Invoke(nameof(ResetTrap), cooldownDuration);
    }

    private void ResetTrap()
    {
        Debug.Log("CooldownOver");
        isOnCooldown = false;
    }

    private bool isTurningPointReached(Vector3 currentPosition)
    {
        return currentPosition == turningPoint;
    }

    private void swapTurningPoint()
    {
        turningPoint = transform.localPosition == endPoint ? startPoint : endPoint;
    }
}
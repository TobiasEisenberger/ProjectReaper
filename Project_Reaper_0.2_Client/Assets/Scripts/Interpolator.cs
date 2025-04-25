using System.Collections.Generic;
using UnityEngine;

public class Interpolator : MonoBehaviour
{
    [SerializeField] private float timeElapsed = 0f;
    [SerializeField] private float timeToReachTarget = 0.05f;
    [SerializeField] private float movementThreshold = 0.05f;

    private readonly List<TransformUpdate> futureTransformUpdates = new List<TransformUpdate>();
    private float squareMovementThreshold;
    private TransformUpdate to;
    private TransformUpdate from;
    private TransformUpdate previous;

    private void Awake()
    {
        squareMovementThreshold = movementThreshold * movementThreshold;
        ResetTransformUpdates();
    }

    private void FixedUpdate()
    {
        ProcessTransformUpdates();
        timeElapsed += Time.fixedDeltaTime;
        InterpolatePosition(timeElapsed / timeToReachTarget);
    }

    //Function to revieve a directional Vector3 to determinte ongoing player Path
    public Vector3 DetermineDirection()
    {
        Vector3 direction = to.Position - previous.Position;

        return direction;
    }

    private void ResetTransformUpdates()
    {
        to = new TransformUpdate(NetworkManager.Singleton.ServerTick, false, transform.position);
        from = new TransformUpdate(NetworkManager.Singleton.InterpolationTick, false, transform.position);
        previous = new TransformUpdate(NetworkManager.Singleton.InterpolationTick, false, transform.position);
    }

    private void ProcessTransformUpdates()
    {
        for (int i = 0; i < futureTransformUpdates.Count; i++)
        {
            if (NetworkManager.Singleton.ServerTick >= futureTransformUpdates[i].Tick)
            {
                if (futureTransformUpdates[i].IsTeleport)
                {
                    to = futureTransformUpdates[i];
                    from = to;
                    previous = to;
                    transform.position = to.Position;
                }
                else
                {
                    previous = to;
                    to = futureTransformUpdates[i];
                    from = new TransformUpdate(NetworkManager.Singleton.InterpolationTick, false, transform.position);
                    timeElapsed = 0f;
                    timeToReachTarget = (to.Tick - from.Tick) * Time.fixedDeltaTime;
                }
                futureTransformUpdates.RemoveAt(i);
                i--;
            }
        }
    }

    private void InterpolatePosition(float lerpAmount)
    {
        Vector3 transformPositionDebug;
        if (!IsValidPosition(to.Position) || !IsValidPosition(from.Position) || !IsValidPosition(previous.Position))
        {
            Debug.LogWarning("Invalid position detected. Skipping interpolation.");
            return;
        }

        if ((to.Position - previous.Position).sqrMagnitude < squareMovementThreshold)
        {
            if (to.Position != from.Position && IsValidPosition(from.Position) && IsValidPosition(to.Position))
            {
                transformPositionDebug = Vector3.Lerp(from.Position, to.Position, lerpAmount);
                if (IsValidPosition(transformPositionDebug))
                    transform.position = transformPositionDebug;
            }
            return;
        }

        if (IsValidPosition(from.Position) && IsValidPosition(to.Position))
        {
            transformPositionDebug = Vector3.LerpUnclamped(from.Position, to.Position, lerpAmount);
            if (IsValidPosition(transformPositionDebug))
                transform.position = transformPositionDebug;
        }
    }

    private bool IsValidPosition(Vector3 position)
    {
        return !float.IsInfinity(position.x) && !float.IsInfinity(position.y) && !float.IsInfinity(position.z) &&
               !float.IsNaN(position.x) && !float.IsNaN(position.y) && !float.IsNaN(position.z);
    }

    public void ReceiveTransformUpdate(ushort tick, bool isTeleport, Vector3 position)
    {
        if (tick <= NetworkManager.Singleton.InterpolationTick && !isTeleport)
            return;

        for (int i = 0; i < futureTransformUpdates.Count; i++)
        {
            if (tick < futureTransformUpdates[i].Tick)
            {
                futureTransformUpdates.Insert(i, new TransformUpdate(tick, isTeleport, position));
                return;
            }
        }

        futureTransformUpdates.Add(new TransformUpdate(tick, isTeleport, position));
    }
}
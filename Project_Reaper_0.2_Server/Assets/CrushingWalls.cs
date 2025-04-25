using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrushingWalls : Trap
{
    [SerializeField]
    private GameObject exitWall;
    [SerializeField]
    private GameObject entranceWall;
    [SerializeField]
    private List<GameObject> sideWalls;
    [SerializeField]
    private float meterPerSeconds = 5f;
    [SerializeField]
    private float center;
    private List<Vector3> initialSideWallPositions = new();
    private Vector3 initialPositionEntrance;
    private Vector3 initialPositionExit;
    [SerializeField]
    private Vector3 endPositionEntrance, endPositionExit;
    [SerializeField]
    private float cooldownDuration = 20;
    [SerializeField]
    private List<BoxCollider> killColliders;
    private List<Vector3> initialKillColliderPositions = new();
    private bool isOnCooldown = false;


    void Start()
    {
        foreach (var wall in sideWalls)
        {
            initialSideWallPositions.Add(wall.transform.localPosition);
        }
        foreach (var collider in killColliders)
        {
            initialKillColliderPositions.Add(collider.center);
        }

        DeactivateKillHitbox();
        initialPositionEntrance = entranceWall.transform.localPosition;
        initialPositionExit = exitWall.transform.localPosition;
    }

    void FixedUpdate()
    {
        if (isActive)
        {
            // Move exitWall
            if (exitWall.transform.localPosition != endPositionExit)
            {
                Vector3 newPosition = Vector3.MoveTowards(exitWall.transform.localPosition, endPositionExit, meterPerSeconds * Time.deltaTime);
                exitWall.transform.localPosition = newPosition;
            }
            // Move entranceWall
            else if (entranceWall.transform.localPosition != endPositionEntrance)
            {
                Vector3 newPosition = Vector3.MoveTowards(entranceWall.transform.localPosition, endPositionEntrance, meterPerSeconds * Time.deltaTime);
                entranceWall.transform.localPosition = newPosition;
            }
            // Move side walls and parent colliders if both entrance and exit walls are in position
            else if (entranceWall.transform.localPosition == endPositionEntrance && exitWall.transform.localPosition == endPositionExit)
            {
                bool allWallsInPosition = true;
                for (int i = 0; i < sideWalls.Count; i++)
                {
                    Debug.LogWarning($"currentIteration: {i}");
                    var wall = sideWalls[i];
                    var target = new Vector3(wall.transform.localPosition.x, wall.transform.localPosition.y, center);
                    if (wall.transform.localPosition != target)
                    {
                        Vector3 newPosition = Vector3.MoveTowards(wall.transform.localPosition, target, meterPerSeconds * Time.deltaTime);
                        Debug.LogWarning($"Moved wall with index {i} from {wall.transform.localPosition} to {newPosition}");
                        wall.transform.localPosition = newPosition;
            
                        // Move the parent colliders in the same manner
                        if (i < killColliders.Count)
                        {
                            var coll = killColliders[i];
                            Debug.LogWarning($"Moved kill collider with index {i} from {coll.transform.localPosition} to {new Vector3(coll.transform.localPosition.x, coll.transform.localPosition.y, newPosition.z)}");
                            Debug.LogWarning($"Did wall with index {i} from {wall.transform.localPosition} to {newPosition} change?");
                            coll.center = new Vector3(coll.center.x, coll.center.y, newPosition.z);
                            //coll.transform.localPosition = new Vector3(2000f, 2000f, 2000f);
                        }
                        allWallsInPosition = false;
                    }
                }
                if (allWallsInPosition)
                {
                    ResetTrap();
                }
            }
        }
    }

    private void ResetTrap()
    {
        for (int i = 0; i < sideWalls.Count; i++)
        {
            sideWalls[i].transform.localPosition = initialSideWallPositions[i];
        }
        exitWall.transform.localPosition = initialPositionExit;
        entranceWall.transform.localPosition = initialPositionEntrance;
        for (int i = 0; i < killColliders.Count; i++)
        {
            killColliders[i].center = initialKillColliderPositions[i];
        }
        Invoke(nameof(ResetCooldown), cooldownDuration);
        isActive = false;
    }

    private void ResetCooldown()
    {
        isOnCooldown = false;
    }

    private void ActivateKillHitbox()
    {
        foreach (var collider in killColliders)
        {
            collider.enabled = true;
        }
    }

    private void DeactivateKillHitbox()
    {
        foreach (var collider in killColliders)
        {
            collider.enabled = false;
        }
    }

    public override void activate()
    {
        if (!isOnCooldown)
        {
            base.activate();
            isOnCooldown = true;
            isActive = true;
            ActivateKillHitbox();
        }
    }

    public override void deactivate()
    {
        base.deactivate();
        DeactivateKillHitbox();
        isOnCooldown = true;
        isActive = false;
        Invoke(nameof(ResetTrap), cooldownDuration);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player playerScript = other.gameObject.GetComponent<Player>();
            if (playerScript)
            {
                if (playerScript.isReaper || playerScript.IsInDmgCooldown || playerScript.IsInvincible)
                    return;
                else
                {
                    other.gameObject.GetComponent<Player>().LowerHealth(100);
                    other.gameObject.GetComponent<Player>().StartDamageCooldown();
                }
            }
        }
    }
}

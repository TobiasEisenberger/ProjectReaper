using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public SpawnPointReaper[] reaperSpawnPoints;
    public SpawnPointRunner[] runnerSpawnPoints;

    public Vector3 FindSpawnPoint(bool playerIsReaper)
    {
        if (playerIsReaper)
        {
            for (int i = 0; i < reaperSpawnPoints.Length; i++)
            {
                if (reaperSpawnPoints[i].isTaken == false)
                {
                    reaperSpawnPoints[i].isTaken = true;
                    return reaperSpawnPoints[i].transform.position;
                }
            }
            return reaperSpawnPoints[0].transform.position;
        }
        for (int i = 0; i < runnerSpawnPoints.Length; i++)
        {
            if (runnerSpawnPoints[i].isTaken == false)
            {
                runnerSpawnPoints[i].isTaken = true;
                return runnerSpawnPoints[i].transform.position;
            }
        }
        return runnerSpawnPoints[0].transform.position;
    }
}
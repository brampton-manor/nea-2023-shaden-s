using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;
    public MapGenerator mapGenerator;

    private List<Vector3> spawnPoints;
    private bool isSpawnPointsReady = false; // Added flag to track readiness of spawn points

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        StartCoroutine(EnsureMapGeneratedBeforeSpawning());
    }

    IEnumerator EnsureMapGeneratedBeforeSpawning()
    {
        Debug.Log("Waiting for map to be generated...");
        while (!mapGenerator.mapGenerated)
        {
            yield return new WaitForSeconds(1);
        }

        spawnPoints = mapGenerator.GetEmptyCellPositions();
        isSpawnPointsReady = true; // Set the flag to true once spawn points are ready
        Debug.Log("Spawn points are ready.");
    }

    public Vector3 GetSpawnpoint()
    {
        if (!isSpawnPointsReady || spawnPoints.Count == 0)
        {
            Debug.LogError("Spawn points are not ready or not available.");
            // Return a default spawn point or handle this scenario appropriately.
            // For example, you might wait and try again, or handle the lack of spawn points differently.
            return new Vector3(0, 0, 0);
        }

        Debug.Log("Returning a spawn point.");
        return spawnPoints[Random.Range(0, spawnPoints.Count)];
    }

    public bool IsSpawnPointsReady
    {
        get { return isSpawnPointsReady; }
    }
}

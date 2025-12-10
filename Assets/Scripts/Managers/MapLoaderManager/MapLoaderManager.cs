using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapLoaderManager : MonoBehaviour
{
    public static MapLoaderManager Instance { get; private set; }

    [Header("Map Prefabs")]
    public GameObject[] mapPrefabs;

    [Header("Instances")]
    public Transform mapParent;

    private GameObject currentMap;

    [Header("Spawn Points (auto-detected)")]
    public Transform playerSpawnPoint;
    public Transform bossSpawnPoint;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void LoadMap(int index)
    {
        if (index < 0 || index >= mapPrefabs.Length)
        {
            Debug.LogError("Map index out of range!");
            return;
        }

        // Destroy old map
        if (currentMap != null)
            Destroy(currentMap);

        // Load new map prefab
        currentMap = Instantiate(mapPrefabs[index], mapParent);


        EnemySpawnManager.Instance.ResetSpawner();
        // Send spawn points to EnemySpawnManager
        AssignSpawnPointsToEnemyManager();

        SetPlayerPosition();

    }


    private void SetPlayerPosition()
    {
        PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
        if (player != null && playerSpawnPoint != null)
            player.transform.position = playerSpawnPoint.position;
    }

    private void AssignSpawnPointsToEnemyManager()
    {
        Transform spawnRoot = currentMap.transform.Find("EnemySpawnPoints");

        if (spawnRoot == null)
        {
            Debug.LogWarning("No 'SpawnPoints' child found in map prefab!");
            EnemySpawnManager.Instance.spawnPoints = new Transform[0];
            return;
        }

        Transform[] spawnPoints = new Transform[spawnRoot.childCount];
        for (int i = 0; i < spawnRoot.childCount; i++)
        {
            spawnPoints[i] = spawnRoot.GetChild(i);
        }

        EnemySpawnManager.Instance.spawnPoints = spawnPoints;


        //Debug.Log($"Loaded {spawnPoints.Length} enemy spawn points from current map.");

        // Player spawn point
        Transform pSpawn = currentMap.transform.Find("PlayerSpawnPoint");

        if (pSpawn == null)
        {
            Debug.LogError("PlayerSpawnPoint missing in map prefab!");
            playerSpawnPoint = null;
        }
        else
        {
            playerSpawnPoint = pSpawn;
            Debug.Log($"Player spawn point loaded at {playerSpawnPoint.position}");
        }

        // Assign BossSpawnPoint
        bossSpawnPoint = currentMap.transform.Find("BossSpawnPoint");
        if (bossSpawnPoint == null)
            Debug.LogWarning("BossSpawnPoint missing.");
        else
            Debug.Log($"Boss Spawn = {bossSpawnPoint.position}");
    }


}

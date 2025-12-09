using UnityEngine;

public class MapProgressionManager : MonoBehaviour
{
    public static MapProgressionManager Instance { get; private set; }

    [Header("Progression Settings")]
    public int mapsBeforeBoss = 3;

    [Header("Boss Settings")]
    public GameObject bossPrefab;

    [Header("Runtime")]
    public int currentMapIndex = 0;
    private int mapsClearedSinceBoss = 0;
    private bool bossActive = false;

    [Header("Enemy Level Scaling")]
    public int baseEnemyLevel = 1;
    public int enemyLevelIncreasePerMap = 1;

    public int CurrentEnemyLevel { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        CurrentEnemyLevel = baseEnemyLevel;
        LoadNextRoom();
    }

    public void LoadNextRoom()
    {
        mapsClearedSinceBoss++;

        bool shouldSpawnBoss = mapsClearedSinceBoss >= mapsBeforeBoss;

        // Clamp map index for safe access
        int safeMapIndex = Mathf.Min(currentMapIndex, MapLoaderManager.Instance.mapPrefabs.Length - 1);

        if (shouldSpawnBoss)
        {
            SpawnBossRoom(safeMapIndex);
        }
        else
        {
            bossActive = false;

            MapLoaderManager.Instance.LoadMap(safeMapIndex);

            EnemySpawnManager.Instance.OnAllEnemiesDefeated += OnRoomCleared;
        }

        // Increment currentMapIndex only after map is safely loaded
        currentMapIndex++;
    }

    private void OnRoomCleared()
    {
        // Prevent multiple fires
        EnemySpawnManager.Instance.OnAllEnemiesDefeated -= OnRoomCleared;

        Debug.Log("[Progression] Room cleared → loading next room...");
        LoadNextRoom();
    }


    private void SpawnBossRoom(int safeMapIndex)
    {
        Debug.Log("[Progression] BOSS ROOM!");
        bossActive = true;

        // Load map safely
        MapLoaderManager.Instance.LoadMap(safeMapIndex);

        Transform bSpawn = MapLoaderManager.Instance.bossSpawnPoint;
        Transform pSpawn = MapLoaderManager.Instance.playerSpawnPoint;
        Transform[] enemySpawns = EnemySpawnManager.Instance.spawnPoints;

        Vector3 spawnPos;

        if (bSpawn != null)
            spawnPos = bSpawn.position;
        else if (enemySpawns != null && enemySpawns.Length > 0)
            spawnPos = enemySpawns[0].position;
        else if (pSpawn != null)
            spawnPos = pSpawn.position + new Vector3(3f, 0, 0);
        else
            spawnPos = Vector3.zero;

        Instantiate(bossPrefab, spawnPos, Quaternion.identity);

        mapsClearedSinceBoss = 0;
    }
}

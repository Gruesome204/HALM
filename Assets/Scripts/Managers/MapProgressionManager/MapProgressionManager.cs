using UnityEngine;

public class MapProgressionManager : MonoBehaviour
{
    public static MapProgressionManager Instance { get; private set; }

    [Header("Progression Settings")]
    public int mapsBeforeBoss = 3;
    public float enemyHealthMultiplierPerMap = 1.2f;
    public float enemyDamageMultiplierPerMap = 1.15f;

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
        currentMapIndex++;
        mapsClearedSinceBoss++;

        Debug.Log($"[Progression] Loading room {currentMapIndex}...");

        bool shouldSpawnBoss = mapsClearedSinceBoss >= mapsBeforeBoss;

        if (shouldSpawnBoss)
        {
            SpawnBossRoom();
        }
        else
        {
            bossActive = false;

            // Increase enemy level
            CurrentEnemyLevel += enemyLevelIncreasePerMap;
            Debug.Log($"[Progression] Enemy Level increased → {CurrentEnemyLevel}");

            MapLoaderManager.Instance.LoadMap(currentMapIndex);

            // Listen for room completion
            EnemySpawnManager.Instance.OnAllEnemiesDefeated += OnRoomCleared;
        }
    }

    private void OnRoomCleared()
    {
        // Prevent multiple fires
        EnemySpawnManager.Instance.OnAllEnemiesDefeated -= OnRoomCleared;

        Debug.Log("[Progression] Room cleared → loading next room...");
        LoadNextRoom();
    }

    private void SpawnBossRoom()
    {
        Debug.Log("[Progression] BOSS ROOM!");

        bossActive = true;

        // Load map for boss
        MapLoaderManager.Instance.LoadMap(currentMapIndex);

        // Grab all possible spawn references
        Transform bSpawn = MapLoaderManager.Instance.bossSpawnPoint;
        Transform pSpawn = MapLoaderManager.Instance.playerSpawnPoint;
        Transform[] enemySpawns = EnemySpawnManager.Instance.spawnPoints;


        Vector3 spawnPos;

        if (bSpawn != null)
        {
            // 1️⃣ Preferred: boss spawn point
            spawnPos = bSpawn.position;
            Debug.Log("[Progression] Boss spawning at BossSpawnPoint.");
        }
        else if (enemySpawns != null && enemySpawns.Length > 0)
        {
            // 2️⃣ Fallback #1: normal enemy spawn
            spawnPos = enemySpawns[0].position;
            Debug.Log("[Progression] Boss spawn fallback: EnemySpawnPoint[0].");
        }
        else if (pSpawn != null)
        {
            // 3️⃣ Fallback #2: near player
            spawnPos = pSpawn.position + new Vector3(3f, 0, 0);
            Debug.Log("[Progression] Boss spawn fallback: PlayerSpawnPoint.");
        }
        else
        {
            // 4️⃣ Last fallback: origin
            spawnPos = Vector3.zero;
            Debug.LogWarning("[Progression] No spawn points found! Boss at (0,0,0).");
        }

        // Spawn boss
        Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        mapsClearedSinceBoss = 0;
    }
}

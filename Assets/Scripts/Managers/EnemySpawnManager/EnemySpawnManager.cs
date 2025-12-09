using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class EnemySpawnManager : MonoBehaviour, IPausable
{
    public static EnemySpawnManager Instance { get; private set; }

    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public float spawnInterval = 3f;
    public int spawnAmount = 1; // Total number of enemies this spawner will spawn

    [Header("Spawn Randomization")]
    public float spawnRadius = 5f;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;
    public bool useRandomSpawnPoint = true;

    [Header("Boss Settings")]
    public bool isBossRoom = false; // new flag
    public GameObject bossPrefab;


    [Header("Global Enemy Limit")]
    public static int maxEnemies = 20; // Shared across all spawners
    public static List<GameObject> activeEnemies = new List<GameObject>();

    private float spawnTimer = 0f; 
    public int totalSpawned = 0; // How many enemies this spawner has spawned
    private bool allEnemiesSpawned = false; // Tracks if we've spawned all enemies

    public event System.Action OnAllEnemiesDefeated;

    [SerializeField]private bool isPaused;

    private void OnDisable() => GameManager.Instance?.UnregisterPausable(this);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        GameManager.Instance.RegisterPausable(this);
    }
    void Update()
    {
        if (isPaused) return;

        // Don't spawn normal enemies in boss rooms
        if (isBossRoom)
            return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            TrySpawnEnemy();
        }
    }

    private void TrySpawnEnemy()
    {
        // Clean up destroyed enemies from global list
        activeEnemies.RemoveAll(e => e == null);

        // Check if all local enemies spawned
        if (totalSpawned >= spawnAmount)
        {
            allEnemiesSpawned = true;
            CheckIfAllEnemiesDefeated();
            return;
        }

        // Check global enemy limit (subtract this spawner's active enemies)
        int spawnerActiveCount = 0;
        foreach (var e in activeEnemies)
        {
            if (e != null && e.transform.parent == transform)
                spawnerActiveCount++;
        }

        if (activeEnemies.Count >= maxEnemies)
            return;

        // Spawn enemy
        SpawnEnemy();

        // Reset timer only if we actually spawned
        spawnTimer = 0f;
    }

    void SpawnEnemy()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning($"No spawn points assigned on {name}, spawning at spawner position instead.");
            SpawnAtPoint(transform.position);
            return;
        }

        // Choose which spawn point to use
        Transform chosenPoint = useRandomSpawnPoint
            ? spawnPoints[Random.Range(0, spawnPoints.Length)]
            : spawnPoints[0];

        SpawnAtPoint(chosenPoint.position);
    }

    void SpawnAtPoint(Vector3 position)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemyPrefab not assigned!");
            return;
        }

        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = position + new Vector3(offset.x, offset.y, 0f);

        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        if (spawnedEnemy == null)
        {
            Debug.LogError("Failed to instantiate enemyPrefab!");
            return;
        }

        // Apply enemy level scaling
        EnemyStats stats = spawnedEnemy.GetComponent<EnemyStats>();
        if (stats != null)
        {
            if (MapProgressionManager.Instance != null)
                stats.SetLevel(MapProgressionManager.Instance.CurrentEnemyLevel);
            else
                Debug.LogWarning("MapProgressionManager.Instance is null!");
        }
        else
        {
            Debug.LogWarning("EnemyStats component missing on prefab!");
        }

        // Assign target
        EnemyMovement movement = spawnedEnemy.GetComponentInChildren<EnemyMovement>();
        if (movement != null)
        {
            PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
            if (player != null)
                movement.target = player.gameObject;
            else
                Debug.LogWarning("PlayerMovement not found in scene!");
        }

        // Track globally
        activeEnemies.Add(spawnedEnemy);

        // Track per-spawner
        totalSpawned++;
    }

    public void UnregisterEnemy(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
        activeEnemies.RemoveAll(e => e == null);

        CheckIfAllEnemiesDefeated();
    }
    private void CheckIfAllEnemiesDefeated()
    {
        // Only trigger when all have been spawned AND none remain alive
        if (allEnemiesSpawned && activeEnemies.Count == 0)
        {
            OnAllEnemiesDefeated?.Invoke();
        }
    }

    public void SpawnBoss()
    {
        if (MapLoaderManager.Instance == null)
        {
            Debug.LogError("Cannot spawn boss: MapLoaderManager missing!");
            return;
        }

        // Get spawn points
        Transform bSpawn = MapLoaderManager.Instance.bossSpawnPoint;
        Transform pSpawn = MapLoaderManager.Instance.playerSpawnPoint;
        Transform[] enemySpawns = spawnPoints;

        Vector3 spawnPos;

        if (bSpawn != null)
        {
            spawnPos = bSpawn.position;
            Debug.Log("[Spawner] Boss spawning at BossSpawnPoint.");
        }
        else if (enemySpawns != null && enemySpawns.Length > 0)
        {
            spawnPos = enemySpawns[0].position;
            Debug.Log("[Spawner] Boss spawn fallback: EnemySpawnPoint[0].");
        }
        else if (pSpawn != null)
        {
            spawnPos = pSpawn.position + new Vector3(3f, 0, 0);
            Debug.Log("[Spawner] Boss spawn fallback: PlayerSpawnPoint.");
        }
        else
        {
            spawnPos = Vector3.zero;
            Debug.LogWarning("[Spawner] No spawn points found! Boss at (0,0,0).");
        }

        if (bossPrefab == null)
        {
            Debug.LogError("BossPrefab not assigned in EnemySpawnManager!");
            return;
        }

        GameObject boss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);

        // Optionally scale boss stats
        EnemyStats stats = boss.GetComponent<EnemyStats>();
        if (stats != null && MapProgressionManager.Instance != null)
        {
            stats.SetLevel(MapProgressionManager.Instance.CurrentEnemyLevel);
        }

        // Mark as boss room so normal enemies won't spawn
        isBossRoom = true;

        // Reset spawner counters
        ResetSpawner();
        // Track globally
        activeEnemies.Add(boss);
    }

    public void OnPause()
    {
        isPaused = true;
        Debug.Log("Spawner paused");
    }

    public void OnResume()
    {
        isPaused = false;
        Debug.Log("Spawner resumed");
    }

    public void ResetSpawner()
    {
        totalSpawned = 0;
        allEnemiesSpawned = false;
    }
}

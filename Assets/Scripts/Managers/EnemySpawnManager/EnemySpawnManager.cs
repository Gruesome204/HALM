using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour, IPausable
{
    public static EnemySpawnManager Instance { get; private set; }

    [Header("Spawn Settings")]
    [HideInInspector]
    public List<GameObject> enemyPrefabs = new List<GameObject>(); // Multiple enemy types

    [Header("Spawn Randomization")]
    public float spawnRadius = 5f;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;
    public bool useRandomSpawnPoint = true;

    [Header("Boss Settings")]
    public bool isBossRoom = false; // new flag

    [Header("Global Enemy Limit")]
    public static int maxEnemies = 20; // Shared across all spawners
    public static List<GameObject> activeEnemies = new List<GameObject>();
    public bool AreEnemiesAlive => activeEnemies.Count > 0;

    private float spawnTimer = 0f; 
    public int totalSpawned = 0; // How many enemies this spawner has spawned
    private bool allEnemiesSpawned = false; // Tracks if we've spawned all enemies

    public event System.Action OnAllEnemiesDefeated;

    public event System.Action OnBossDefeated;

    [SerializeField]private bool isPaused;

    private MapEnemySetup CurrentMapSetup => MapLoaderManager.Instance?.CurrentMap?.GetComponent<MapEnemySetup>();

    public int CurrentSpawnAmount => (CurrentMapSetup != null && CurrentMapSetup.spawnAmount > -1)
        ? CurrentMapSetup.spawnAmount
        : 1; // fallback

    public float CurrentSpawnInterval => (CurrentMapSetup != null && CurrentMapSetup.spawnInterval > 0f)
        ? CurrentMapSetup.spawnInterval : 3f;

    [Header("Debug / Inspector")]
    [SerializeField, ReadOnly] private int enemiesRemainingInspector;

    [SerializeField] public int EnemiesRemaining
    {
        get
        {
            // Clean null references first
            activeEnemies.RemoveAll(e => e == null);

            // Only count remaining normal enemies (exclude bosses if you want separate handling)
            return activeEnemies.Count;
        }
    }

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

        enemiesRemainingInspector = EnemiesRemaining;

        // Don't spawn normal enemies in boss rooms
        if (isBossRoom)
            return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= CurrentSpawnInterval)
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
        if (totalSpawned >= CurrentSpawnAmount)
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
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogError("No enemy prefabs assigned!");
            return;
        }

        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = position + new Vector3(offset.x, offset.y, 0f);

        // Pick random prefab
        GameObject chosenPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];

        GameObject spawnedEnemy = Instantiate(chosenPrefab, spawnPos, Quaternion.identity);

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
        // Track per-spawner
        totalSpawned++;
    }

    public void RegisterEnemy(GameObject enemy, Transform owner = null)
    {
        if (enemy == null)
            return;

        // Prevent duplicates
        if (!activeEnemies.Contains(enemy))
            activeEnemies.Add(enemy);

        // Optional: parent to spawner for per-spawner tracking
        if (owner != null)
            enemy.transform.SetParent(owner);

        // Clean up null references
        activeEnemies.RemoveAll(e => e == null);
    }

    public void UnregisterEnemy(GameObject enemy)
    {
        bool wasBoss = enemy.CompareTag("Boss"); // Make sure your boss prefab has the "Boss" tag!

        activeEnemies.Remove(enemy);
        activeEnemies.RemoveAll(e => e == null);

        // If it was a boss, trigger boss defeated event
        if (wasBoss)
        {
            Debug.Log("[Spawner] Boss defeated!");
            OnBossDefeated?.Invoke();

            // Reset boss room flag
            isBossRoom = false;
        }

        // Check normal enemies
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

    public void SpawnBoss(GameObject bossPrefab)
    {
        if (MapLoaderManager.Instance == null)
        {
            Debug.LogError("Cannot spawn boss: MapLoaderManager missing!");
            return;
        }

        // Get spawn points
        Transform bSpawn = MapLoaderManager.Instance.bossSpawnPoint;

        GameObject boss = Instantiate(bossPrefab, bSpawn.position, Quaternion.identity);

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
        RegisterEnemy(boss);

        totalSpawned = 1;
        allEnemiesSpawned = true;
    }

    public void PrepareForNewRoom()
    {
        ResetSpawner();
        isBossRoom = false;
        activeEnemies.Clear();
    }

    public void OnPause()
    {
        isPaused = true;
    }

    public void OnResume()
    {
        isPaused = false;
    }

    public void ResetSpawner()
    {
        totalSpawned = 0;
        allEnemiesSpawned = false;
    }

}

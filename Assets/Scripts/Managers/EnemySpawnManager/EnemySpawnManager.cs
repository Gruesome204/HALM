using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour, IPausable
{
    public static EnemySpawnManager Instance { get; private set; }

    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public float spawnInterval = 3f;
    public int spawnAmount = 1; // Total number of enemies this spawner will spawn

    [Header("Spawn Points")]
    public Transform[] spawnPoints;
    public bool useRandomSpawnPoint = true;

    [Header("Global Enemy Limit")]
    public static int maxEnemies = 20; // Shared across all spawners
    public static List<GameObject> activeEnemies = new List<GameObject>();

    private float spawnTimer = 0f; 
    private int totalSpawned = 0; // How many enemies this spawner has spawned
    private bool allEnemiesSpawned = false; // Tracks if we've spawned all enemies

    public event System.Action OnAllEnemiesDefeated;

    private bool isPaused;

    private void OnEnable() => GameManager.Instance?.RegisterPausable(this);
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
    void Update()
    {
        if (isPaused) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            TrySpawnEnemy();
        }
    }

    void TrySpawnEnemy()
    {
        if (isPaused) return;
        // Clean up destroyed enemies
        activeEnemies.RemoveAll(e => e == null);

        // Respect global and local limits
        if (activeEnemies.Count >= maxEnemies) return;
        if (totalSpawned >= spawnAmount)
        {
            allEnemiesSpawned = true;
            CheckIfAllEnemiesDefeated();
            return;
        }

        SpawnEnemy();
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
        GameObject spawnedEnemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        spawnedEnemy.GetComponent<EnemyMovement>().target = FindAnyObjectByType<PlayerMovement>().gameObject;
        activeEnemies.Add(spawnedEnemy);
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

    public void OnPause()
    {
        isPaused = true;
        // Stop moving, stop attacking, etc.
    }

    public void OnResume()
    {
        isPaused = false;
        // Resume AI
    }
}
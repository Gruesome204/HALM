using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour, IPausable, IGameSystem
{
    public static EnemySpawnManager Instance { get; private set; }

    [Header("Spawn Settings")]
    [HideInInspector]
    public List<GameObject> enemyPrefabs = new List<GameObject>(); // Multiple enemy types

    [Header("Spawn Randomization")]
    public float spawnRadius = 2f;
    public bool randomizeSpawnPosition = true; 

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

    [SerializeField] private bool isPaused;
    private bool isInitialized = false;
    private bool isSubscribedToEvents = false;

    // ========================
    // PROPERTIES
    // ========================
    private MapEnemySetup CurrentMapSetup => MapLoaderManager.Instance?.CurrentMap?.GetComponent<MapEnemySetup>();

    public int CurrentSpawnAmount => (CurrentMapSetup != null && CurrentMapSetup.spawnAmount > -1)
        ? CurrentMapSetup.spawnAmount
        : 1; // fallback

    public float CurrentSpawnInterval => (CurrentMapSetup != null && CurrentMapSetup.spawnInterval > 0f)
        ? CurrentMapSetup.spawnInterval : 3f;

    [Header("Debug / Inspector")]
    [SerializeField, ReadOnly] private int enemiesRemainingInspector;

    [SerializeField]
    public int EnemiesRemaining
    {
        get
        {
            // Clean null references first
            activeEnemies.RemoveAll(e => e == null);

            // Only count remaining normal enemies (exclude bosses if you want separate handling)
            return activeEnemies.Count;
        }
    }

    public bool IsInitialized => isInitialized;

    // ========================
    // IGameSystem Implementation
    // ========================
    public int InitializePriority => 3;

    public void Initialize()
    {
        if (isInitialized)
        {
            Debug.Log("[EnemySpawnManager] Already initialized.");
            return;
        }

        Debug.Log("[EnemySpawnManager] Initializing...");

        // Validate dependencies
        ValidateDependencies();

        // Reset state
        ResetState();

        // Clean up any stale enemies
        CleanupStaleEnemies();

        isInitialized = true;
        Debug.Log("[EnemySpawnManager] Initialized successfully.");
    }

    public void PostInitialize()
    {
        Debug.Log("[EnemySpawnManager] Post-Initializing...");

        // Subscribe to events
        SubscribeToEvents();

        // Register with GameManager for pause functionality
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterPausable(this);
            Debug.Log("[EnemySpawnManager] Registered with GameManager.");
        }

        // Prepare for first room
        PrepareForNewRoom();

        Debug.Log("[EnemySpawnManager] Post-Initialization complete.");
    }

    // ========================
    // UNITY LIFECYCLE
    // ========================
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Auto-initialize if not already done
        if (!isInitialized)
        {
            Debug.LogWarning("[EnemySpawnManager] Auto-initializing in Start()");
            Initialize();
            PostInitialize();
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterPausable(this);
        }

        // Clean up active enemies
        CleanupAllEnemies();
        isInitialized = false;
    }

    private void Update()
    {
        if (!isInitialized || isPaused) return;

        // Update inspector display
        enemiesRemainingInspector = EnemiesRemaining;

        // Don't spawn normal enemies in boss rooms
        if (isBossRoom)
            return;

        // Don't spawn if we've spawned all enemies
        if (allEnemiesSpawned)
        {
            CheckIfAllEnemiesDefeated();
            return;
        }

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= CurrentSpawnInterval)
        {
            spawnTimer = 0f;
            TrySpawnEnemy();
        }
    }

    // ========================
    // VALIDATION
    // ========================
    private void ValidateDependencies()
    {
        if (MapLoaderManager.Instance == null)
        {
            Debug.LogWarning("[EnemySpawnManager] MapLoaderManager not found!");
        }

        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogWarning("[EnemySpawnManager] No enemy prefabs assigned!");
        }
    }

    // ========================
    // EVENT SUBSCRIPTION
    // ========================
    private void SubscribeToEvents()
    {
        if (isSubscribedToEvents) return;

        // Subscribe to map loaded events
        if (MapLoaderManager.Instance != null)
        {
            MapLoaderManager.Instance.OnMapLoaded += OnMapLoadedHandler;
            isSubscribedToEvents = true;
            Debug.Log("[EnemySpawnManager] Subscribed to MapLoader events.");
        }
        else
        {
            Debug.LogWarning("[EnemySpawnManager] Cannot subscribe to MapLoader events - MapLoaderManager missing.");
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (isSubscribedToEvents && MapLoaderManager.Instance != null)
        {
            MapLoaderManager.Instance.OnMapLoaded -= OnMapLoadedHandler;
            isSubscribedToEvents = false;
            Debug.Log("[EnemySpawnManager] Unsubscribed from MapLoader events.");
        }
    }

    private void OnMapLoadedHandler(GameObject map)
    {
        // Prepare for new room when map loads
        PrepareForNewRoom();
        Debug.Log($"[EnemySpawnManager] New map loaded: {map.name}");
    }

    // ========================
    // STATE MANAGEMENT
    // ========================
    private void ResetState()
    {
        spawnTimer = 0f;
        totalSpawned = 0;
        allEnemiesSpawned = false;
        isBossRoom = false;
        isPaused = false;
    }

    private void CleanupStaleEnemies()
    {
        activeEnemies.RemoveAll(e => e == null);
    }

    private void CleanupAllEnemies()
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
    }

    // ========================
    // SPAWNING LOGIC
    // ========================
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

        // Check global enemy limit
        if (activeEnemies.Count >= maxEnemies)
            return;

        // Spawn enemy
        SpawnEnemy();

        // Reset timer only if we actually spawned
        spawnTimer = 0f;
    }

    private void SpawnEnemy()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning($"[EnemySpawnManager] No spawn points assigned on {name}, spawning at spawner position instead.");
            SpawnAtPoint(transform.position);
            return;
        }

        // Choose which spawn point to use
        Transform chosenPoint = useRandomSpawnPoint
            ? spawnPoints[Random.Range(0, spawnPoints.Length)]
            : spawnPoints[0];

        SpawnAtPoint(chosenPoint.position);
    }

    private void SpawnAtPoint(Vector3 position)
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogError("[EnemySpawnManager] No enemy prefabs assigned!");
            return;
        }

        // Pick random prefab FIRST
        GameObject chosenPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];

        // If randomizeSpawnPosition is false, use the exact spawn point
        Vector3 spawnPos = randomizeSpawnPosition
            ? GetValidSpawnPosition(position, chosenPrefab)
            : position; // Use exact position

        GameObject spawnedEnemy = Instantiate(chosenPrefab, spawnPos, Quaternion.identity);

        if (spawnedEnemy == null)
        {
            Debug.LogError("[EnemySpawnManager] Failed to instantiate enemyPrefab!");
            return;
        }

        // Apply enemy level scaling
        EnemyStats stats = spawnedEnemy.GetComponent<EnemyStats>();
        if (stats != null)
        {
            if (MapProgressionManager.Instance != null)
                stats.SetLevel(MapProgressionManager.Instance.CurrentEnemyLevel);
            else
                Debug.LogWarning("[EnemySpawnManager] MapProgressionManager.Instance is null!");
        }
        else
        {
            Debug.LogWarning("[EnemySpawnManager] EnemyStats component missing on prefab!");
        }

        // Assign target
        EnemyMovement movement = spawnedEnemy.GetComponentInChildren<EnemyMovement>();
        if (movement != null)
        {
            PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
            if (player != null)
                movement.target = player.gameObject;
            else
                Debug.LogWarning("[EnemySpawnManager] PlayerMovement not found in scene!");
        }

        // Track per-spawner
        totalSpawned++;
        RegisterEnemy(spawnedEnemy, transform);
    }
    private Vector3 GetValidSpawnPosition(Vector3 basePosition, GameObject enemyPrefab)
    {
        // Maximum attempts to find a valid position
        const int maxAttempts = 30;
        const float searchRadius = 4f; // How far to search from the base position

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Generate random offset within the search radius
            Vector2 randomOffset = Random.insideUnitCircle * searchRadius;
            Vector3 testPosition = basePosition + new Vector3(randomOffset.x, randomOffset.y, 0f);

            // Check if this position is valid (walkable AND clear of walls)
            if (IsValidSpawnPosition(testPosition, enemyPrefab))
            {
                return testPosition;
            }
        }

        // If no valid position found, try with a smaller radius
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * (searchRadius / 2f);
            Vector3 testPosition = basePosition + new Vector3(randomOffset.x, randomOffset.y, 0f);

            if (IsValidSpawnPosition(testPosition, enemyPrefab))
            {
                return testPosition;
            }
        }

        // Last resort: try the base position with a slightly larger margin
        if (IsValidSpawnPosition(basePosition, enemyPrefab))
        {
            return basePosition;
        }

        // If still no valid position, return the original position (with warning)
        Debug.LogWarning($"[EnemySpawnManager] Could not find valid spawn position for {enemyPrefab.name} near {basePosition}");
        return basePosition;
    }

    private bool IsValidSpawnPosition(Vector3 worldPosition, GameObject enemyPrefab)
    {
        // Check if GridManager exists
        if (GridManager.Instance == null)
        {
            Debug.LogWarning("[EnemySpawnManager] GridManager.Instance is null, skipping grid validation.");
            return true; // Allow spawn if no grid manager
        }

        // Convert world position to grid coordinates
        Vector2Int gridCoords = GridManager.Instance.GetGridCoordinates(worldPosition);

        // Check if coordinates are within grid bounds
        if (gridCoords.x < 0 || gridCoords.x >= GridManager.Instance.gridWidth ||
            gridCoords.y < 0 || gridCoords.y >= GridManager.Instance.gridHeight)
        {
            return false; // Outside grid bounds
        }

        // Check if the cell is walkable
        bool isWalkable = GridManager.Instance.IsWalkable(gridCoords);
        if (!isWalkable)
            return false;

        // CRITICAL: Check for actual physics collisions with walls
        if (!IsPositionClearOfWalls(worldPosition, enemyPrefab))
            return false;

        return true;
    }

    private bool IsPositionClearOfWalls(Vector3 position, GameObject enemyPrefab)
    {
        // Get the enemy's collider to check its size
        Collider2D enemyCollider = enemyPrefab.GetComponent<Collider2D>();
        if (enemyCollider == null)
        {
            // If no collider, just check a small area
            return CheckCircleClear(position, 0.5f);
        }

        // Get the collider's size and check appropriate area
        float checkRadius;
        if (enemyCollider is CircleCollider2D circleCollider)
        {
            checkRadius = circleCollider.radius * 1.2f; // Add 20% margin
        }
        else if (enemyCollider is BoxCollider2D boxCollider)
        {
            // Use the larger dimension plus margin
            checkRadius = Mathf.Max(boxCollider.size.x, boxCollider.size.y) / 2f * 1.2f;
        }
        else if (enemyCollider is CapsuleCollider2D capsuleCollider)
        {
            checkRadius = capsuleCollider.size.x / 2f * 1.2f;
        }
        else
        {
            // Default fallback
            checkRadius = 0.75f;
        }

        return CheckCircleClear(position, checkRadius);
    }

    private bool CheckCircleClear(Vector3 center, float radius)
    {
        // Check for any colliders in the area (walls, obstacles, etc.)
        // You might want to use a specific layer mask for walls
        LayerMask wallMask = LayerMask.GetMask("Walls"); // Adjust to your wall layer
                                                         // Or use the ground layer from GridManager
        if (GridManager.Instance != null && GridManager.Instance.groundLayer != 0)
        {
            wallMask = GridManager.Instance.groundLayer;
        }

        // Check if there are any colliders in the area
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, radius, wallMask);

        // Also check if any existing enemies are overlapping (optional)
        // This prevents enemies from spawning on top of each other
        Collider2D[] enemyColliders = Physics2D.OverlapCircleAll(center, radius, LayerMask.GetMask("Enemy"));
        if (enemyColliders.Length > 0)
        {
            // Don't count the enemy we're about to spawn if it's already in the scene
            // This is a safety check
            return false;
        }

        // If there are any walls in the area, position is invalid
        if (hitColliders.Length > 0)
        {
            // Optional debug - uncomment if needed
            // Debug.Log($"Position {center} is blocked by {hitColliders[0].gameObject.name}");
            return false;
        }

        return true;
    }
    // ========================
    // ENEMY REGISTRATION
    // ========================
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
        if (enemy == null) return;

        bool wasBoss = enemy.CompareTag("Boss"); // Make sure your boss prefab has the "Boss" tag!

        activeEnemies.Remove(enemy);
        activeEnemies.RemoveAll(e => e == null);

        // If it was a boss, trigger boss defeated event
        if (wasBoss)
        {
            Debug.Log("[EnemySpawnManager] Boss defeated!");
            OnBossDefeated?.Invoke();

            // Reset boss room flag
            isBossRoom = false;
        }

        // Check normal enemies
        CheckIfAllEnemiesDefeated();
    }

    // ========================
    // BOSS SPAWNING
    // ========================
    public void SpawnBoss(GameObject bossPrefab)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[EnemySpawnManager] Cannot spawn boss - not initialized!");
            return;
        }

        if (MapLoaderManager.Instance == null)
        {
            Debug.LogError("[EnemySpawnManager] Cannot spawn boss: MapLoaderManager missing!");
            return;
        }

        if (bossPrefab == null)
        {
            Debug.LogError("[EnemySpawnManager] Boss prefab is null!");
            return;
        }

        // Get spawn point
        Transform bSpawn = MapLoaderManager.Instance.BossSpawnPoint;
        if (bSpawn == null)
        {
            Debug.LogError("[EnemySpawnManager] Boss spawn point not found!");
            return;
        }

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

        Debug.Log("[EnemySpawnManager] Boss spawned successfully!");
    }

    // ========================
    // ROOM MANAGEMENT
    // ========================
    public void PrepareForNewRoom()
    {
        if (!isInitialized) return;

        ResetSpawner();
        isBossRoom = false;
        CleanupStaleEnemies();

        // If CurrentSpawnAmount is 0, mark as all spawned so room clears
        if (CurrentSpawnAmount <= 0)
        {
            allEnemiesSpawned = true;
            CheckIfAllEnemiesDefeated();
        }

        Debug.Log("[EnemySpawnManager] Prepared for new room.");
    }

    private void CheckIfAllEnemiesDefeated()
    {
        // Clean up null references first
        activeEnemies.RemoveAll(e => e == null);

        // Only trigger when all have been spawned AND none remain alive
        if (allEnemiesSpawned && activeEnemies.Count == 0)
        {
            Debug.Log("[EnemySpawnManager] All enemies defeated!");
            OnAllEnemiesDefeated?.Invoke();
        }
    }

    // ========================
    // PUBLIC METHODS
    // ========================
    public void ResetSpawner()
    {
        totalSpawned = 0;
        allEnemiesSpawned = false;
        spawnTimer = 0f;
    }

    public void ClearAllEnemies()
    {
        CleanupAllEnemies();
        ResetSpawner();
        Debug.Log("[EnemySpawnManager] All enemies cleared.");
    }

    public int GetActiveEnemyCount()
    {
        CleanupStaleEnemies();
        return activeEnemies.Count;
    }

    public List<GameObject> GetActiveEnemies()
    {
        CleanupStaleEnemies();
        return new List<GameObject>(activeEnemies);
    }

    public bool HasEnemiesRemaining()
    {
        CleanupStaleEnemies();
        return activeEnemies.Count > 0 || !allEnemiesSpawned;
    }

    // ========================
    // IPausable Implementation
    // ========================
    public void OnPause()
    {
        isPaused = true;
        Debug.Log("[EnemySpawnManager] Paused.");
    }

    public void OnResume()
    {
        isPaused = false;
        Debug.Log("[EnemySpawnManager] Resumed.");
    }

    // ========================
    // DEBUG HELPERS
    // ========================
    public void DebugSpawnerState()
    {
        if (!isInitialized)
        {
            Debug.Log("[EnemySpawnManager] Not initialized!");
            return;
        }

        Debug.Log("[EnemySpawnManager] Spawner State:");
        Debug.Log($"  - Initialized: {isInitialized}");
        Debug.Log($"  - Paused: {isPaused}");
        Debug.Log($"  - Is Boss Room: {isBossRoom}");
        Debug.Log($"  - Total Spawned: {totalSpawned}");
        Debug.Log($"  - All Spawned: {allEnemiesSpawned}");
        Debug.Log($"  - Spawn Amount: {CurrentSpawnAmount}");
        Debug.Log($"  - Spawn Interval: {CurrentSpawnInterval}");
        Debug.Log($"  - Active Enemies: {activeEnemies.Count}");
        Debug.Log($"  - Spawn Points: {(spawnPoints != null ? spawnPoints.Length : 0)}");
        Debug.Log($"  - Enemy Prefabs: {(enemyPrefabs != null ? enemyPrefabs.Count : 0)}");
    }

    // ========================
    // GAME STATE HANDLING
    // ========================
    public void OnGameReset()
    {
        if (!isInitialized) return;

        ClearAllEnemies();
        ResetState();
        PrepareForNewRoom();
        Debug.Log("[EnemySpawnManager] Game reset complete.");
    }

    // ========================
    // UNITY EDITOR HELPERS
    // ========================
#if UNITY_EDITOR
    private void OnValidate()
    {
        // Validate spawn settings in editor
        if (spawnRadius < 0) spawnRadius = 0;
        if (maxEnemies < 1) maxEnemies = 1;
    }
#endif
}
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps; // Add this for Tilemap support

public class EnemySpawnManager : MonoBehaviour, IPausable, IGameSystem
{
    public static EnemySpawnManager Instance { get; private set; }

    [Header("Spawn Settings")]
    [HideInInspector]
    public List<GameObject> enemyPrefabs = new List<GameObject>(); // Multiple enemy types

    [Header("Spawn Points")]
    public Transform[] spawnPoints;
    public int spawnPointIndex = 0; // Which spawn point to use

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
        spawnPointIndex = 0;
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
            Vector3 spawnPos = SnapToGround(transform.position);
            if (IsPositionValidForSpawning(spawnPos))
            {
                SpawnAtPoint(spawnPos);
            }
            return;
        }

        // Try multiple spawn points if the first one is blocked
        int attempts = 0;
        int maxAttempts = spawnPoints.Length * 2;
        bool spawned = false;

        while (attempts < maxAttempts && !spawned)
        {
            // Ensure spawn point index is valid
            if (spawnPointIndex >= spawnPoints.Length)
            {
                spawnPointIndex = 0;
            }

            Transform chosenPoint = spawnPoints[spawnPointIndex];

            // Snap position to ground first
            Vector3 snappedPosition = SnapToGround(chosenPoint.position);

            // Check if position is valid (on ground and not blocked by walls)
            if (IsPositionValidForSpawning(snappedPosition))
            {
                SpawnAtPoint(snappedPosition);
                spawned = true;
            }
            else
            {
                // Try next spawn point
                spawnPointIndex = (spawnPointIndex + 1) % spawnPoints.Length;
                attempts++;
            }
        }

        // If no valid spawn point found after all attempts
        if (!spawned)
        {
            Debug.LogWarning("[EnemySpawnManager] No valid spawn points found! Skipping spawn.");
        }
        else
        {
            // Increment for next spawn (round-robin)
            spawnPointIndex = (spawnPointIndex + 1) % spawnPoints.Length;
        }
    }

    private void SpawnAtPoint(Vector3 position)
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogError("[EnemySpawnManager] No enemy prefabs assigned!");
            return;
        }

        // Double-check position is valid before spawning
        if (!IsPositionValidForSpawning(position))
        {
            Debug.LogWarning($"[EnemySpawnManager] Position {position} is not valid for spawning!");
            return;
        }

        // Pick random prefab
        GameObject chosenPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];

        GameObject spawnedEnemy = Instantiate(chosenPrefab, position, Quaternion.identity);
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

    // ========================
    // SPAWN POSITION VALIDATION - FIXED FOR TILEMAPS
    // ========================

    /// <summary>
    /// Checks if a position is valid for enemy spawning
    /// - Must be on GroundLayer
    /// - Must not be blocked by wall layer objects
    /// </summary>
    private bool IsPositionValidForSpawning(Vector3 position)
    {
        // Check if position is on ground layer
        if (!IsOnGroundLayer(position))
        {
            Debug.Log($"[EnemySpawnManager] Position {position} is not on GroundLayer");
            return false;
        }

        // Check if position is blocked by walls
        if (IsPositionBlockedByWalls(position))
        {
            Debug.Log($"[EnemySpawnManager] Position {position} is blocked by walls");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if the position is on the GroundLayer - SPECIALLY FOR TILEMAPS
    /// </summary>
    private bool IsOnGroundLayer(Vector3 position)
    {
        // Get the ground layer mask
        LayerMask groundMask = GetGroundLayerMask();

        // If no ground layer is configured, try to find it automatically
        if (groundMask == 0)
        {
            Debug.LogWarning("[EnemySpawnManager] Ground layer not configured, attempting to find Tilemap ground.");
            groundMask = FindTilemapGroundLayer();

            if (groundMask == 0)
            {
                Debug.LogWarning("[EnemySpawnManager] Still no ground layer found, using fallback detection.");
                return IsPositionAboveGround(position);
            }
        }

        // METHOD 1: Check 2D first (for Tilemaps)
        if (Is2DGame() || HasTilemapCollider())
        {
            // Check if position overlaps with ground collider (2D)
            Collider2D[] groundColliders = Physics2D.OverlapCircleAll(position, 0.3f, groundMask);
            if (groundColliders.Length > 0)
            {
                // Check if any of these are TilemapCollider2D
                foreach (var collider in groundColliders)
                {
                    if (collider is TilemapCollider2D || collider is CompositeCollider2D)
                    {
                        Debug.Log($"[EnemySpawnManager] Position {position} is on Tilemap ground (2D)");
                        return true;
                    }
                }
                Debug.Log($"[EnemySpawnManager] Position {position} is on ground (2D)");
                return true;
            }

            // Raycast downward in 2D
            Vector2 origin2D = new Vector2(position.x, position.y + 1f);
            RaycastHit2D hit2D = Physics2D.Raycast(origin2D, Vector2.down, 3f, groundMask);
            if (hit2D.collider != null)
            {
                float distanceToGround = Mathf.Abs(position.y - hit2D.point.y);
                if (distanceToGround < 0.5f)
                {
                    Debug.Log($"[EnemySpawnManager] Position {position} is above Tilemap ground (2D raycast)");
                    return true;
                }
            }
        }

        // METHOD 2: Check 3D
        // Check if the position itself is on ground
        if (Physics.CheckSphere(position, 0.3f, groundMask))
        {
            return true;
        }

        // Raycast downward from slightly above
        float checkDistance = 3f;
        Vector3 origin = position + Vector3.up * 1f;
        RaycastHit hit;

        if (Physics.Raycast(origin, Vector3.down, out hit, checkDistance, groundMask))
        {
            float distanceToGround = Mathf.Abs(position.y - hit.point.y);
            if (distanceToGround < 0.5f)
            {
                return true;
            }
            Debug.Log($"[EnemySpawnManager] Ground found at {hit.point.y}, but position is at {position.y} (distance: {distanceToGround})");
        }

        // METHOD 3: Check if there's any collider below (fallback)
        if (IsPositionAboveGround(position))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if there's a TilemapCollider2D in the scene
    /// </summary>
    private bool HasTilemapCollider()
    {
        TilemapCollider2D[] tilemapColliders = GameObject.FindObjectsOfType<TilemapCollider2D>();
        return tilemapColliders.Length > 0;
    }

    /// <summary>
    /// Tries to find the ground layer from Tilemap colliders
    /// </summary>
    private LayerMask FindTilemapGroundLayer()
    {
        // Look for TilemapCollider2D components
        TilemapCollider2D[] tilemapColliders = GameObject.FindObjectsOfType<TilemapCollider2D>();
        if (tilemapColliders.Length > 0)
        {
            // Get the layer from the first Tilemap
            int layer = tilemapColliders[0].gameObject.layer;
            Debug.Log($"[EnemySpawnManager] Found Tilemap on layer {layer} ({LayerMask.LayerToName(layer)})");
            return 1 << layer;
        }

        // Look for CompositeCollider2D (often used with Tilemaps)
        CompositeCollider2D[] compositeColliders = GameObject.FindObjectsOfType<CompositeCollider2D>();
        if (compositeColliders.Length > 0)
        {
            int layer = compositeColliders[0].gameObject.layer;
            Debug.Log($"[EnemySpawnManager] Found CompositeCollider2D on layer {layer} ({LayerMask.LayerToName(layer)})");
            return 1 << layer;
        }

        // Look for any ground-named objects
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains("ground") || obj.name.ToLower().Contains("floor"))
            {
                if (obj.GetComponent<Collider2D>() != null)
                {
                    int layer = obj.layer;
                    Debug.Log($"[EnemySpawnManager] Found ground object '{obj.name}' on layer {layer}");
                    return 1 << layer;
                }
            }
        }

        return 0;
    }

    /// <summary>
    /// Checks if a position is blocked by wall layer objects
    /// </summary>
    private bool IsPositionBlockedByWalls(Vector3 position)
    {
        float checkRadius = 0.4f;

        // Get wall layer mask
        LayerMask wallMask = GetWallLayerMask();

        if (wallMask == 0)
        {
            // If no wall layer is configured, return false (not blocked)
            return false;
        }

        // Check 2D first (for Tilemaps)
        if (Is2DGame() || HasTilemapCollider())
        {
            Collider2D[] colliders2D = Physics2D.OverlapCircleAll(position, checkRadius, wallMask);
            if (colliders2D.Length > 0)
            {
                foreach (var collider in colliders2D)
                {
                    if (collider is TilemapCollider2D || collider is CompositeCollider2D)
                    {
                        Debug.Log($"[EnemySpawnManager] Position {position} blocked by Tilemap wall: {collider.gameObject.name}");
                        return true;
                    }
                }
                Debug.Log($"[EnemySpawnManager] Position {position} blocked by wall (2D)");
                return true;
            }
        }

        // Check 3D
        Collider[] colliders = Physics.OverlapSphere(position, checkRadius, wallMask);
        bool isBlocked = colliders.Length > 0;

        if (isBlocked)
        {
            foreach (var collider in colliders)
            {
                Debug.Log($"[EnemySpawnManager] Position {position} blocked by: {collider.gameObject.name} (Layer: {collider.gameObject.layer})");
            }
        }

        return isBlocked;
    }

    /// <summary>
    /// Fallback method to check if a position is above any collider (not just ground)
    /// </summary>
    private bool IsPositionAboveGround(Vector3 position)
    {
        // Check 2D first
        if (Is2DGame() || HasTilemapCollider())
        {
            Vector2 origin2D = new Vector2(position.x, position.y + 0.5f);
            RaycastHit2D hit2D = Physics2D.Raycast(origin2D, Vector2.down, 5f);
            if (hit2D.collider != null)
            {
                float distanceToGround = Mathf.Abs(position.y - hit2D.point.y);
                if (distanceToGround < 0.5f)
                {
                    Debug.Log($"[EnemySpawnManager] Fallback 2D: Found ground below at {hit2D.point.y} (distance: {distanceToGround})");
                    return true;
                }
            }
        }

        // Check 3D
        float checkDistance = 5f;
        RaycastHit hit;

        if (Physics.Raycast(position + Vector3.up * 0.5f, Vector3.down, out hit, checkDistance))
        {
            float distanceToGround = Mathf.Abs(position.y - hit.point.y);
            if (distanceToGround < 0.5f)
            {
                Debug.Log($"[EnemySpawnManager] Fallback 3D: Found ground below at {hit.point.y} (distance: {distanceToGround})");
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines if we're in a 2D game
    /// </summary>
    private bool Is2DGame()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null && mainCamera.orthographic)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the ground layer mask - PRIORITIZES TILEMAPS
    /// </summary>
    private LayerMask GetGroundLayerMask()
    {
        // FIRST: Try to find Tilemap ground layers (most important for your case)
        LayerMask tilemapLayer = FindTilemapGroundLayer();
        if (tilemapLayer != 0)
        {
            return tilemapLayer;
        }

        // SECOND: Try to get from GridManager
        if (GridManager.Instance != null)
        {
            var groundLayerField = typeof(GridManager).GetField("groundLayer");
            if (groundLayerField != null)
            {
                LayerMask groundMask = (LayerMask)groundLayerField.GetValue(GridManager.Instance);
                if (groundMask.value != 0)
                {
                    Debug.Log($"[EnemySpawnManager] Using groundLayer from GridManager: {groundMask.value}");
                    return groundMask;
                }
            }

            var floorLayerField = typeof(GridManager).GetField("floorLayer");
            if (floorLayerField != null)
            {
                LayerMask floorMask = (LayerMask)floorLayerField.GetValue(GridManager.Instance);
                if (floorMask.value != 0)
                {
                    Debug.Log($"[EnemySpawnManager] Using floorLayer from GridManager: {floorMask.value}");
                    return floorMask;
                }
            }
        }

        // THIRD: Try common layer names
        string[] layerNames = { "Ground", "Floor", "Platform", "Terrain", "Default" };
        foreach (string layerName in layerNames)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer != -1)
            {
                Debug.Log($"[EnemySpawnManager] Found layer '{layerName}' with index {layer}");
                return 1 << layer;
            }
        }

        Debug.LogWarning("[EnemySpawnManager] Could not determine ground layer!");
        return 0;
    }

    /// <summary>
    /// Gets the wall layer mask - PRIORITIZES TILEMAPS
    /// </summary>
    private LayerMask GetWallLayerMask()
    {
        // FIRST: Look for Tilemap colliders that might be walls
        TilemapCollider2D[] tilemapColliders = GameObject.FindObjectsOfType<TilemapCollider2D>();
        foreach (var tilemapCollider in tilemapColliders)
        {
            // If the Tilemap has "wall" in its name or is on a wall layer
            if (tilemapCollider.gameObject.name.ToLower().Contains("wall") ||
                tilemapCollider.gameObject.layer == LayerMask.NameToLayer("Walls"))
            {
                int layer = tilemapCollider.gameObject.layer;
                Debug.Log($"[EnemySpawnManager] Found wall Tilemap on layer {layer}");
                return 1 << layer;
            }
        }

        // SECOND: Try to get from GridManager
        if (GridManager.Instance != null)
        {
            var wallLayerField = typeof(GridManager).GetField("wallLayer");
            if (wallLayerField != null)
            {
                LayerMask wallMask = (LayerMask)wallLayerField.GetValue(GridManager.Instance);
                if (wallMask.value != 0)
                {
                    Debug.Log($"[EnemySpawnManager] Using wallLayer from GridManager: {wallMask.value}");
                    return wallMask;
                }
            }
        }

        // THIRD: Try common layer names
        string[] layerNames = { "Walls", "Wall", "Obstacles", "Obstacle", "Solid" };
        foreach (string layerName in layerNames)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer != -1)
            {
                Debug.Log($"[EnemySpawnManager] Found wall layer '{layerName}' with index {layer}");
                return 1 << layer;
            }
        }

        Debug.LogWarning("[EnemySpawnManager] Could not determine wall layer! Wall blocking checks disabled.");
        return 0;
    }

    /// <summary>
    /// Adjusts a position to snap to the ground level - FIXED FOR TILEMAPS
    /// </summary>
    private Vector3 SnapToGround(Vector3 position)
    {
        // Get the ground layer mask
        LayerMask groundMask = GetGroundLayerMask();

        if (groundMask == 0)
        {
            Debug.LogWarning("[EnemySpawnManager] Cannot snap to ground - no ground layer found!");
            return position;
        }

        // Try 2D first (for Tilemaps)
        if (Is2DGame() || HasTilemapCollider())
        {
            Vector2 origin2D = new Vector2(position.x, position.y + 2f);
            RaycastHit2D hit2D = Physics2D.Raycast(origin2D, Vector2.down, 5f, groundMask);
            if (hit2D.collider != null)
            {
                Vector3 snappedPos = position;
                snappedPos.y = hit2D.point.y + 0.1f;
                Debug.Log($"[EnemySpawnManager] Snapped position (2D) from {position} to {snappedPos}");
                return snappedPos;
            }
        }

        // Try 3D
        float checkDistance = 5f;
        RaycastHit hit;

        if (Physics.Raycast(position + Vector3.up * 2f, Vector3.down, out hit, checkDistance, groundMask))
        {
            Vector3 snappedPos = position;
            snappedPos.y = hit.point.y + 0.1f;
            Debug.Log($"[EnemySpawnManager] Snapped position (3D) from {position} to {snappedPos}");
            return snappedPos;
        }

        Debug.LogWarning($"[EnemySpawnManager] Could not snap position {position} to ground!");
        return position;
    }

    // Keep the original IsPositionBlocked method for backward compatibility
    private bool IsPositionBlocked(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, 0.5f);

        if (colliders.Length > 0)
        {
            foreach (var collider in colliders)
            {
                Debug.Log($"Blocking object at {position}: {collider.gameObject.name} (Layer: {collider.gameObject.layer})");
            }
            return true;
        }
        return false;
    }

    // Keep the original CheckCircleClear method for backward compatibility
    private bool CheckCircleClear(Vector3 center, float radius)
    {
        LayerMask wallMask = LayerMask.GetMask("Walls");
        if (GridManager.Instance != null && GridManager.Instance.groundLayer != 0)
        {
            wallMask = GridManager.Instance.groundLayer;
        }

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, radius, wallMask);

        if (hitColliders.Length > 0)
        {
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

        if (!activeEnemies.Contains(enemy))
            activeEnemies.Add(enemy);

        if (owner != null)
            enemy.transform.SetParent(owner);

        activeEnemies.RemoveAll(e => e == null);
    }

    public void UnregisterEnemy(GameObject enemy)
    {
        if (enemy == null) return;

        bool wasBoss = enemy.CompareTag("Boss");

        activeEnemies.Remove(enemy);
        activeEnemies.RemoveAll(e => e == null);

        if (wasBoss)
        {
            Debug.Log("[EnemySpawnManager] Boss defeated!");
            OnBossDefeated?.Invoke();
            isBossRoom = false;
        }

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

        Transform bSpawn = MapLoaderManager.Instance.BossSpawnPoint;
        if (bSpawn == null)
        {
            Debug.LogError("[EnemySpawnManager] Boss spawn point not found!");
            return;
        }

        Vector3 bossSpawnPos = SnapToGround(bSpawn.position);

        if (!IsPositionValidForSpawning(bossSpawnPos))
        {
            Debug.LogError($"[EnemySpawnManager] Boss spawn position {bossSpawnPos} is invalid!");
            return;
        }

        GameObject boss = Instantiate(bossPrefab, bossSpawnPos, Quaternion.identity);

        EnemyStats stats = boss.GetComponent<EnemyStats>();
        if (stats != null && MapProgressionManager.Instance != null)
        {
            stats.SetLevel(MapProgressionManager.Instance.CurrentEnemyLevel);
        }

        isBossRoom = true;
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

        if (CurrentSpawnAmount <= 0)
        {
            allEnemiesSpawned = true;
            CheckIfAllEnemiesDefeated();
        }

        Debug.Log("[EnemySpawnManager] Prepared for new room.");
    }

    private void CheckIfAllEnemiesDefeated()
    {
        activeEnemies.RemoveAll(e => e == null);

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
        spawnPointIndex = 0;
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
        Debug.Log($"  - Current Spawn Point Index: {spawnPointIndex}");
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
        if (maxEnemies < 1) maxEnemies = 1;
        if (spawnPointIndex < 0) spawnPointIndex = 0;
        if (spawnPoints != null && spawnPointIndex >= spawnPoints.Length)
        {
            spawnPointIndex = spawnPoints.Length - 1;
        }
    }
#endif
}
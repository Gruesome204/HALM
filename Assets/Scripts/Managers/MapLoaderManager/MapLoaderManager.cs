using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapLoaderManager : MonoBehaviour, IGameSystem
{
    // Singleton
    public static MapLoaderManager Instance { get; private set; }

    [Header("Map Prefabs")]
    [SerializeField] private GameObject[] normalMapPrefabs;
    [SerializeField] private GameObject bossMapPrefab;

    [Header("References")]
    [SerializeField] private Transform mapParent;

    [Header("Debug")]
    [SerializeField] private bool logDebugMessages = true;

    [Header("Testing")]
    [SerializeField] private bool useTestRoom = false;
    [SerializeField] private GameObject testRoomPrefab;
    [SerializeField] private bool loadTestRoomOnStart = true;
    [SerializeField] private bool testRoomIsBossRoom = false;
    [SerializeField] private GameObject testBossPrefab;

    // State
    private List<GameObject> mapSequence = new List<GameObject>();
    private int currentMapIndex = -1;
    private GameObject currentMap;
    private bool isInitialized = false;
    private bool isTestRoomLoaded = false;

    // Public Properties
    public GameObject CurrentMap => currentMap;
    public Transform PlayerSpawnPoint { get; private set; }
    public Transform BossSpawnPoint { get; private set; }
    public List<GameObject> ExitBlockerObjects { get; private set; } = new List<GameObject>();
    public GameObject ExitTriggerObject { get; private set; }
    public int InitializePriority => 4;
    public bool IsUsingTestRoom => useTestRoom && testRoomPrefab != null;

    // Events
    public event Action<GameObject> OnMapLoaded;
    public event Action OnMapSequenceGenerated;

    #region Unity Lifecycle
    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        if (!isInitialized)
        {
            Debug.LogError($"{name}: MapLoaderManager not properly initialized!");
        }
    }

    private void OnDestroy()
    {
        CleanupCurrentMap();
    }
    #endregion

    #region IGameSystem Implementation
    public void Initialize()
    {
        if (isInitialized) return;

        ValidateReferences();

        if (useTestRoom && testRoomPrefab != null)
        {
            LogMessage("Test room mode enabled. Skipping map sequence generation.");
        }
        else
        {
            GenerateMapSequence();
        }

        isInitialized = true;

        LogMessage("MapLoaderManager initialized successfully.");
    }

    public void PostInitialize()
    {
        if (currentMap == null)
        {
            if (useTestRoom && testRoomPrefab != null && loadTestRoomOnStart)
            {
                LoadTestRoom();
            }
            else
            {
                LoadNextMap();
            }
        }
    }
    #endregion

    #region Singleton Management
    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region Public Methods
    public void GenerateMapSequence()
    {
        if (useTestRoom && testRoomPrefab != null)
        {
            LogMessage("Test room mode active - map sequence generation skipped.");
            return;
        }

        mapSequence.Clear();

        if (normalMapPrefabs == null || normalMapPrefabs.Length == 0)
        {
            Debug.LogError("No normal map prefabs assigned!");
            return;
        }

        // Create and shuffle normal maps
        List<GameObject> shuffledMaps = new List<GameObject>(normalMapPrefabs);
        ShuffleList(shuffledMaps);
        mapSequence.AddRange(shuffledMaps);

        // Add boss map if available
        if (bossMapPrefab != null)
        {
            mapSequence.Add(bossMapPrefab);
        }

        currentMapIndex = -1;
        OnMapSequenceGenerated?.Invoke();

        LogMessage($"Generated map sequence with {mapSequence.Count} maps.");
    }

    public GameObject LoadNextMap()
    {
        if (!isInitialized)
        {
            Debug.LogError("MapLoaderManager not initialized. Call Initialize() first.");
            return null;
        }

        // If test room is active, don't load normal maps
        if (useTestRoom && testRoomPrefab != null)
        {
            LogMessage("Test room mode active - use LoadTestRoom() instead.");
            return null;
        }

        currentMapIndex++;

        if (currentMapIndex >= mapSequence.Count)
        {
            LogMessage("No more maps to load. All levels completed!");
            return null;
        }

        GameObject prefab = mapSequence[currentMapIndex];
        if (prefab == null)
        {
            Debug.LogError($"Map prefab at index {currentMapIndex} is null!");
            return null;
        }

        return LoadMapFromPrefab(prefab);
    }

    public void LoadSpecificMap(int index)
    {
        if (useTestRoom && testRoomPrefab != null)
        {
            LogMessage("Test room mode active - cannot load specific map index.");
            return;
        }

        if (index < 0 || index >= mapSequence.Count)
        {
            Debug.LogError($"Invalid map index: {index}. Valid range: 0-{mapSequence.Count - 1}");
            return;
        }

        currentMapIndex = index - 1; // -1 so LoadNextMap loads the correct one
        LoadNextMap();
    }

    public void ResetMapSequence()
    {
        if (useTestRoom && testRoomPrefab != null)
        {
            LogMessage("Test room mode active - reloading test room instead.");
            LoadTestRoom();
            return;
        }

        GenerateMapSequence();
        currentMapIndex = -1;
        CleanupCurrentMap();
    }

    /// <summary>
    /// Load the test room prefab directly
    /// </summary>
    public GameObject LoadTestRoom()
    {
        if (testRoomPrefab == null)
        {
            Debug.LogError("Test room prefab not assigned!");
            return null;
        }

        if (!useTestRoom)
        {
            Debug.LogWarning("Test room mode is disabled. Enable useTestRoom to load test rooms.");
            return null;
        }

        LogMessage("Loading test room...");
        isTestRoomLoaded = true;

        // Load the map
        GameObject loadedMap = LoadMapFromPrefab(testRoomPrefab);

        // If this is a boss room, spawn the boss using EnemySpawnManager
        if (testRoomIsBossRoom && loadedMap != null)
        {
            SpawnTestBoss();
        }

        return loadedMap;
    }

    /// <summary>
    /// Toggle between test room mode and normal map mode
    /// </summary>
    public void ToggleTestRoomMode(bool enable)
    {
        useTestRoom = enable;
        LogMessage($"Test room mode {(enable ? "enabled" : "disabled")}");

        if (enable && testRoomPrefab != null)
        {
            // If enabling test room mode, immediately load the test room
            LoadTestRoom();
        }
        else if (!enable)
        {
            // If disabling, reload the normal map sequence
            CleanupCurrentMap();
            currentMapIndex = -1;
            GenerateMapSequence();
            LoadNextMap();
        }
    }

    /// <summary>
    /// Set a different test room prefab at runtime
    /// </summary>
    public void SetTestRoomPrefab(GameObject newTestRoom)
    {
        if (newTestRoom == null)
        {
            Debug.LogError("Cannot set null test room prefab!");
            return;
        }

        testRoomPrefab = newTestRoom;
        LogMessage($"Test room prefab updated to: {newTestRoom.name}");

        if (useTestRoom)
        {
            LoadTestRoom();
        }
    }

    /// <summary>
    /// Set test room as boss room and assign boss prefab
    /// </summary>
    public void SetTestRoomAsBossRoom(bool isBossRoom, GameObject bossPrefab = null)
    {
        testRoomIsBossRoom = isBossRoom;
        if (bossPrefab != null)
        {
            testBossPrefab = bossPrefab;
        }

        LogMessage($"Test room boss mode: {(isBossRoom ? "Enabled" : "Disabled")}");

        if (useTestRoom && isTestRoomLoaded)
        {
            // Reload the test room to apply changes
            LoadTestRoom();
        }
    }

    /// <summary>
    /// Check if currently in test room mode
    /// </summary>
    public bool IsInTestRoomMode()
    {
        return IsUsingTestRoom && isTestRoomLoaded;
    }

    /// <summary>
    /// Check if test room is a boss room
    /// </summary>
    public bool IsTestRoomBossRoom()
    {
        return testRoomIsBossRoom;
    }
    #endregion

    #region Boss Spawning
    /// <summary>
    /// Spawn the test boss using EnemySpawnManager
    /// </summary>
    private void SpawnTestBoss()
    {
        if (!testRoomIsBossRoom)
        {
            LogMessage("Test room is not configured as a boss room.");
            return;
        }

        if (testBossPrefab == null)
        {
            Debug.LogError("Test boss prefab is not assigned! Cannot spawn boss in test room.");
            return;
        }

        // Use EnemySpawnManager to spawn the boss
        EnemySpawnManager spawnManager = EnemySpawnManager.Instance;
        if (spawnManager == null)
        {
            Debug.LogError("EnemySpawnManager instance not found! Cannot spawn boss.");
            return;
        }

        // Verify Boss Spawn Point exists
        if (BossSpawnPoint == null)
        {
            Debug.LogError("No BossSpawnPoint found in test room! Cannot spawn boss.");
            return;
        }

        // Use the EnemySpawnManager's spawn boss method
        spawnManager.SpawnBoss(testBossPrefab);

        // Update the boss room flag in EnemySpawnManager
        spawnManager.isBossRoom = true;

        LogMessage($"Test boss spawned successfully at {BossSpawnPoint.position}");
    }

    /// <summary>
    /// Manually spawn/despawn test boss
    /// </summary>
    public void SpawnTestBossManually()
    {
        if (!useTestRoom || !isTestRoomLoaded)
        {
            Debug.LogWarning("Cannot spawn test boss - not in test room mode!");
            return;
        }

        if (!testRoomIsBossRoom)
        {
            Debug.LogWarning("Test room is not set as a boss room!");
            return;
        }

        SpawnTestBoss();
    }

    public void DespawnTestBossManually()
    {
        EnemySpawnManager spawnManager = EnemySpawnManager.Instance;
        if (spawnManager != null)
        {
            spawnManager.ClearAllEnemies();
            spawnManager.isBossRoom = false;
            LogMessage("Test boss despawned.");
        }
    }
    #endregion

    #region Map Loading
    private GameObject LoadMapFromPrefab(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("Cannot load null map prefab!");
            return null;
        }

        // Validate required dependencies
        if (EnemySpawnManager.Instance == null)
        {
            Debug.LogError("EnemySpawnManager missing!");
            return null;
        }

        // Clean up old map
        CleanupCurrentMap();

        // Instantiate new map
        currentMap = Instantiate(prefab, mapParent);
        if (currentMap == null)
        {
            Debug.LogError($"Failed to instantiate map from prefab: {prefab.name}");
            return null;
        }

        // Setup map components
        SetupMapComponents();
        AssignSpawnPoints();
        ConfigureEnemySpawner();
        SetPlayerPosition();

        OnMapLoaded?.Invoke(currentMap);

        string mapType = (useTestRoom && testRoomPrefab == prefab) ? "TEST ROOM" : "map";
        LogMessage($"Loaded {mapType}: {prefab.name}");

        return currentMap;
    }

    private void CleanupCurrentMap()
    {
        if (currentMap != null)
        {
            Destroy(currentMap);
            currentMap = null;
        }

        ExitBlockerObjects.Clear();
        ExitTriggerObject = null;
        PlayerSpawnPoint = null;
        BossSpawnPoint = null;
        isTestRoomLoaded = false;
    }

    private void SetupMapComponents()
    {
        if (currentMap == null) return;

        // Find exit blockers
        ExitBlockerObjects = FindAllObjectsInChildrenWithTag(currentMap, "ExitBlocker");

        // Find exit trigger
        ExitTriggerObject = FindObjectInChildrenWithTag(currentMap, "ExitTrigger");

        // Apply map-specific enemy setup
        MapEnemySetup setup = currentMap.GetComponent<MapEnemySetup>();
        if (setup != null)
        {
            // For test rooms, we might want to override the boss room setting
            if (useTestRoom && testRoomPrefab != null)
            {
                // Test room settings override
                EnemySpawnManager.Instance.isBossRoom = testRoomIsBossRoom;

                // Don't clear enemy prefabs for test rooms if we're spawning a boss
                if (testRoomIsBossRoom)
                {
                    // Keep existing enemy prefabs or set to empty for boss room
                    if (setup.enemyPrefabs != null && setup.enemyPrefabs.Count > 0)
                    {
                        EnemySpawnManager.Instance.enemyPrefabs = new List<GameObject>(setup.enemyPrefabs);
                    }
                    else
                    {
                        EnemySpawnManager.Instance.enemyPrefabs = new List<GameObject>();
                    }
                }
                else
                {
                    // Normal test room - use setup's enemy prefabs
                    if (setup.enemyPrefabs != null && setup.enemyPrefabs.Count > 0)
                    {
                        EnemySpawnManager.Instance.enemyPrefabs = new List<GameObject>(setup.enemyPrefabs);
                    }
                }
            }
            else
            {
                // Normal map loading
                EnemySpawnManager.Instance.isBossRoom = setup.isBossRoom;
                if (setup.enemyPrefabs != null && setup.enemyPrefabs.Count > 0)
                {
                    EnemySpawnManager.Instance.enemyPrefabs = new List<GameObject>(setup.enemyPrefabs);
                }
            }
        }
        else
        {
            Debug.LogWarning($"No MapEnemySetup found on {currentMap.name}");
        }
    }
    #endregion

    #region Spawn Point Management
    private void AssignSpawnPoints()
    {
        if (currentMap == null) return;

        // Find Enemy Spawn Points
        Transform spawnRoot = currentMap.transform.Find("EnemySpawnPoints");
        if (spawnRoot != null)
        {
            Transform[] spawnPoints = new Transform[spawnRoot.childCount];
            for (int i = 0; i < spawnRoot.childCount; i++)
            {
                spawnPoints[i] = spawnRoot.GetChild(i);
            }
            EnemySpawnManager.Instance.spawnPoints = spawnPoints;
            LogMessage($"Found {spawnPoints.Length} enemy spawn points.");
        }
        else
        {
            EnemySpawnManager.Instance.spawnPoints = Array.Empty<Transform>();
            Debug.LogWarning($"No 'EnemySpawnPoints' child found in {currentMap.name}");
        }

        // Find Player Spawn Point
        Transform pSpawn = currentMap.transform.Find("PlayerSpawnPoint");
        PlayerSpawnPoint = pSpawn;
        if (pSpawn == null)
        {
            Debug.LogError($"PlayerSpawnPoint missing in {currentMap.name}!");
        }

        // Find Boss Spawn Point
        BossSpawnPoint = currentMap.transform.Find("BossSpawnPoint");
        if (BossSpawnPoint == null && (EnemySpawnManager.Instance.isBossRoom || testRoomIsBossRoom))
        {
            Debug.LogWarning($"BossSpawnPoint missing in boss room: {currentMap.name}");
        }
    }

    private void ConfigureEnemySpawner()
    {
        EnemySpawnManager spawner = EnemySpawnManager.Instance;
        if (spawner == null) return;

        spawner.ResetSpawner();
    }

    private void SetPlayerPosition()
    {
        if (PlayerSpawnPoint == null) return;

        PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
        if (player != null)
        {
            player.transform.position = PlayerSpawnPoint.position;
            LogMessage($"Player positioned at {PlayerSpawnPoint.position}");
        }
        else
        {
            Debug.LogWarning("PlayerMovement not found in scene!");
        }
    }
    #endregion

    #region Helper Methods
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    private GameObject FindObjectInChildrenWithTag(GameObject parent, string tag)
    {
        if (parent == null || string.IsNullOrEmpty(tag)) return null;

        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.CompareTag(tag))
                return child.gameObject;
        }
        return null;
    }

    private List<GameObject> FindAllObjectsInChildrenWithTag(GameObject parent, string tag)
    {
        List<GameObject> result = new List<GameObject>();
        if (parent == null || string.IsNullOrEmpty(tag)) return result;

        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.CompareTag(tag))
                result.Add(child.gameObject);
        }
        return result;
    }

    private void ValidateReferences()
    {
        if (normalMapPrefabs == null || normalMapPrefabs.Length == 0)
        {
            Debug.LogError("No normal map prefabs assigned in inspector!");
        }

        if (mapParent == null)
        {
            mapParent = transform;
            Debug.LogWarning("mapParent not assigned, using current transform.");
        }

        if (EnemySpawnManager.Instance == null)
        {
            Debug.LogError("EnemySpawnManager.Instance is null! Make sure it exists in scene.");
        }

        if (useTestRoom && testRoomPrefab == null)
        {
            Debug.LogError("Test room mode enabled but testRoomPrefab is not assigned!");
        }

        if (testRoomIsBossRoom && testBossPrefab == null)
        {
            Debug.LogError("Test room is set as boss room but testBossPrefab is not assigned!");
        }
    }

    private void LogMessage(string message)
    {
        if (logDebugMessages)
        {
            Debug.Log($"[MapLoaderManager] {message}");
        }
    }
    #endregion
}
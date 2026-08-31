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

    // State
    private List<GameObject> mapSequence = new List<GameObject>();
    private int currentMapIndex = -1;
    private GameObject currentMap;
    private bool isInitialized = false;

    // Public Properties
    public GameObject CurrentMap => currentMap;
    public Transform PlayerSpawnPoint { get; private set; }
    public Transform BossSpawnPoint { get; private set; }
    public List<GameObject> ExitBlockerObjects { get; private set; } = new List<GameObject>();
    public GameObject ExitTriggerObject { get; private set; }
    public int InitializePriority => 4;

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
        GenerateMapSequence();
        isInitialized = true;

        LogMessage("MapLoaderManager initialized successfully.");
    }

    public void PostInitialize()
    {
        if (currentMap == null)
        {
            LoadNextMap();
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
        GenerateMapSequence();
        currentMapIndex = -1;
        CleanupCurrentMap();
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
        LogMessage($"Loaded map: {prefab.name}");

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
            EnemySpawnManager.Instance.isBossRoom = setup.isBossRoom;
            if (setup.enemyPrefabs != null && setup.enemyPrefabs.Count > 0)
            {
                EnemySpawnManager.Instance.enemyPrefabs = new List<GameObject>(setup.enemyPrefabs);
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
        if (BossSpawnPoint == null && EnemySpawnManager.Instance.isBossRoom)
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
using UnityEngine;

public class MapProgressionManager : MonoBehaviour
{
    public static MapProgressionManager Instance { get; private set; }

    [Header("Progression Settings")]
    public int mapsBeforeBoss = 3;


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
        // Clear all towers before loading a new map
        TurretPlacementController.Instance?.ClearAllTurrets();

        // Clamp map index
        int safeMapIndex = Mathf.Min(currentMapIndex, MapLoaderManager.Instance.mapPrefabs.Length - 1);

        // Load map normally
        MapLoaderManager.Instance.LoadMap(safeMapIndex);

        bool shouldSpawnBoss = mapsClearedSinceBoss >= mapsBeforeBoss;

        if (shouldSpawnBoss)
        {
            bossActive = true;
            EnemySpawnManager.Instance.SpawnBoss();
            mapsClearedSinceBoss = 0; // reset after boss
        }
        else
        {
            bossActive = false;
            EnemySpawnManager.Instance.OnAllEnemiesDefeated += OnRoomCleared;
            mapsClearedSinceBoss++; // increment only if not boss
        }

        currentMapIndex++;
    }
        private void OnRoomCleared()
    {
        // Prevent multiple fires
        EnemySpawnManager.Instance.OnAllEnemiesDefeated -= OnRoomCleared;

        Debug.Log("[Progression] Room cleared → loading next room...");
        LoadNextRoom();
    }

}

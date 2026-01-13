using UnityEngine;

public class MapProgressionManager : MonoBehaviour
{
    public static MapProgressionManager Instance { get; private set; }

    [Header("Enemy Level Scaling")]
    public int baseEnemyLevel = 1;
    public int enemyLevelIncreasePerMap = 1;
    public int CurrentEnemyLevel { get; private set; }

    [Header("Progression Options")]
    [Tooltip("If true, rooms auto-load after clearing. If false, player must interact to proceed.")]
    public bool autoProgress = false;

    public bool roomClearedWaitingForPlayer = false;

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

        MapLoaderManager.Instance.GenerateMapSequence();
        LoadNextRoom();
    }

    public void LoadNextRoom()
    {
        PlayerManager.Instance.playerHealth.Heal(10);
        TurretPlacementController.Instance?.ClearAllTurrets();

        GameObject map = MapLoaderManager.Instance.LoadNextMap();
        if (map == null) return;

        EnemySpawnManager spawner = EnemySpawnManager.Instance;
        spawner.PrepareForNewRoom();

        MapEnemySetup setup = map.GetComponent<MapEnemySetup>();

        if (setup != null && setup.isBossRoom)
        {
            spawner.OnBossDefeated -= OnRoomCleared;
            spawner.OnBossDefeated += OnRoomCleared;
            spawner.SpawnBoss(setup.bossPrefab);
        }
        else
        {
            spawner.OnAllEnemiesDefeated -= OnRoomCleared;
            spawner.OnAllEnemiesDefeated += OnRoomCleared;
        }

        CurrentEnemyLevel += enemyLevelIncreasePerMap;
    }

    private void OnRoomCleared()
    {
        // Prevent multiple fires
        EnemySpawnManager.Instance.OnAllEnemiesDefeated -= OnRoomCleared;
        EnemySpawnManager.Instance.OnBossDefeated -= OnRoomCleared;

        Debug.Log("[Progression] Room cleared!");

        if (autoProgress)
        {
            Debug.Log("[Progression] Auto-loading next room...");
            LoadNextRoom();
        }
        else
        {
            roomClearedWaitingForPlayer = true;
            // Enable all blockers when room is cleared
            foreach (var blocker in MapLoaderManager.Instance.ExitBlockerObjects)
            {
                if (blocker != null)
                    blocker.SetActive(true);
            }

            // Disable trigger until blocker is removed
            if (MapLoaderManager.Instance.ExitTriggerObject != null)
                MapLoaderManager.Instance.ExitTriggerObject.SetActive(false);

            Debug.Log("[Progression] Waiting for player to interact with exit...");
        }
    }
    public void PlayerTriggerNextRoom()
    {
        if (!roomClearedWaitingForPlayer) return;

        roomClearedWaitingForPlayer = false;
        LoadNextRoom();
    }

    public void PlayerClickedExitBlocker(GameObject clickedBlocker)
    {
        if (!roomClearedWaitingForPlayer) return;
        clickedBlocker.SetActive(false);

        Debug.Log("[Progression] Exit Blocker removed, activating ExitTrigger.");

        // Activate trigger so player can walk through
        if (MapLoaderManager.Instance.ExitTriggerObject != null)
            MapLoaderManager.Instance.ExitTriggerObject.SetActive(true);
    }

    public void ResetProgression()
    {
        CurrentEnemyLevel = baseEnemyLevel;
    }
}

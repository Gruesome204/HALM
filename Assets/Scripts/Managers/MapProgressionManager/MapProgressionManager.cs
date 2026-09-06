using System.Collections.Generic;
using UnityEngine;

public class MapProgressionManager : MonoBehaviour, IGameSystem
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
    private bool isLoadingNextRoom = false;

    [Header("Debug Options")]
    [Tooltip("Enable debug features for testing")]
    public bool enableDebug = false;

    [Tooltip("Key to press to force load next room (only works if enableDebug is true)")]
    public KeyCode forceNextRoomKey = KeyCode.N;

    [Tooltip("Key to press to force clear current room (only works if enableDebug is true)")]
    public KeyCode forceClearRoomKey = KeyCode.C;

    public int InitializePriority => 2;
    public void Initialize()
    {
      
    }

    public void PostInitialize()
    {
        CurrentEnemyLevel = baseEnemyLevel;

        LoadNextRoom();
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (!enableDebug) return;

        // Force load next room
        if (Input.GetKeyDown(forceNextRoomKey))
        {
            Debug.Log("[DEBUG] Force loading next room...");
            if (roomClearedWaitingForPlayer)
            {
                PlayerTriggerNextRoom();
            }
            else
            {
                // Force complete current room and load next
                Debug.Log("[DEBUG] Room not cleared, forcing completion...");
                OnRoomCleared(); // This will trigger auto-progress if enabled, or set waiting state

                // If autoProgress is false, we need to manually trigger the next room
                if (!autoProgress && roomClearedWaitingForPlayer)
                {
                    Debug.Log("[DEBUG] Auto-progress disabled, forcing next room load...");
                    PlayerTriggerNextRoom();
                }
            }
        }

        // Force clear current room (trigger OnRoomCleared without killing enemies)
        if (Input.GetKeyDown(forceClearRoomKey))
        {
            Debug.Log("[DEBUG] Force clearing current room...");
            if (!isLoadingNextRoom && !roomClearedWaitingForPlayer)
            {
                OnRoomCleared();
            }
            else if (roomClearedWaitingForPlayer)
            {
                Debug.Log("[DEBUG] Room already cleared, use force next room key to proceed.");
            }
            else
            {
                Debug.Log("[DEBUG] Cannot clear room while loading next room.");
            }
        }
    }

    public void LoadNextRoom()
    {
        if (isLoadingNextRoom) return;
        isLoadingNextRoom = true;
        PlayerManager.Instance.Health.Heal(10);
        TurretPlacementController.Instance?.ClearAllTurrets();

        GameObject map = MapLoaderManager.Instance.LoadNextMap();
        if (map == null)
        {
            isLoadingNextRoom = false;
            return;
        }

        if (MapLoaderManager.Instance.IsUsingTestRoom)
        {
            Debug.Log("[Progression] Test room loaded - skipping automatic enemy spawning");
            isLoadingNextRoom = false;
            return;
        }
        EnemySpawnManager spawner = EnemySpawnManager.Instance;
        spawner.PrepareForNewRoom();

        MapEnemySetup setup = map.GetComponent<MapEnemySetup>();

        // Unsubscribe first to avoid multiple calls
        spawner.OnAllEnemiesDefeated -= OnRoomCleared;
        spawner.OnBossDefeated -= OnRoomCleared;

        if (setup != null && setup.isBossRoom)
        {
            if (setup.bossPrefab == null)
            {
                Debug.LogError("[Progression] Boss room has no boss prefab!");
                isLoadingNextRoom = false;
                return;
            }

            // Make sure normal enemies won't spawn
            spawner.isBossRoom = true;

            // Subscribe to boss defeat
            spawner.OnBossDefeated += OnRoomCleared;

            // Spawn the boss
            spawner.SpawnBoss(setup.bossPrefab);

            Debug.Log("[Progression] Boss room loaded - boss spawned");
        }
        else
        {
            // Normal room - spawn regular enemies
            spawner.isBossRoom = false;
            spawner.OnAllEnemiesDefeated += OnRoomCleared;

            // The spawner will automatically start spawning enemies in its Update() method
            // Reset the spawner to ensure it starts fresh
            spawner.ResetSpawner();

            Debug.Log($"[Progression] Normal room loaded - will spawn {spawner.CurrentSpawnAmount} enemies");
        }

        CurrentEnemyLevel += enemyLevelIncreasePerMap;
        isLoadingNextRoom = false;
    }

    private void OnRoomCleared()
    {
        // Prevent multiple fires
        EnemySpawnManager.Instance.OnAllEnemiesDefeated -= OnRoomCleared;
        EnemySpawnManager.Instance.OnBossDefeated -= OnRoomCleared;

        Debug.Log("[Progression] Room cleared!");

        // For test rooms, we might want different behavior
        if (MapLoaderManager.Instance.IsUsingTestRoom)
        {
            Debug.Log("[Progression] Test room cleared!");
            return;
        }

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
    public bool IsTestRoomMode()
    {
        return MapLoaderManager.Instance != null && MapLoaderManager.Instance.IsUsingTestRoom;
    }
}
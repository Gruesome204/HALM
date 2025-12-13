using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Data")]
    public GameDataSO gameDataSO;
    [SerializeField] private GameDataDefaultsSO defaultData;

    public TempSaveData tempSaveData;
    public enum GameState
    {
        MainMenu,
        Playing,
        Loading,
        Paused,
        Stats,
        GameOver
    }

    public GameState CurrentState { get; private set; }
    public GameState PreviousState { get; private set; }

    public event Action<GameState, GameState> OnGameStateChanged;
    public event Action OnSaveReset;
    public event System.Action OnGameReset;

    [SerializeField]private readonly List<IPausable> pausables = new List<IPausable>();

    [Header("Autosave")]
    [SerializeField] private float autosaveInterval = 60f;
    private float autosaveTimer = 0f;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // only one instance allowed
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // survive scene loads
    }

    private void Start()
    {
        StartCoroutine(LoadGameRoutine());
    }

    private System.Collections.IEnumerator LoadGameRoutine()
    {
        ChangeState(GameState.Loading);

        TempSaveData loadedData = SaveSystem.Load();
        if (loadedData != null)
        {
                tempSaveData = loadedData;
            ApplyRuntimeDataToSO();
        }
        else
        {
            Debug.Log("[GameManager] No save found → creating new GameData from SO defaults");
                tempSaveData = new TempSaveData
            {
                gameCurrency = gameDataSO.gameCurrency,
                currentPlayerLevel = gameDataSO.currentPlayerLevel,
                currentClass = gameDataSO.currentClass,
                unlockedBlueprints = new List<TurretBlueprint>(gameDataSO.GetUnlockedBlueprints()),
            };


    }


        // Setup turret placement system
        TurretPlacementController.Instance?.SetupFromGameData(gameDataSO);


        // Load Scene Elements (Map)
        MapLoaderManager.Instance?.LoadMap(0);


        // WAIT until the player is spawned and has PlayerStats
        yield return new WaitUntil(() =>
            PlayerManager.Instance != null
        );

        // Apply upgrades now that player exists
        ApplyPlayerUpgrades();

        ChangeState(GameState.Playing);
    }

    private void ApplyPlayerUpgrades()  
    {
        var so = tempSaveData;
        PlayerStats playerStats = PlayerManager.Instance.playerStats;
        playerStats.currentHealth += so.additionalHealth;
        playerStats.currentMaxHealth += so.additionalMaxHealth;
        playerStats.currentRegen += so.additionalRegen;
        playerStats.currentArmor += so.additionalArmor;
        playerStats.currentMagicResistance += so.additionalMagicResistance;
    }

    private void ApplyRuntimeDataToSO()
    {
        if (gameDataSO == null)
        {
            Debug.LogWarning("[GameManager] Cannot apply runtime data: gameDataSO is null.");
            return;
        }

        if (tempSaveData == null)
        {
            Debug.Log("[GameManager] No save data found, keeping defaults from GameDataSO.");
            return; // don't overwrite defaults
        }

        // Copy basic fields
        gameDataSO.gameCurrency = tempSaveData.gameCurrency;
        gameDataSO.currentPlayerLevel = tempSaveData.currentPlayerLevel;
        gameDataSO.currentClass = tempSaveData.currentClass;

        Debug.Log("[GameManager] Runtime data applied to GameDataSO.");
    }

    private void Update()
    {
        if (CurrentState != GameState.Playing)
            return;

        autosaveTimer += Time.deltaTime;

        if (autosaveTimer >= autosaveInterval)
        {
            autosaveTimer = 0f;
            SaveGame();
            Debug.Log("Auto-saved.");
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            SaveGame();
            Debug.Log("Auto-saved.");
        }
    }


    public void SaveGame()
    {
        if (tempSaveData == null) tempSaveData = new TempSaveData();
        if (gameDataSO == null) return;

        tempSaveData.gameCurrency = gameDataSO.gameCurrency;
        tempSaveData.currentPlayerLevel = gameDataSO.currentPlayerLevel;
        tempSaveData.currentClass = gameDataSO.currentClass;
        tempSaveData.unlockedBlueprints = gameDataSO.GetUnlockedBlueprints().ToList();

        SaveSystem.Save(tempSaveData);
    }


    public void ChangeState(GameState newState)
    {
        if (newState == CurrentState) return;

        PreviousState = CurrentState;
        CurrentState = newState;
        Debug.Log($"[GameManager] State changed: {PreviousState} → {CurrentState}");
        OnGameStateChanged?.Invoke(CurrentState, PreviousState); 

        UpdatePausables(newState);
    }

    private void UpdatePausables(GameState newState)
    {
        foreach (var p in pausables)
        {
            if (p == null) continue;

            switch (newState)
            {
                case GameState.GameOver:
                    p.OnPause();
                    break;
                case GameState.Paused:
                    p.OnPause();
                    break;
                case GameState.Playing:
                    p.OnResume();
                    break;
            }
        }
    }

    public void RegisterPausable(IPausable pausable)
    {
        if (!pausables.Contains(pausable))
            pausables.Add(pausable);

        // Immediately notify of current state
        if (CurrentState == GameState.Paused)
            pausable.OnPause();
        else if (CurrentState == GameState.Playing)
            pausable.OnResume();
    }

    public void UnregisterPausable(IPausable pausable)
    {
        if (pausable == null) return;
        pausables.Remove(pausable);
    }

    public void PauseGame() => ChangeState(GameState.Paused);
    public void ResumeGame() => ChangeState(GameState.Playing);

    public bool IsPlaying() => CurrentState == GameState.Playing;
    public bool IsPaused() => CurrentState == GameState.Paused;

    public void ResetGame()
    {
        Debug.Log("[GameManager] FULL GAME RESET");

        // 1. Delete save files
        SaveSystem.DeleteSaveFiles();

        // 2. Reset SO to defaults
        gameDataSO.ResetToDefaults(defaultData);

        // 3. Create fresh runtime save
        tempSaveData = new TempSaveData(gameDataSO);

        // 4. Notify listeners
        OnGameReset?.Invoke();

        // 5. Go to main menu or reload
        ChangeState(GameState.MainMenu);
    }


    private void OnApplicationQuit()
    {
        Debug.Log("[GameManager] Application quitting → Saving game.");
        SaveGame();
    }

    [ContextMenu("RESET SAVE DATA")]
    private void ResetSaveFromEditor()
    {
        ResetGame();
        SaveGame();
    }
}

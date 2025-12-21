using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Data")]
    public GameDataSO gameDataSO;
    [SerializeField] private GameDataDefaultsSO defaultData;
    private TempSaveData tempSaveData;

    [Header("In-Game Timer")]
    [SerializeField] private float playTimeSeconds;
    public float PlayTimeSeconds => playTimeSeconds;
    public TimeSpan PlayTime => TimeSpan.FromSeconds(playTimeSeconds);

    public event Action<float> OnPlayTimeUpdated;
    private float timerTick;

    public enum GameState
    {
        MainMenu,
        HubMenu,
        Playing,
        Loading,
        Paused,
        Stats,
        GameOver
    }

    public GameState CurrentState { get; private set; }
    public GameState PreviousState { get; private set; }

    public event Action<GameState, GameState> OnGameStateChanged;

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
        Scene activeScene = SceneManager.GetActiveScene();

        if (activeScene.name == "GameScene") // replace with your gameplay scene name
        {
            StartCoroutine(LoadGameRoutine());
        }else
        if (activeScene.name == "HubScene") // replace with your gameplay scene name
        {
            ChangeState(GameState.HubMenu);
        }
        else
        {
            ChangeState(GameState.MainMenu); // or HubMenu
        }
    }

    private System.Collections.IEnumerator LoadGameRoutine()
    {
        ChangeState(GameState.Loading);

        TempSaveData loadedData = SaveSystem.Load();
        if (loadedData != null)
        {
            tempSaveData = loadedData;
        }
        else
        {
            Debug.Log("[GameManager] No save found → creating new GameData from SO defaults");
            tempSaveData = gameDataSO.ToSaveData();
        }

        // Ensure lists are not null
        tempSaveData.unlockedBlueprintNames ??= new List<string>();
        tempSaveData.buildMasterModifiers ??= new List<BuildMasterModifier>();
        playTimeSeconds = tempSaveData.playTimeSeconds;

        // Apply to SO
        ApplyRuntimeDataToSO();


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

    private void Update()
    {
        if (CurrentState != GameState.Playing)
            return;

        //In-game timer
        playTimeSeconds += Time.deltaTime;
        timerTick += Time.deltaTime;

        if (timerTick >= 1f)
        {
            timerTick = 0f;
            OnPlayTimeUpdated?.Invoke(playTimeSeconds);
        }

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
        if (gameDataSO == null) return;

        // Sync SO runtime values that can change in gameplay
        tempSaveData.playTimeSeconds = playTimeSeconds;

        // Settings
        tempSaveData.masterVolume = gameDataSO.masterVolume;
        tempSaveData.musicVolume = gameDataSO.musicVolume;
        tempSaveData.soundVolume = gameDataSO.soundVolume;
        tempSaveData.musicTrack = gameDataSO.musicTrack;
        tempSaveData.localSelected = gameDataSO.localSelected;

        // Player
        tempSaveData.currentPlayerLevel = gameDataSO.currentPlayerLevel;
        tempSaveData.currentClass = gameDataSO.currentClass;

        // Resources
        tempSaveData.gameCurrency = gameDataSO.gameCurrency;
        tempSaveData.woodResource = gameDataSO.woodResource;
        tempSaveData.steinResource = gameDataSO.steinResource;
        tempSaveData.metallResource = gameDataSO.metallResource;
        tempSaveData.pulverResource = gameDataSO.pulverResource;    

        // Turrets
        tempSaveData.unlockedBlueprintNames = gameDataSO.GetUnlockedBlueprints()
            .Where(b => b != null)
            .Select(b => b.name)
            .ToList();

        tempSaveData.buildMasterModifiers = new List<BuildMasterModifier>(gameDataSO.buildMasterModifiers);

        // Save to disk
        SaveSystem.Save(tempSaveData);
        Debug.Log("[GameManager] Game saved.");
    }

    private void ApplyRuntimeDataToSO()
    {
        if (tempSaveData != null && gameDataSO != null)
        {
            gameDataSO.ApplySave(tempSaveData);
            Debug.Log("[GameManager] Runtime data fully applied to GameDataSO.");
        }
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
        playTimeSeconds = 0f;

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

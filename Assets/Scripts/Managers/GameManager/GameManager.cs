using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Data")]
    public GameDataSO gameDataSO;                 // Asset reference (defaults)
    [SerializeField] private GameDataDefaultsSO defaultData;

    // Runtime clone for gameplay
    public GameDataSO RuntimeGameData { get; private set; }
    private TempSaveData tempSaveData;

    [Header("In-Game Timer")]
    [SerializeField] private float playTimeSeconds;
    public float PlayTimeSeconds => playTimeSeconds;
    public TimeSpan PlayTime => TimeSpan.FromSeconds(playTimeSeconds);

    public event Action<float> OnPlayTimeUpdated;
    private float timerTick;

    public enum GameState { MainMenu, HubMenu, Playing, Loading, Paused, Stats, GameOver }
    public GameState CurrentState { get; private set; }
    public GameState PreviousState { get; private set; }
    public event Action<GameState, GameState> OnGameStateChanged;

    [SerializeField] private readonly List<IPausable> pausables = new();

    [Header("Autosave")]
    [SerializeField] private float autosaveInterval = 60f;
    private float autosaveTimer = 0f;

    #region Unity Callbacks

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create a runtime clone of the ScriptableObject to store loaded values
        RuntimeGameData = Instantiate(gameDataSO);

        LoadOrCreateSave();
    }

    private void OnEnable() => SceneManager.sceneUnloaded += OnSceneUnloaded;
    private void OnDisable() => SceneManager.sceneUnloaded -= OnSceneUnloaded;

    private void OnSceneUnloaded(Scene scene)
    {
        Debug.Log($"[GameManager] Scene '{scene.name}' unloaded → saving.");
        SaveGame();
    }

    private void Start()
    {
        Scene activeScene = SceneManager.GetActiveScene();

        if (activeScene.name == "GameScene") 
        StartCoroutine(LoadGameRoutine());
        else if (activeScene.name == "HubScene")
        {
            ChangeState(GameState.HubMenu);
        }
        else
        {
            ChangeState(GameState.MainMenu);
        }
    }

    private void Update()
    {
        if (CurrentState != GameState.Playing)
            return;

        UpdatePlayTimer();
        HandleAutosave();
        HandleDebugInput();
    }

    private void OnApplicationQuit()
    {
        Debug.Log("[GameManager] Application quitting → saving.");
        SaveGame();
    }

    #endregion

    #region Save / Load

    private void LoadOrCreateSave()
    {
        TempSaveData loadedData = SaveSystem.Load();
        Debug.Log("Loaded TempSaveData: " + JsonUtility.ToJson(loadedData));
        if (loadedData != null)
        {
            tempSaveData = loadedData;
            Debug.Log("[GameManager] Save file loaded.");
        }
        else
        {
            Debug.Log("[GameManager] No save found → using defaults.");
            RuntimeGameData.ResetToDefaults(defaultData);
            tempSaveData = RuntimeGameData.ToSaveData();
            SaveSystem.Save(tempSaveData);
        }


        ApplySave(tempSaveData); // Apply to runtime clone
    }

    public void SaveGame()
    {
        if (RuntimeGameData == null)
            return;

        if (tempSaveData == null)
            tempSaveData = RuntimeGameData.ToSaveData();

        // Settings
        tempSaveData.masterVolume = RuntimeGameData.masterVolume;
        tempSaveData.musicVolume = RuntimeGameData.musicVolume;
        tempSaveData.soundVolume = RuntimeGameData.soundVolume;
        tempSaveData.musicTrack = RuntimeGameData.musicTrack;
        tempSaveData.localSelected = RuntimeGameData.localSelected;

        // Player
        tempSaveData.currentPlayerLevel = RuntimeGameData.currentPlayerLevel;
        tempSaveData.currentClass = RuntimeGameData.currentClass;

        // Resources
        tempSaveData.gameCurrency = RuntimeGameData.gameCurrency;
        tempSaveData.woodResource = RuntimeGameData.woodResource;
        tempSaveData.steinResource = RuntimeGameData.steinResource;
        tempSaveData.metallResource = RuntimeGameData.metallResource;
        tempSaveData.pulverResource = RuntimeGameData.pulverResource;

        // Turrets
        tempSaveData.unlockedBlueprintNames = RuntimeGameData.GetUnlockedBlueprints()
            .Where(b => b != null)
            .Select(b => b.name)
            .ToList();

        tempSaveData.buildMasterModifiers = new List<BuildMasterModifier>(RuntimeGameData.buildMasterModifiers);

        // Save to disk
        SaveSystem.Save(tempSaveData);
        Debug.Log("[GameManager] Game saved.");
    }
    public void ApplySave(TempSaveData data)
    {
        RuntimeGameData.ApplySave(data);
    }


    #endregion

    #region Gameplay Timer

    private void UpdatePlayTimer()
    {
        playTimeSeconds += Time.deltaTime;
        timerTick += Time.deltaTime;

        if (timerTick >= 1f)
        {
            timerTick = 0f;
            OnPlayTimeUpdated?.Invoke(playTimeSeconds);
        }
    }

    #endregion

    #region Autosave & Debug

    private void HandleAutosave()
    {
        autosaveTimer += Time.deltaTime;

        if (autosaveTimer >= autosaveInterval)
        {
            autosaveTimer = 0f;
            SaveGame();
        }
    }

    private void HandleDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.H))
            SaveGame();
    }

    #endregion

    #region Game State & Pausables

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

    #endregion

    #region Reset Game

    [ContextMenu("RESET SAVE DATA")]
    private void ResetSaveFromEditor() => ResetGame();

    public void ResetGame()
    {
        Debug.Log("[GameManager] FULL GAME RESET");
        SaveSystem.DeleteSaveFiles();
        RuntimeGameData.ResetToDefaults(defaultData);
        tempSaveData = null;
        Debug.Log("[GameManager] Save deleted");
    }

    #endregion

    #region Load Game Routine

    private System.Collections.IEnumerator LoadGameRoutine()
    {
        ChangeState(GameState.Loading);

        // Setup systems
        TurretPlacementController.Instance?.SetupFromGameData(RuntimeGameData);
        MapLoaderManager.Instance?.LoadMap(0);

        yield return new WaitUntil(() => PlayerManager.Instance != null);

        ChangeState(GameState.Playing);
    }

    #endregion
}

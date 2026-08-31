using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour, IGameSystem
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] public GameDataSO gameDataSO;
    [SerializeField] public GameDataDefaultsSO defaultDataSO;


    [Header("In-Game Timer")]
    [SerializeField] private float playTimeSeconds;
    public float PlayTimeSeconds => playTimeSeconds;
    public TimeSpan PlayTime => TimeSpan.FromSeconds(playTimeSeconds);
    private float timerTick;
    private const float TIMER_INTERVAL = 1f;

    public event Action<float> OnPlayTimeUpdated;

    public GameState CurrentState { get; private set; }
    public GameState PreviousState { get; private set; }
    public event Action<GameState, GameState> OnGameStateChanged;

    [SerializeField] private readonly List<IPausable> pausables = new();


    // IGameSystem implementation
    public int InitializePriority => 0; // Highest priority for GameManager

    public void Initialize()
    {
        // Critical initialization that must happen first
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("[GameManager] Initialized");
    }

    public void PostInitialize()
    {
        // Additional setup after all systems are initialized
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

        Debug.Log("[GameManager] Post-Initialized");
    }

    #region Unity Callbacks

    public void LoadScene(string sceneName)
    {
        SaveManager.Instance.SaveGame();
        SceneManager.LoadScene(sceneName);
    }

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
        HandleDebugInput();
    }

    private void OnApplicationQuit()
    {
        Debug.Log("[GameManager] Application quitting → saving.");
        SaveManager.Instance.SaveGame();
    }

    #endregion

    #region Gameplay Timer

    private void UpdatePlayTimer()
    {
        playTimeSeconds += Time.deltaTime;
        timerTick += Time.deltaTime;

        if (timerTick >= TIMER_INTERVAL)
        {
            timerTick -= TIMER_INTERVAL;
            OnPlayTimeUpdated?.Invoke(playTimeSeconds);
        }
    }
    public void ResetTimer()
    {
        playTimeSeconds = 0f;
        timerTick = 0f;
        OnPlayTimeUpdated?.Invoke(0f);
    }

    #endregion

    private void HandleDebugInput()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.H))
            SaveManager.Instance?.SaveGame();

        if (Input.GetKeyDown(KeyCode.R))
            ResetGame();

        // Add more debug shortcuts
        if (Input.GetKeyDown(KeyCode.P))
            TogglePause();
#endif
    }

    private void TogglePause()
    {
        if (CurrentState == GameState.Playing)
            PauseGame();
        else if (CurrentState == GameState.Paused)
            ResumeGame();
    }

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
        SaveManager.Instance.DeleteSave();
        Debug.Log("[GameManager] Save deleted");
    }

    #endregion

    #region Load Game Routine

    private System.Collections.IEnumerator LoadGameRoutine()
    {
        ChangeState(GameState.Loading);

        GameDataSO gameDataToUse = null;

        // Try to get save data first
        var saveData = SaveManager.Instance?.GetGameData();

        if (saveData != null)
        {
            // Use the save data
            gameDataToUse = saveData;
            Debug.Log("[GameManager] Loaded from save data");
        }
        else if (defaultDataSO != null)
        {
            // Create a default GameDataSO from the defaults
            gameDataToUse = GetDefaultGameData();
            Debug.Log("[GameManager] No save found, using default data");
        }
        else
        {
            Debug.LogError("[GameManager] No save data and no default data available!");
            ChangeState(GameState.GameOver);
            yield break;
        }

        // Now pass the GameDataSO to TurretPlacementController
        if (gameDataToUse != null)
        {
            TurretPlacementController.Instance?.SetupFromGameData(gameDataToUse);
        }

        MapLoaderManager.Instance?.GenerateMapSequence();

        float timeout = 10f;
        float elapsed = 0f;
        while (PlayerManager.Instance == null && elapsed < timeout)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }

        if (PlayerManager.Instance == null)
        {
            Debug.LogError("[GameManager] PlayerManager not found after timeout!");
            ChangeState(GameState.GameOver);
            yield break;
        }

        MapProgressionManager.Instance.ResetProgression();
        MapProgressionManager.Instance.LoadNextRoom();
        ChangeState(GameState.Playing);
    }
    private GameDataSO GetDefaultGameData()
    {
        if (defaultDataSO == null)
        {
            Debug.LogError("[GameManager] defaultDataSO is null!");
            return null;
        }

        // Create a new GameDataSO instance
        GameDataSO defaultGameData = ScriptableObject.CreateInstance<GameDataSO>();

        // Reset it using your defaultDataSO
        defaultGameData.ResetToDefaults(defaultDataSO);

        return defaultGameData;
    }
    #endregion
}

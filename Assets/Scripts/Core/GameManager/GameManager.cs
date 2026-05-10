using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] public GameDataSO gameDataSO;

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

        if (timerTick >= 1f)
        {
            timerTick = 0f;
            OnPlayTimeUpdated?.Invoke(playTimeSeconds);
        }
    }

    #endregion

    private void HandleDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.H))
            SaveManager.Instance.SaveGame();
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

        // Setup systems
        TurretPlacementController.Instance?.SetupFromGameData(
            SaveManager.Instance.GetGameData()
        );
        MapLoaderManager.Instance?.GenerateMapSequence();


            yield return new WaitUntil(() => PlayerManager.Instance != null);
            MapProgressionManager.Instance.ResetProgression();
            MapProgressionManager.Instance.LoadNextRoom();
            ChangeState(GameState.Playing);
         }
    #endregion

}

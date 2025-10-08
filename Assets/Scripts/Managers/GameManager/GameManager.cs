using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public GameDataSO gameDataSO;
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Stats,
        GameOver
    }
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }
    public GameState PreviousState { get; private set; }

    public event Action<GameState, GameState> OnGameStateChanged;

    private readonly List<IPausable> pausables = new List<IPausable>();

    [Header("Debug Controls")]
    [SerializeField] private GameState debugState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // only one instance allowed
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // survive scene loads

        ChangeState(GameState.MainMenu);
    }

    private void Update()
    {
#if UNITY_EDITOR
        // Allow changing game state from inspector in play mode
        if (debugState != CurrentState)
        {
            ChangeState(debugState);
        }
#endif

        HandlePauseInput();
    }

    private void HandlePauseInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused())
            {
                ResumeGame();
                InGameMenuManager.Instance?.ReturnToGame();
            }
            else if (IsPlaying())
            {
                PauseGame();
                InGameMenuManager.Instance?.OpenOneInGameMenu(1);
            }
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

    public void SetPaused(bool paused)
    {
        CurrentState = paused ? GameState.Paused : GameState.Playing;
    }

    public void SaveGame()
    {
        SaveSystem.Save(gameDataSO);
    }

    public void LoadGame()
    {
        SaveSystem.Load(gameDataSO);
        //Loads the saved turretTypes into TurretManager
        TurretPlacementController.Instance.SetupFromGameData(gameDataSO);
    }

    public void PauseGame() => ChangeState(GameState.Paused);
    public void ResumeGame() => ChangeState(GameState.Playing);

    public bool IsPlaying() => CurrentState == GameState.Playing;
    public bool IsPaused() => CurrentState == GameState.Paused;
#if UNITY_EDITOR
    // Called when a serialized field changes in the inspector
    private void OnValidate()
    {
        // Only apply if the game is running
        if (!Application.isPlaying) return;

        if (debugState != CurrentState)
        {
            ChangeState(debugState);
        }
    }
#endif
}

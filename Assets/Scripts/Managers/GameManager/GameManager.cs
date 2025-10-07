using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
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

    public event Action<GameState> OnGameStateChanged;

    private readonly List<IPausable> pausables = new List<IPausable>();
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
    public void UnregisterPausable(IPausable pausable) => pausables.Remove(pausable);

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
        if (debugState != CurrentState)
        {
            ChangeState(debugState);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused())
            {
                ChangeState(GameState.Playing);
                InGameMenuManager.Instance?.ReturnToGame();
            }
            else if (IsPlaying())
            {
                ChangeState(GameState.Paused);
                InGameMenuManager.Instance?.OpenOneInGameMenu(1); // Show pause menu
            }
        }
    }


    public void ChangeState(GameState newState)
    {
        if (newState == CurrentState) return;

        CurrentState = newState;
        OnGameStateChanged?.Invoke(CurrentState);

        foreach (var p in pausables)
        {
            if (newState == GameState.Paused) p.OnPause();
            else if (newState == GameState.Playing) p.OnResume();
        }
    }
    public void SetPaused(bool paused)
    {
        CurrentState = paused ? GameState.Paused : GameState.Playing;
    }
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

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
        GameOver
    }
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }

    public event Action<GameState> OnGameStateChanged;

    private readonly List<IPausable> pausables = new List<IPausable>();
    public void RegisterPausable(IPausable pausable) => pausables.Add(pausable);
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
}

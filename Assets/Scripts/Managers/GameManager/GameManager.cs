using UnityEngine;
using System;

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
        Debug.Log($"Game State changed to: {CurrentState}");

        // Notify listeners (UI, enemies, player controller, etc.)
        OnGameStateChanged?.Invoke(CurrentState);

        // Optional: perform built-in actions depending on the state
        switch (CurrentState)
        {
            case GameState.MainMenu:
                break;

            case GameState.Playing:
                break;

            case GameState.Paused:
                break;

            case GameState.GameOver:
                break;
        }
    }
    public void SetPaused(bool paused)
    {
        CurrentState = paused ? GameState.Paused : GameState.Playing;
    }
    public bool IsPlaying() => CurrentState == GameState.Playing;
    public bool IsPaused() => CurrentState == GameState.Paused;
}

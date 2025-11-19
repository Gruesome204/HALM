using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public GameDataSO gameDataSO;
    public GameData gameData;
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

    [SerializeField]private readonly List<IPausable> pausables = new List<IPausable>();


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

    private void Start()
    {
        gameData = SaveSystem.Load();
        ApplyRuntimeDataToSO();
    }

    private void ApplyRuntimeDataToSO()
    {
        gameDataSO.currentPlayerLevel = gameData.currentPlayerLevel;
        gameDataSO.gameCurrency = gameData.gameCurrency;
        gameDataSO.currentClass = gameData.currentClass;

        gameDataSO.unlockedTurrets = new List<TurretType>(gameData.unlockedTurrets);
        gameDataSO.selectedTurrets = new List<TurretType>(gameData.selectedTurrets);
    }

    public void SaveGame()
    {
        gameData.gameCurrency = gameDataSO.gameCurrency;
        gameData.currentPlayerLevel = gameDataSO.currentPlayerLevel;
        gameData.currentClass = gameDataSO.currentClass;

        gameData.unlockedTurrets = gameDataSO.unlockedTurrets.ToList();
        gameData.selectedTurrets = gameDataSO.selectedTurrets.ToList();

        SaveSystem.Save(gameData);
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

}

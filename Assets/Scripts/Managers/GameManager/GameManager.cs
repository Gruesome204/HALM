using UnityEngine;
using System.Collections.Generic;
using System;

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

    [SerializeField]private readonly List<IPausable> pausables = new List<IPausable>();

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

    //Testin Purpose
    private void Update()
    {
    //#if UNITY_EDITOR
    //        // Allow changing game state from inspector in play mode
    //        if (debugState != CurrentState)
    //        {
    //            ChangeState(debugState);
    //        }
    //#endif

    }

    public void ChangeState(GameState newState)
    {
        if (newState == CurrentState) return;

        PreviousState = CurrentState;
        CurrentState = newState;
        Debug.Log($"[GameManager] State changed: {PreviousState} → {CurrentState}");
        OnGameStateChanged?.Invoke(CurrentState, PreviousState); 

        UpdatePausables(newState);
        GameManager.Instance.DebugRegisteredPausables();
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
    public void DebugRegisteredPausables()
    {
        Debug.Log($"[GameManager] {pausables.Count} IPausable(s) currently registered:");

        for (int i = 0; i < pausables.Count; i++)
        {
            var p = pausables[i];
            if (p == null)
            {
                Debug.Log($" - {i}: null reference");
            }
            else
            {
                // If the IPausable is a MonoBehaviour, show the GameObject name
                if (p is MonoBehaviour mb)
                    Debug.Log($" - {i}: {mb.GetType().Name} on GameObject '{mb.gameObject.name}'");
                else
                    Debug.Log($" - {i}: {p.GetType().Name}");
            }
        }
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

    [ContextMenu("Debug Registered Pausables")]
    private void DebugRegisteredPausablesContextMenu()
    {
        DebugRegisteredPausables();
    }
#endif
}

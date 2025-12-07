using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Data")]
    public GameDataSO gameDataSO;
    public GameData gameData;
    public enum GameState
    {
        MainMenu,
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
        LoadGameAtStartup();
    }
    private void LoadGameAtStartup()
    {
        ChangeState(GameState.Loading);

        // Load save file
        gameData = SaveSystem.Load();

        // Apply runtime data to SO
        ApplyRuntimeDataToSO();

        // Load Scene Elements (Map)
        MapLoaderManager.Instance?.LoadMap(0);


        // Apply upgrades AFTER player is found
      //  ApplyPlayerUpgrades();

        ChangeState(GameState.Playing);
    }

    private void ApplyPlayerUpgrades()  
    {
        var so = gameDataSO;
        var playerStats = PlayerManager.Instance.GetComponent<PlayerStats>();
        playerStats.currentHealth += so.additionalHealth;   
        playerStats.currentMaxHealth += so.additionalHealth;
        playerStats.currentRegen += so.additionalRegen;
        playerStats.currentArmor += so.additionalArmor;
        playerStats.currentMagicResistance += so.additionalMagicResistance;
    }

    private void ApplyRuntimeDataToSO()
    {
        var so = gameDataSO;

        so.gameCurrency = gameData.gameCurrency;
        so.currentPlayerLevel = gameData.currentPlayerLevel;
        so.currentClass = gameData.currentClass;

        so.unlockedTurrets = new List<TurretType>(gameData.unlockedTurrets);
        so.selectedTurrets = new List<TurretType>(gameData.selectedTurrets);
    }

    private void Update()
    {
        if (CurrentState != GameState.Playing)
            return;

        autosaveTimer += Time.deltaTime;

        if (autosaveTimer >= autosaveInterval)
        {
            autosaveTimer = 0f;
            SaveGame();
            Debug.Log("Auto-saved.");
        }
    }


    public void SaveGame()
    {
        if (gameData == null) gameData = new GameData();
        if (gameDataSO == null) return;

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

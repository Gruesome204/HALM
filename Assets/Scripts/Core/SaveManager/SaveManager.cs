using System;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameDataSO gameDataSO;
    [SerializeField] private GameDataDefaultsSO defaultData;

    [Header("Autosave")]
    [SerializeField] private float autosaveInterval = 60f;

    private float autosaveTimer;

    public bool IsSaveLoaded { get; private set; }

    public event Action OnSaveCompleted;
    public event Action OnLoadCompleted;

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

        InitializeGameData();
        LoadOrCreateSave();
    }

    private void Update()
    {
        HandleAutosave();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    #endregion

    #region Initialization

    private void InitializeGameData()
    {
        if (gameDataSO == null)
        {
            Debug.LogError("[SaveManager] Missing GameDataSO reference!");
            return;
        }

        gameDataSO = Instantiate(gameDataSO);
    }

    #endregion

    #region Save / Load

    public void LoadOrCreateSave()
    {
        TempSaveData saveData = SaveSystem.Load();

        if (saveData != null)
        {
            gameDataSO.ApplySave(saveData);
            Debug.Log("[SaveManager] Save loaded.");
        }
        else
        {
            CreateNewSave();
        }

        IsSaveLoaded = true;
        OnLoadCompleted?.Invoke();
    }

    public void SaveGame()
    {
        if (gameDataSO == null)
        {
            Debug.LogWarning("[SaveManager] GameDataSO is null!");
            return;
        }

        TempSaveData saveData = gameDataSO.ToSaveData();

        SaveSystem.Save(saveData);

        Debug.Log("[SaveManager] Game saved.");
        OnSaveCompleted?.Invoke();
    }

    public void CreateNewSave()
    {
        gameDataSO.ResetToDefaults(defaultData);

        SaveGame();

        Debug.Log("[SaveManager] Created new save.");
    }

    public void DeleteSave()
    {
        SaveSystem.DeleteSaveFiles();

        Debug.Log("[SaveManager] Save deleted.");
    }

    #endregion

    #region Autosave

    private void HandleAutosave()
    {
        if (GameManager.Instance == null)
            return;

        if (!GameManager.Instance.IsPlaying())
            return;

        autosaveTimer += Time.deltaTime;

        if (autosaveTimer >= autosaveInterval)
        {
            autosaveTimer = 0f;
            SaveGame();
        }
    }

    #endregion

    #region Public Accessors

    public GameDataSO GetGameData()
    {
        return gameDataSO;
    }

    #endregion
}
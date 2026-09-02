using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurretLevelManager : MonoBehaviour, IGameSystem
{
    public static TurretLevelManager Instance { get; private set; }

    [Serializable]
    public class TurretProgress
    {
        [SerializeField] public int currentLevel = 1;
        [SerializeField] public float currentXP = 0;
        [SerializeField] public float xpToNextLevel = 100f;

        public void ResetProgress()
        {
            currentLevel = 1;
            currentXP = 0;
            xpToNextLevel = 100f;
        }
    }

    private Dictionary<TurretType, TurretProgress> turretProgressDict = new Dictionary<TurretType, TurretProgress>();
    private bool isInitialized = false;
    private bool isSubscribedToEvents = false;

    [Header("Level Settings")]
    [SerializeField] private int maxLevel = 10;
    [SerializeField] private float xpGrowthMultiplier = 1.2f;
    [SerializeField] private float baseXPToNextLevel = 100f;

    // Public properties for safe access
    public int MaxLevel => maxLevel;
    public float XPGrowthMultiplier => xpGrowthMultiplier;

    // Level-up event
    public delegate void LevelUpEvent(TurretType type, int newLevel);
    public event LevelUpEvent OnLevelUp;

    // Milestone event
    public delegate void MilestoneEvent(TurretType type, int level);
    public event MilestoneEvent OnMilestoneReached;

    // XP change event
    public event Action<TurretType, float, float> OnXPChanged; // type, currentXP, xpToNext

    // ========================
    // IGameSystem Implementation
    // ========================
    public int InitializePriority => 3;

    public void Initialize()
    {
        if (isInitialized)
        {
            Debug.Log("[TurretLevelManager] Already initialized.");
            return;
        }

        // Validate dependencies
        if (TurretUpgradeChoiceManager.Instance == null)
        {
            Debug.LogWarning("[TurretLevelManager] TurretUpgradeChoiceManager not found! Upgrades may not work.");
        }

        if (TurretPlacementController.Instance == null)
        {
            Debug.LogWarning("[TurretLevelManager] TurretPlacementController not found!");
        }

        if (TurretGlobalModifierManager.Instance == null)
        {
            Debug.LogWarning("[TurretLevelManager] TurretGlobalModifierManager not found!");
        }

        // Initialize turret progress for all turret types
        InitializeTurretProgress();

        isInitialized = true;
        Debug.Log($"[TurretLevelManager] Initialized. {turretProgressDict.Count} turret types.");
    }

    public void PostInitialize()
    {
        Debug.Log("[TurretLevelManager] Post-Initializing...");

        // Subscribe to events if not already
        if (!isSubscribedToEvents)
        {
            SubscribeToEvents();
        }

        // Load saved progress if any
        LoadTurretProgress();

        // Apply current levels to all existing turrets
        ReapplyAllUpgrades();
    }

    // ========================
    // UNITY LIFECYCLE
    // ========================
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
        // Auto-initialize if not already done
        if (!isInitialized)
        {
            Debug.LogWarning("[TurretLevelManager] Auto-initializing in Start()");
            Initialize();
            PostInitialize();
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
        SaveTurretProgress();
    }

    private void OnApplicationQuit()
    {
        SaveTurretProgress();
    }

    // ========================
    // EVENT SUBSCRIPTION
    // ========================
    private void SubscribeToEvents()
    {
        // Subscribe to turret placement events
        if (TurretPlacementController.Instance != null)
        {
            TurretPlacementController.Instance.OnTurretsChanged += OnTurretsChangedHandler;
            isSubscribedToEvents = true;
            Debug.Log("[TurretLevelManager] Subscribed to turret events.");
        }
        else
        {
            Debug.LogWarning("[TurretLevelManager] Cannot subscribe to TurretPlacementController events.");
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (isSubscribedToEvents && TurretPlacementController.Instance != null)
        {
            TurretPlacementController.Instance.OnTurretsChanged -= OnTurretsChangedHandler;
            isSubscribedToEvents = false;
            Debug.Log("[TurretLevelManager] Unsubscribed from turret events.");
        }
    }

    private void OnTurretsChangedHandler()
    {
        // When turrets change, reapply upgrades to ensure all turrets have correct stats
        if (isInitialized)
        {
            ReapplyAllUpgrades();
        }
    }

    // ========================
    // INITIALIZATION
    // ========================
    private void InitializeTurretProgress()
    {
        turretProgressDict.Clear();

        foreach (TurretType type in Enum.GetValues(typeof(TurretType)))
        {
            var progress = new TurretProgress();
            progress.xpToNextLevel = baseXPToNextLevel;
            turretProgressDict[type] = progress;
        }
    }

    private void LoadTurretProgress()
    {
        // Load from PlayerPrefs or save system
        // This is a placeholder - implement your save system here
        foreach (var kvp in turretProgressDict)
        {
            string key = $"TurretLevel_{kvp.Key}";
            if (PlayerPrefs.HasKey(key))
            {
                kvp.Value.currentLevel = PlayerPrefs.GetInt(key, 1);
                kvp.Value.currentXP = PlayerPrefs.GetFloat($"TurretXP_{kvp.Key}", 0);
                kvp.Value.xpToNextLevel = PlayerPrefs.GetFloat($"TurretXPToNext_{kvp.Key}", baseXPToNextLevel);
                Debug.Log($"[TurretLevelManager] Loaded progress for {kvp.Key}: Level {kvp.Value.currentLevel}");
            }
        }
    }

    private void SaveTurretProgress()
    {
        // Save to PlayerPrefs or save system
        // This is a placeholder - implement your save system here
        foreach (var kvp in turretProgressDict)
        {
            PlayerPrefs.SetInt($"TurretLevel_{kvp.Key}", kvp.Value.currentLevel);
            PlayerPrefs.SetFloat($"TurretXP_{kvp.Key}", kvp.Value.currentXP);
            PlayerPrefs.SetFloat($"TurretXPToNext_{kvp.Key}", kvp.Value.xpToNextLevel);
        }
        PlayerPrefs.Save();
    }

    // ========================
    // XP MANAGEMENT
    // ========================
    public void AddXP(TurretType type, float amount)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[TurretLevelManager] Cannot add XP - not initialized!");
            return;
        }

        if (!turretProgressDict.TryGetValue(type, out var progress))
        {
            Debug.LogError($"[TurretLevelManager] Unknown turret type: {type}");
            return;
        }

        if (progress.currentLevel >= maxLevel)
        {
            Debug.Log($"[TurretLevelManager] {type} already at max level {maxLevel}");
            return;
        }

        // Apply XP multipliers from global modifiers if needed
        float modifiedAmount = amount; // Could add XP multipliers here

        progress.currentXP += modifiedAmount;
        OnXPChanged?.Invoke(type, progress.currentXP, progress.xpToNextLevel);

        // Check for level ups
        while (progress.currentXP >= progress.xpToNextLevel && progress.currentLevel < maxLevel)
        {
            // Level up!
            progress.currentXP -= progress.xpToNextLevel;
            progress.currentLevel++;
            progress.xpToNextLevel = CalculateXPToNextLevel(progress.currentLevel);

            Debug.Log($"[TurretLevelManager] {type} leveled up to {progress.currentLevel}!");

            // Trigger level up event
            OnLevelUp?.Invoke(type, progress.currentLevel);

            // Apply upgrades for this level
            ApplyUpgradesForLevel(type, progress.currentLevel);
        }

        // Save progress
        SaveTurretProgress();

        Debug.Log($"[TurretLevelManager] {type} XP: {progress.currentXP:F1}/{progress.xpToNextLevel:F1} | Level {progress.currentLevel}");
    }

    private float CalculateXPToNextLevel(int currentLevel)
    {
        // XP needed = baseXP * (growthMultiplier ^ (level - 1))
        return baseXPToNextLevel * Mathf.Pow(xpGrowthMultiplier, currentLevel - 1);
    }

    // ========================
    // UPGRADE APPLICATION
    // ========================
    private void ApplyUpgradesForLevel(TurretType type, int level)
    {
        if (TurretUpgradeChoiceManager.Instance == null)
        {
            Debug.LogWarning("[TurretLevelManager] TurretUpgradeChoiceManager missing, cannot apply upgrades.");
            return;
        }

        var options = TurretUpgradeChoiceManager.Instance.GetAvailableOptionsForLevel(type, level);

        if (options != null && options.Any())
        {
            Debug.Log($"[TurretLevelManager] Found {options.Count()} upgrade options for {type} at level {level}:");

            foreach (var option in options)
            {
                if (option?.modifier != null)
                {
                    Debug.Log($"  - {option.displayName}: Damage x{option.modifier.damageMultiplier} | " +
                              $"FireRate x{option.modifier.shotsPerSecondBonus} | " +
                              $"Range +{option.modifier.rangeBonus}");
                }
            }

            // Trigger milestone event
            OnMilestoneReached?.Invoke(type, level);
        }

        // Force all turrets of this type to reapply upgrades
        ForceReapplyUpgrades(type);
    }

    public void ForceReapplyUpgrades(TurretType type)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[TurretLevelManager] Cannot reapply upgrades - not initialized!");
            return;
        }

        var turretController = TurretPlacementController.Instance;
        if (turretController == null)
        {
            Debug.LogWarning("[TurretLevelManager] TurretPlacementController not found!");
            return;
        }

        var activeTurrets = turretController.GetActiveTurrets();
        if (activeTurrets == null || activeTurrets.Count == 0)
        {
            Debug.Log($"[TurretLevelManager] No active turrets to reapply upgrades for {type}");
            return;
        }

        int updatedCount = 0;
        foreach (var turret in activeTurrets)
        {
            if (turret == null) continue;

            var turretBehaviour = turret.GetComponentInChildren<TurretBehaviour>();
            if (turretBehaviour == null || turretBehaviour.TurretBlueprint == null)
                continue;

            if (turretBehaviour.TurretBlueprint.turretType != type)
                continue;

            int level = GetLevel(type);
            var stats = turret.GetComponentInChildren<TurretStats>();
            if (stats == null)
                continue;

            var upgrade = TurretUpgradeChoiceManager.Instance != null
                ? TurretUpgradeChoiceManager.Instance.GetCombinedModifier(type)
                : null;

            stats.RecalculateStats(
                turretBehaviour,
                turretBehaviour.TurretBlueprint,
                level,
                upgrade,
                TurretGlobalModifierManager.Instance
            );

            // Also update health if needed
            var health = turret.GetComponentInChildren<TurretHealth>();
            if (health != null)
            {
                health.RecalculateStatsAfterModifiers();
            }

            updatedCount++;
        }

        if (updatedCount > 0)
        {
            Debug.Log($"[TurretLevelManager] Reapplied upgrades to {updatedCount} turrets of type {type}");
        }
    }

    public void ReapplyAllUpgrades()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[TurretLevelManager] Cannot reapply all upgrades - not initialized!");
            return;
        }

        foreach (TurretType type in Enum.GetValues(typeof(TurretType)))
        {
            ForceReapplyUpgrades(type);
        }

        Debug.Log("[TurretLevelManager] Reapplied upgrades for all turret types.");
    }

    // ========================
    // PUBLIC GETTERS
    // ========================
    public int GetLevel(TurretType type)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[TurretLevelManager] Not initialized, returning default level 1.");
            return 1;
        }

        if (turretProgressDict.TryGetValue(type, out var progress))
        {
            return progress.currentLevel;
        }

        Debug.LogWarning($"[TurretLevelManager] Unknown turret type: {type}, returning default level 1.");
        return 1;
    }

    public float GetXP(TurretType type)
    {
        if (turretProgressDict.TryGetValue(type, out var progress))
        {
            return progress.currentXP;
        }
        return 0f;
    }

    public float GetXPToNextLevel(TurretType type)
    {
        if (turretProgressDict.TryGetValue(type, out var progress))
        {
            return progress.xpToNextLevel;
        }
        return baseXPToNextLevel;
    }

    public float GetProgressPercentage(TurretType type)
    {
        if (turretProgressDict.TryGetValue(type, out var progress))
        {
            if (progress.currentLevel >= maxLevel) return 1f;
            return Mathf.Clamp01(progress.currentXP / progress.xpToNextLevel);
        }
        return 0f;
    }

    public bool IsAtMaxLevel(TurretType type)
    {
        return GetLevel(type) >= maxLevel;
    }

    public int GetRemainingXPToNextLevel(TurretType type)
    {
        if (turretProgressDict.TryGetValue(type, out var progress))
        {
            if (progress.currentLevel >= maxLevel) return 0;
            return Mathf.CeilToInt(progress.xpToNextLevel - progress.currentXP);
        }
        return 0;
    }

    public TurretProgress GetProgress(TurretType type)
    {
        if (turretProgressDict.TryGetValue(type, out var progress))
        {
            return progress;
        }
        return null;
    }

    public Dictionary<TurretType, TurretProgress> GetAllProgress()
    {
        return new Dictionary<TurretType, TurretProgress>(turretProgressDict);
    }

    // ========================
    // CONVENIENCE METHODS
    // ========================
    public void OnTurretLevelChanged(TurretType type)
    {
        if (!isInitialized) return;
        ForceReapplyUpgrades(type);
    }

    public void ResetTurretProgress(TurretType type)
    {
        if (!isInitialized) return;

        if (turretProgressDict.TryGetValue(type, out var progress))
        {
            progress.ResetProgress();
            SaveTurretProgress();
            ForceReapplyUpgrades(type);
            Debug.Log($"[TurretLevelManager] Reset progress for {type}");
        }
    }

    public void ResetAllProgress()
    {
        if (!isInitialized) return;

        foreach (var kvp in turretProgressDict)
        {
            kvp.Value.ResetProgress();
        }
        SaveTurretProgress();
        ReapplyAllUpgrades();
        Debug.Log("[TurretLevelManager] Reset all turret progress.");
    }

    // ========================
    // DEBUG HELPERS
    // ========================
    public void DebugAllTurretLevels()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[TurretLevelManager] Not initialized!");
            return;
        }

        Debug.Log("[TurretLevelManager] Current Turret Levels:");
        foreach (var kvp in turretProgressDict)
        {
            Debug.Log($"  {kvp.Key}: Level {kvp.Value.currentLevel}, " +
                      $"XP {kvp.Value.currentXP:F1}/{kvp.Value.xpToNextLevel:F1} " +
                      $"({GetProgressPercentage(kvp.Key) * 100:F1}%)");
        }
    }

    public void AddDebugXP(TurretType type, float amount = 100f)
    {
        if (!isInitialized) return;
        Debug.Log($"[TurretLevelManager] Adding {amount} debug XP to {type}");
        AddXP(type, amount);
    }

    // ========================
    // GAME STATE HANDLING
    // ========================
    public void OnGamePaused(bool paused)
    {
        // Handle game pause if needed
        if (paused)
        {
            // Optionally save progress when paused
            SaveTurretProgress();
        }
    }

    public void OnGameReset()
    {
        ResetAllProgress();
        Debug.Log("[TurretLevelManager] Game reset complete.");
    }

    // ========================
    // VALIDATION
    // ========================
    public bool IsInitialized => isInitialized;

    private void OnValidate()
    {
        // Validate settings in editor
        if (maxLevel < 1) maxLevel = 1;
        if (baseXPToNextLevel < 1) baseXPToNextLevel = 1;
        if (xpGrowthMultiplier < 1f) xpGrowthMultiplier = 1f;
    }
}
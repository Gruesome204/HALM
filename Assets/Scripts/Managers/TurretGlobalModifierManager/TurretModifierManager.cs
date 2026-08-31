using System.Collections.Generic;
using UnityEngine;

public class TurretGlobalModifierManager : MonoBehaviour, IGameSystem
{
    public static TurretGlobalModifierManager Instance { get; private set; }

    public event System.Action OnModifiersChanged;

    // List of all currently applied modifiers
    private readonly List<BuildMasterModifier.Modifier> appliedModifiers = new List<BuildMasterModifier.Modifier>();
    private bool isInitialized = false;
    private bool isSubscribedToEvents = false;

    // ========================
    // IGameSystem Implementation
    // ========================
    public int InitializePriority => 3;

    public void Initialize()
    {
        if (isInitialized)
        {
            Debug.Log("[TurretGlobalModifierManager] Already initialized.");
            return;
        }

        Debug.Log("[TurretGlobalModifierManager] Initializing...");

        // Reset all global modifiers to default
        ResetGlobalModifiers();

        // Validate dependencies
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[TurretGlobalModifierManager] GameManager not found!");
        }
        else if (GameManager.Instance.gameDataSO == null)
        {
            Debug.LogWarning("[TurretGlobalModifierManager] GameDataSO not found!");
        }

        // Clear any existing modifiers
        appliedModifiers.Clear();

        isInitialized = true;
        Debug.Log("[TurretGlobalModifierManager] Initialized successfully.");
    }

    public void PostInitialize()
    {
        Debug.Log("[TurretGlobalModifierManager] Post-Initializing...");

        // Subscribe to events if not already
        if (!isSubscribedToEvents)
        {
            SubscribeToEvents();
        }

        // Load modifiers from GameData
        LoadModifiersFromGameData();

        // Apply modifiers to existing turrets
        ApplyModifiersToAllExistingTurrets();

        // Update placement controller settings
        UpdatePlacementController();
    }

    // ========================
    // UNITY LIFECYCLE
    // ========================
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Auto-initialize if not already done
        if (!isInitialized)
        {
            Debug.LogWarning("[TurretGlobalModifierManager] Auto-initializing in Start()");
            Initialize();
            PostInitialize();
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void Update()
    {
        // Only process input if initialized
        if (!isInitialized) return;

        // Debug input - consider wrapping in #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("K - Adding test damage modifier");
            AddDamageModifier();
        }
    }

    // ========================
    // EVENT SUBSCRIPTION
    // ========================
    private void SubscribeToEvents()
    {
        if (GameManager.Instance?.gameDataSO != null)
        {
            GameManager.Instance.gameDataSO.OnBuildMasterModifiersChanged += UpdateModifiersFromSO;
            isSubscribedToEvents = true;
            Debug.Log("[TurretGlobalModifierManager] Subscribed to modifier change events.");
        }
        else
        {
            Debug.LogWarning("[TurretGlobalModifierManager] Cannot subscribe to events - GameDataSO missing.");
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (isSubscribedToEvents && GameManager.Instance?.gameDataSO != null)
        {
            GameManager.Instance.gameDataSO.OnBuildMasterModifiersChanged -= UpdateModifiersFromSO;
            isSubscribedToEvents = false;
            Debug.Log("[TurretGlobalModifierManager] Unsubscribed from modifier change events.");
        }
    }

    // ========================
    // MODIFIER LOADING
    // ========================
    private void LoadModifiersFromGameData()
    {
        if (GameManager.Instance?.gameDataSO == null)
        {
            Debug.LogWarning("[TurretGlobalModifierManager] Cannot load modifiers - GameDataSO missing.");
            return;
        }

        UpdateModifiersFromSO();
    }

    private void UpdateModifiersFromSO()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[TurretGlobalModifierManager] Not initialized, cannot update modifiers.");
            return;
        }

        appliedModifiers.Clear();

        var gameData = GameManager.Instance?.gameDataSO;
        if (gameData == null)
        {
            Debug.LogWarning("[TurretGlobalModifierManager] GameDataSO is null, skipping modifier update.");
            return;
        }

        var selectedModifiers = gameData.GetSelectedModifiers();
        if (selectedModifiers == null)
        {
            Debug.LogWarning("[TurretGlobalModifierManager] No selected modifiers found.");
            return;
        }

        foreach (var modifierSO in selectedModifiers)
        {
            if (modifierSO?.options == null)
            {
                Debug.LogWarning("[TurretGlobalModifierManager] Skipping null modifier.");
                continue;
            }

            var runtimeModifier = new BuildMasterModifier.Modifier
            {
                name = modifierSO.options.name,
                description = modifierSO.options.description,
                icon = modifierSO.options.icon,
                costs = modifierSO.options.costs,
                additionalStats = modifierSO.options.additionalStats
            };

            appliedModifiers.Add(runtimeModifier);

            Debug.Log($"[TurretGlobalModifierManager] Loaded modifier: {runtimeModifier.name}, " +
                      $"Damage: +{runtimeModifier.additionalStats.turretDamageMultiplier * 100}%, " +
                      $"Health: +{runtimeModifier.additionalStats.turretHealthMultiplier * 100}%");
        }

        Debug.Log($"[TurretGlobalModifierManager] Loaded {appliedModifiers.Count} modifiers from GameData.");
        RecalculateGlobalModifiers();
    }

    // ========================
    // TEST MODIFIERS (Debug)
    // ========================
    private void AddDamageModifier()
    {
        var mod = new BuildMasterModifier.Modifier
        {
            name = "Damage +100%",
            description = "Doubles turret damage",
            additionalStats = new BuildMasterModifier.Stats
            {
                turretDamageMultiplier = 5.0f
            }
        };
        AddModifier(mod);
    }

    // ========================
    // GLOBAL MODIFIERS (Percentage Style)
    // ========================
    [Header("Global Turret Stats (Percentage)")]
    [SerializeField] private float globalTurretPlacementCooldownMultiplier = 0f;
    [SerializeField] private float globalHealthMultiplier = 0f;
    [SerializeField] private float globalDamageMultiplier = 0f;
    [SerializeField] private float globalShotsPerSecondBonus = 0f;
    [SerializeField] private int globalProjectilesPerSalve = 0;
    [SerializeField] private float globalProjectileSpeed = 0f;
    [SerializeField] private int globalMaxTurretCapacityBonus = 0;
    [SerializeField] private float globalPlacementRadiusMultiplier = 0f;

    // Public properties for safe access
    public float GlobalTurretPlacementCooldownMultiplier => globalTurretPlacementCooldownMultiplier;
    public float GlobalHealthMultiplier => globalHealthMultiplier;
    public float GlobalDamageMultiplier => globalDamageMultiplier;
    public float GlobalShotsPerSecondBonus => globalShotsPerSecondBonus;
    public int GlobalProjectilesPerSalve => globalProjectilesPerSalve;
    public float GlobalProjectileSpeed => globalProjectileSpeed;
    public int GlobalMaxTurretCapacityBonus => globalMaxTurretCapacityBonus;
    public float GlobalPlacementRadiusMultiplier => globalPlacementRadiusMultiplier;

    // ========================
    // APPLY / REMOVE MODIFIERS
    // ========================
    public void AddModifier(BuildMasterModifier.Modifier modifier)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[TurretGlobalModifierManager] Cannot add modifier - not initialized!");
            return;
        }

        if (modifier == null)
        {
            Debug.LogWarning("[TurretGlobalModifierManager] Cannot add null modifier.");
            return;
        }

        appliedModifiers.Add(modifier);
        Debug.Log($"[TurretGlobalModifierManager] Added modifier: {modifier.name}");
        RecalculateGlobalModifiers();
    }

    public void RemoveModifier(BuildMasterModifier.Modifier modifier)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[TurretGlobalModifierManager] Cannot remove modifier - not initialized!");
            return;
        }

        if (modifier == null) return;

        if (appliedModifiers.Remove(modifier))
        {
            Debug.Log($"[TurretGlobalModifierManager] Removed modifier: {modifier.name}");
            RecalculateGlobalModifiers();
        }
        else
        {
            Debug.LogWarning($"[TurretGlobalModifierManager] Modifier not found: {modifier.name}");
        }
    }

    public void RemoveModifierByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        var modifier = appliedModifiers.Find(m => m.name == name);
        if (modifier != null)
        {
            RemoveModifier(modifier);
        }
        else
        {
            Debug.LogWarning($"[TurretGlobalModifierManager] No modifier found with name: {name}");
        }
    }

    public void ClearAllModifiers()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[TurretGlobalModifierManager] Cannot clear modifiers - not initialized!");
            return;
        }

        appliedModifiers.Clear();
        Debug.Log("[TurretGlobalModifierManager] Cleared all modifiers.");
        RecalculateGlobalModifiers();
    }

    // ========================
    // RECALCULATE GLOBALS
    // ========================
    private void ResetGlobalModifiers()
    {
        globalTurretPlacementCooldownMultiplier = 0f;
        globalHealthMultiplier = 0f;
        globalDamageMultiplier = 0f;
        globalShotsPerSecondBonus = 0f;
        globalProjectilesPerSalve = 0;
        globalProjectileSpeed = 0f;
        globalMaxTurretCapacityBonus = 0;
        globalPlacementRadiusMultiplier = 0f;
    }

    private void RecalculateGlobalModifiers()
    {
        // Reset all globals
        ResetGlobalModifiers();

        // Add all modifiers
        foreach (var mod in appliedModifiers)
        {
            globalTurretPlacementCooldownMultiplier += mod.additionalStats.turretPlacementCooldownMultiplier;
            globalHealthMultiplier += mod.additionalStats.turretHealthMultiplier;
            globalDamageMultiplier += mod.additionalStats.turretDamageMultiplier;
            globalShotsPerSecondBonus += mod.additionalStats.shotsPerSecondBonus;
            globalProjectileSpeed += mod.additionalStats.turretProjectileSpeed;
            globalProjectilesPerSalve += mod.additionalStats.turretProjectilesPerSalve;
            globalMaxTurretCapacityBonus += mod.additionalStats.turretMaxCapacityBonus;
            globalPlacementRadiusMultiplier += mod.additionalStats.turretPlacementRadiusMultiplier;
        }

        // Apply to existing turrets
        ApplyModifiersToAllExistingTurrets();

        // Update placement controller
        UpdatePlacementController();

        // Update UI or other systems
        OnModifiersChanged?.Invoke();

        Debug.Log($"[TurretGlobalModifierManager] Recalculated modifiers. " +
                  $"Damage: +{globalDamageMultiplier * 100}%, " +
                  $"Health: +{globalHealthMultiplier * 100}%, " +
                  $"Capacity Bonus: +{globalMaxTurretCapacityBonus}");
    }

    // ========================
    // UPDATE DEPENDENT SYSTEMS
    // ========================
    private void UpdatePlacementController()
    {
        var tp = TurretPlacementController.Instance;
        if (tp == null) return;

        if (!tp.IsInitialized)
        {
            Debug.LogWarning("[TurretGlobalModifierManager] TurretPlacementController not initialized.");
            return;
        }

        int activeCount = tp.GetActiveTurrets().Count;
        tp.maxTurretCapacity = Mathf.Max(activeCount, tp.defaultMaxTurretCapacity + globalMaxTurretCapacityBonus);
        tp.placementRadius = tp.defaultPlacementRadius * (1f + globalPlacementRadiusMultiplier);

        Debug.Log($"[TurretGlobalModifierManager] Updated placement controller: " +
                  $"Capacity: {tp.maxTurretCapacity}, " +
                  $"Radius: {tp.placementRadius}");
    }

    // ========================
    // APPLY TO EXISTING TURRETS
    // ========================
    public void ApplyModifiersToAllExistingTurrets()
    {
        var tc = TurretPlacementController.Instance;
        if (tc == null)
        {
            Debug.LogWarning("[TurretGlobalModifierManager] TurretPlacementController not found.");
            return;
        }

        var activeTurrets = tc.GetActiveTurrets();
        if (activeTurrets == null || activeTurrets.Count == 0)
        {
            Debug.Log("[TurretGlobalModifierManager] No active turrets to update.");
            return;
        }

        int updatedCount = 0;
        foreach (GameObject turret in activeTurrets)
        {
            if (turret == null) continue;

            // Recalculate health first
            var health = turret.GetComponentInChildren<TurretHealth>();
            if (health != null)
            {
                health.RecalculateStatsAfterModifiers();
            }

            // Recalculate turret stats with level
            var behaviour = turret.GetComponentInChildren<TurretBehaviour>();
            var stats = turret.GetComponentInChildren<TurretStats>();

            if (behaviour == null || stats == null || behaviour.TurretBlueprint == null)
            {
                Debug.LogWarning($"[TurretGlobalModifierManager] Turret missing required components: {turret.name}");
                continue;
            }

            int level = 1;
            if (TurretLevelManager.Instance != null)
            {
                level = TurretLevelManager.Instance.GetLevel(behaviour.TurretBlueprint.turretType);
            }

            var upgrade = TurretUpgradeChoiceManager.Instance != null
                ? TurretUpgradeChoiceManager.Instance.GetCombinedModifier(behaviour.TurretBlueprint.turretType)
                : null;

            stats.RecalculateStats(
                behaviour,
                behaviour.TurretBlueprint,
                level,
                upgrade,
                this // Pass self as global modifier manager
            );

            updatedCount++;
        }

        if (updatedCount > 0)
        {
            Debug.Log($"[TurretGlobalModifierManager] Updated {updatedCount} turrets with new modifiers.");
        }
    }

    // ========================
    // PUBLIC METHODS
    // ========================
    public IReadOnlyList<BuildMasterModifier.Modifier> GetAppliedModifiers()
    {
        return appliedModifiers.AsReadOnly();
    }

    public bool IsModifierApplied(string modifierName)
    {
        if (string.IsNullOrEmpty(modifierName)) return false;
        return appliedModifiers.Exists(m => m.name == modifierName);
    }

    public bool HasAnyModifiers()
    {
        return appliedModifiers.Count > 0;
    }

    public int GetModifierCount()
    {
        return appliedModifiers.Count;
    }

    public bool IsInitialized => isInitialized;

    // ========================
    // GAME STATE HANDLING
    // ========================
    public void OnGamePaused(bool paused)
    {
        // Handle game pause if needed
        if (paused)
        {
            // Optionally disable updates or input
        }
    }

    public void OnGameReset()
    {
        ClearAllModifiers();
        ResetGlobalModifiers();
        UpdatePlacementController();
        Debug.Log("[TurretGlobalModifierManager] Reset complete.");
    }

    // ========================
    // DEBUG HELPERS
    // ========================
    public void LogCurrentModifiers()
    {
        Debug.Log($"[TurretGlobalModifierManager] Current modifiers ({appliedModifiers.Count}):");
        foreach (var mod in appliedModifiers)
        {
            Debug.Log($"  - {mod.name}: {mod.description}");
        }
        Debug.Log($"Globals - Damage: +{globalDamageMultiplier * 100}%, " +
                  $"Health: +{globalHealthMultiplier * 100}%, " +
                  $"Capacity Bonus: +{globalMaxTurretCapacityBonus}");
    }
}
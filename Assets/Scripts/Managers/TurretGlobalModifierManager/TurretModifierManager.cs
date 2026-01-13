using System.Collections.Generic;
using UnityEngine;

public class TurretGlobalModifierManager : MonoBehaviour
{
    public static TurretGlobalModifierManager Instance { get; private set; }

    public event System.Action OnModifiersChanged;

    // List of all currently applied modifiers
    private readonly List<BuildMasterModifier.Modifier> appliedModifiers = new List<BuildMasterModifier.Modifier>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        GameManager.Instance.gameDataSO.OnBuildMasterModifiersChanged += UpdateModifiersFromSO;
        UpdateModifiersFromSO(); // initial sync
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.gameDataSO.OnBuildMasterModifiersChanged -= UpdateModifiersFromSO;
    }

    private void UpdateModifiersFromSO()
    {
        appliedModifiers.Clear();

        foreach (var modifierSO in GameManager.Instance.gameDataSO.GetSelectedModifiers())
        {
            if (modifierSO?.options != null)
            {
                var runtimeModifier = new BuildMasterModifier.Modifier
                {
                    name = modifierSO.options.name,
                    description = modifierSO.options.description,
                    icon = modifierSO.options.icon,
                    costs = modifierSO.options.costs,
                    additionalStats = modifierSO.options.additionalStats
                };

                appliedModifiers.Add(runtimeModifier);

                Debug.Log($"Read modifier from SO: {runtimeModifier.name}, Damage %+{runtimeModifier.additionalStats.turretDamageMultiplier * 100}%");
            }
        }

        RecalculateGlobalModifiers();
    }

    // ---------------- GLOBAL MODIFIERS (percentage style) -------------------
    [Header("Global Turret Stats (Percentage)")]
    public float globalTurretPlacementCooldownMultiplier = 0f; // 0 = no change, 0.2 = +20%
    public float globalHealthMultiplier = 0f;
    public float globalDamageMultiplier = 0f;

    public float globalShotsPerSecondBonus = 0f;
    public int globalProjectilesPerSalve = 0; // still additive
    public float globalProjectileSpeed = 0f;
    public int globalMaxTurretCapacityBonus = 0;  // still additive
    public float globalPlacementRadiusMultiplier = 0f;

    // ---------------- APPLY / REMOVE MODIFIERS -------------------
    public void AddModifier(BuildMasterModifier.Modifier modifier)
    {
        appliedModifiers.Add(modifier);
        RecalculateGlobalModifiers();
    }

    public void RemoveModifier(BuildMasterModifier.Modifier modifier)
    {
        if (appliedModifiers.Remove(modifier))
        {
            RecalculateGlobalModifiers();
        }
    }

    // ---------------- RECALCULATE GLOBALS -------------------
    private void RecalculateGlobalModifiers()
    {
        // Reset all globals
        globalTurretPlacementCooldownMultiplier = 0f;
        globalHealthMultiplier = 0f;
        globalDamageMultiplier = 0f;
        globalShotsPerSecondBonus = 0f;
        globalProjectilesPerSalve = 0;
        globalProjectileSpeed = 0f;
        globalMaxTurretCapacityBonus = 0;
        globalPlacementRadiusMultiplier = 0f;

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

        ApplyModifiersToAllExistingTurrets();

        // Update placement controller
        var tp = TurretPlacementController.Instance;
        if (tp != null)
        {
            int activeCount = tp.GetActiveTurrets().Count;
            tp.maxTurretCapacity = Mathf.Max(activeCount, tp.defaultMaxTurretCapacity + globalMaxTurretCapacityBonus);
            tp.placementRadius = tp.defaultPlacementRadius * (1f + globalPlacementRadiusMultiplier); // percentage-style
        }

        Debug.Log("Recalculating global modifiers (percentage style)...");
        foreach (var mod in appliedModifiers)
        {
            Debug.Log($"Modifier: {mod.name}, Damage %+{mod.additionalStats.turretDamageMultiplier * 100}%, BonusShots %+{mod.additionalStats.shotsPerSecondBonus }");
        }

        OnModifiersChanged?.Invoke();
    }

    // ---------------- APPLY TO ALL EXISTING TURRETS -------------------
    public void ApplyModifiersToAllExistingTurrets()
    {
        var tc = TurretPlacementController.Instance;
        if (tc == null) return;

        foreach (GameObject turret in tc.GetActiveTurrets())
        {
            if (turret == null) continue;

            // Recalculate health first
            turret.GetComponentInChildren<TurretHealth>()?.RecalculateStatsAfterModifiers();

            // Recalculate turret stats with level
            var turretBehaviour = turret.GetComponentInChildren<TurretBehaviour>();
            if (turretBehaviour != null && turretBehaviour.turretBlueprint != null)
            {
                int level = TurretLevelManager.Instance.GetLevel(turretBehaviour.turretBlueprint.turretType);
                turretBehaviour.RecalculateStats(level);
            }
        }
    }

    // ---------------- GET APPLIED MODIFIERS -------------------
    public IReadOnlyList<BuildMasterModifier.Modifier> GetAppliedModifiers() => appliedModifiers;
}

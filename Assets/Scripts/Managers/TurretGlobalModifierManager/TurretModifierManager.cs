using System.Collections.Generic;
using System.Linq;
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
        GameManager.Instance.gameDataSO.OnBuildMasterModifiersChanged -= UpdateModifiersFromSO;
    }


    private void UpdateModifiersFromSO()
    {
        appliedModifiers.Clear(); // remove all at once

        // Apply modifiers from the SO
        foreach (var modifierSO in GameManager.Instance.gameDataSO.GetSelectedModifiers())
        {
            if (modifierSO?.options != null)
                appliedModifiers.Add(modifierSO.options);
        }

        RecalculateGlobalModifiers();
    }

    // ---------------- GLOBAL MODIFIERS -------------------
    // 1 == 100%, 1.1 == 110%
    [Header("Global Turret Stats")]
    public float globalTurretPlacementCooldownMultiplier = 1f;
    public float globalHealthMultiplier = 1f;
    public float globalDamageMultiplier = 1f;
    public float globalFireRateMultiplier = 1f;
    public int globalProjectilesPerSalve = 0; // additive
    public float globalProjectileSpeed = 1f;
    public int globalMaxTurretCapacityBonus = 0;  // additive
    public float globalPlacementRadiusMultiplier = 1f;

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
        // Reset all globals to default
        globalTurretPlacementCooldownMultiplier = 1f;
        globalHealthMultiplier = 1f;
        globalDamageMultiplier = 1f;
        globalFireRateMultiplier = 1f;
        globalProjectilesPerSalve = 0;
        globalProjectileSpeed = 1f;
        globalMaxTurretCapacityBonus = 0;
        globalPlacementRadiusMultiplier = 1f;

        // Reapply all active modifiers
        foreach (var mod in appliedModifiers)
        {
            globalTurretPlacementCooldownMultiplier *= mod.additionalStats.turretPlacementCooldownMultiplier;
            globalHealthMultiplier *= mod.additionalStats.turretHealthMultiplier;
            globalDamageMultiplier *= mod.additionalStats.turretDamageMultiplier;
            globalFireRateMultiplier *= mod.additionalStats.turretFireRateMultiplier;
            globalProjectileSpeed *= mod.additionalStats.turretProjectileSpeed;

            globalProjectilesPerSalve += mod.additionalStats.turretProjectilesPerSalve;
            globalMaxTurretCapacityBonus += mod.additionalStats.turretMaxCapacityBonus;
            globalPlacementRadiusMultiplier *= mod.additionalStats.turretPlacementRadiusMultiplier;
        }

        // Clamp AFTER all modifiers are applied
        globalTurretPlacementCooldownMultiplier = Mathf.Max(0.01f, globalTurretPlacementCooldownMultiplier);
        globalHealthMultiplier = Mathf.Max(0.1f, globalHealthMultiplier);
        globalDamageMultiplier = Mathf.Max(0.1f, globalDamageMultiplier);
        globalFireRateMultiplier = Mathf.Max(0.1f, globalFireRateMultiplier);
        globalProjectileSpeed = Mathf.Max(0.01f, globalProjectileSpeed);
        globalPlacementRadiusMultiplier = Mathf.Max(0.1f, globalPlacementRadiusMultiplier);

        // Apply to all existing turrets
        ApplyModifiersToAllExistingTurrets();

        // Update placement controller
        var tp = TurretPlacementController.Instance;
        if (tp != null)
        {
            int activeCount = tp.GetActiveTurrets().Count;
            tp.maxTurretCapacity = Mathf.Max(activeCount, tp.defaultMaxTurretCapacity + globalMaxTurretCapacityBonus);
            tp.placementRadius = tp.defaultPlacementRadius + tp.defaultPlacementRadius * (globalPlacementRadiusMultiplier - 1f);
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

            turret.GetComponentInChildren<TurretHealth>()?.RecalculateStatsAfterModifiers();
            turret.GetComponentInChildren<TurretBehaviour>()?.RecalculateFinalStats();
        }
    }

    // ---------------- GET APPLIED MODIFIERS -------------------
    public IReadOnlyList<BuildMasterModifier.Modifier> GetAppliedModifiers() => appliedModifiers;
}

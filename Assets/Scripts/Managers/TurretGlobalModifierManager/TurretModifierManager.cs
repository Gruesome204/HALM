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

       GameManager.Instance.gameDataSO.OnBuildMasterModifiersChanged += UpdateModifiersFromSO;
       UpdateModifiersFromSO(); // initial sync
    }

    private void OnDestroy()
    {
        GameManager.Instance.gameDataSO.OnBuildMasterModifiersChanged -= UpdateModifiersFromSO;
    }


    private void UpdateModifiersFromSO()
    {

        // Remove all current modifiers
        foreach (var mod in appliedModifiers.ToList())
            RemoveModifier(mod);

        // Apply modifiers from the SO
        foreach (var modifierSO in GameManager.Instance.gameDataSO.GetSelectedModifiers())
        {
            if (modifierSO?.options != null)
                AddModifier(modifierSO.options);
        }
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
            globalTurretPlacementCooldownMultiplier *= Mathf.Max(0.01f, mod.additionalStats.turretPlacementCooldownMultiplier);
            globalHealthMultiplier *= Mathf.Max(0.01f, mod.additionalStats.turretHealthMultiplier);
            globalDamageMultiplier *= Mathf.Max(0.01f, mod.additionalStats.turretDamageMultiplier);
            globalFireRateMultiplier *= Mathf.Max(0.01f, mod.additionalStats.turretFireRateMultiplier);
            globalProjectilesPerSalve += mod.additionalStats.turretProjectilesPerSalve;
            globalProjectileSpeed *= Mathf.Max(0.01f, mod.additionalStats.turretProjectileSpeed);
            globalMaxTurretCapacityBonus += mod.additionalStats.turretMaxCapacityBonus;
            globalPlacementRadiusMultiplier *= Mathf.Max(0.01f, mod.additionalStats.turretPlacementRadiusMultiplier);
        }

        // Apply to all existing turrets
        ApplyModifiersToAllExistingTurrets();

        // Update placement controller
        var tp = TurretPlacementController.Instance;
        if (tp != null)
        {
            tp.maxTurretCapacity = tp.defaultMaxTurretCapacity + globalMaxTurretCapacityBonus;
            tp.placementRadius = tp.defaultPlacementRadius * globalPlacementRadiusMultiplier;
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
            turret.GetComponentInChildren<TurretBehaviour>()?.RecalculateStats();
        }
    }

    // ---------------- GET APPLIED MODIFIERS -------------------
    public IReadOnlyList<BuildMasterModifier.Modifier> GetAppliedModifiers() => appliedModifiers;
}

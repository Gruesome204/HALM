using System.Collections.Generic;
using UnityEngine;

public class TurretGlobalModifierManager : MonoBehaviour
{
    public static TurretGlobalModifierManager Instance { get; private set; }

    public event System.Action OnModifiersChanged;

    private readonly List<BuildMasterModifier.Modifier> appliedModifiers = new List<BuildMasterModifier.Modifier>();

    [Header("Global Turret Stats")]
    public float globalTurretPlacementCooldownMultiplier = 1f;
    public float globalHealthMultiplier = 1f;
    public float globalDamageMultiplier = 1f;
    public float globalFireRateMultiplier = 1f;
    public int globalProjectilesPerSalve = 0;
    public float globalProjectileSpeed = 1f;
    public int globalMaxTurretCapacityBonus = 0;
    public float globalPlacementRadiusMultiplier = 1f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        //Add subscrption to change event for adding and removing modifier

    }

    private void OnDestroy()
    {

        //Add subscrption to change event for adding and removing  modifier
    }

    // Sync applied modifiers with BuildmasterModifyManager
    private void SyncModifiers()
    {
        appliedModifiers.Clear();

        foreach (var bmModifier in BuildmasterModifyManager.Instance.GetAppliedBuildmasterModifiers())
        {
            appliedModifiers.Add(bmModifier.options);
        }

        RecalculateGlobalModifiers();
    }

    private void RecalculateGlobalModifiers()
    {
        // Reset all globals
        globalTurretPlacementCooldownMultiplier = 1f;
        globalHealthMultiplier = 1f;
        globalDamageMultiplier = 1f;
        globalFireRateMultiplier = 1f;
        globalProjectilesPerSalve = 0;
        globalProjectileSpeed = 1f;
        globalMaxTurretCapacityBonus = 0;
        globalPlacementRadiusMultiplier = 1f;

        // Apply all modifiers
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

        ApplyModifiersToAllExistingTurrets();
        UpdatePlacementController();

        OnModifiersChanged?.Invoke();
    }

    private void ApplyModifiersToAllExistingTurrets()
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

    private void UpdatePlacementController()
    {
        var tp = TurretPlacementController.Instance;
        if (tp != null)
        {
            tp.maxTurretCapacity = tp.defaultMaxTurretCapacity + globalMaxTurretCapacityBonus;
            tp.placementRadius = tp.defaultPlacementRadius * globalPlacementRadiusMultiplier;
        }
    }

    public IReadOnlyList<BuildMasterModifier.Modifier> GetAppliedModifiers() => appliedModifiers;
}

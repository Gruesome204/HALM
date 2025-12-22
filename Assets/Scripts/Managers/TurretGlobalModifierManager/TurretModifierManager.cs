using System.Collections.Generic;
using UnityEngine;

public class TurretGlobalModifierManager : MonoBehaviour
{
    public static TurretGlobalModifierManager Instance { get; private set; }

    // Fired whenever any modifier changes (optional)
    public event System.Action OnModifiersChanged;
    // List of all modifiers the player has acquired
    private readonly List<TurretModifier> appliedModifiers = new List<TurretModifier>();
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ---------------- GLOBAL MODIFIERS -------------------
    // 1 == 100%
    // 1.1 == 110%

    public float globalTurretPlacementCooldownMultiplier = 1f;
    public float globalHealthMultiplier = 1f;
    public float globalDamageMultiplier = 1f;
    public float globalFireRateMultiplier = 1f;
    public int globalProjectilesPerSalve = 0;// additive
    public float globalProjectileSpeed = 1f;
    public int globalMaxTurretCapacityBonus = 0;  // additive
    public float globalPlacementRadiusMultiplier = 1f; 

    // Expand as needed...

    // Called by upgrades, research, player stats, etc.
    public void ApplyBuildMasterModifier(BuildMasterModifier.Modifier modifier)
    {
        // Apply player-related stats if needed (health, armor, movement speed)
        // Example: global player stats manager could handle this

        // Apply turret global stats
        globalTurretPlacementCooldownMultiplier *= Mathf.Max(0.01f, modifier.additionalStats.turretPlacementCooldownMultiplier);
        globalHealthMultiplier *= Mathf.Max(0.01f, modifier.additionalStats.turretHealthMultiplier);
        globalDamageMultiplier *= Mathf.Max(0.01f, modifier.additionalStats.turretDamageMultiplier);
        globalFireRateMultiplier *= Mathf.Max(0.01f, modifier.additionalStats.turretFireRateMultiplier);
        globalProjectilesPerSalve += modifier.additionalStats.turretProjectilesPerSalve;
        globalProjectileSpeed *= Mathf.Max(0.01f, modifier.additionalStats.turretProjectileSpeed);
        globalMaxTurretCapacityBonus += modifier.additionalStats.turretMaxCapacityBonus;
        globalPlacementRadiusMultiplier *= Mathf.Max(0.01f, modifier.additionalStats.turretPlacementRadiusMultiplier);


        // Recalculate stats for all existing turrets
        ApplyModifiersToAllExistingTurrets();

        // Update turret placement controller values
        var tp = TurretPlacementController.Instance;
        if (tp != null)
        {
            tp.maxTurretCapacity = tp.maxTurretCapacity + globalMaxTurretCapacityBonus;
            tp.placementRadius = tp.placementRadius * globalPlacementRadiusMultiplier;
        }


        // Notify UI / save systems
        OnModifiersChanged?.Invoke();
    }


    // Prevent 0 / negative numbers from breaking the system
    private float MultiplySafe(float current, float modifier)
    {
        return current * Mathf.Max(0.01f, modifier);
    }

    // ---------------- AUTO-APPLY TO ALL EXISTING TURRETS -------------------
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


    // ---------------- GET ALL APPLIED MODIFIERS (optional) -------------------
    public IReadOnlyList<TurretModifier> GetAppliedModifiers() => appliedModifiers;

}

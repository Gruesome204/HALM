using System.Collections.Generic;
using UnityEngine;

public class TurretModifierManager: MonoBehaviour
{
    public static TurretModifierManager Instance { get; private set; }

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
    public int globalProjectilesPerSalve = 0;
    public float globalProjectileSpeed = 1f;

    // Expand as needed...

    // Called by upgrades, research, player stats, etc.
    public void ApplyModifier(TurretModifier modifier)
    {
        appliedModifiers.Add(modifier);

        globalTurretPlacementCooldownMultiplier *= Mathf.Max(0.01f, modifier.turretPlacementCooldownMultiplier);
        globalHealthMultiplier *= Mathf.Max(0.01f, modifier.healthMultiplier);
        globalDamageMultiplier *= Mathf.Max(0.01f, modifier.damageMultiplier);
        globalFireRateMultiplier *= Mathf.Max(0.01f, modifier.fireRateMultiplier);
        globalProjectileSpeed *= Mathf.Max(0.01f, modifier.projectileSpeed);
        globalProjectilesPerSalve += modifier.projectilesPerSalve;



        // Apply changes to all existing turrets
        ApplyModifiersToAllExistingTurrets();

        // Notify UI or save systems
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

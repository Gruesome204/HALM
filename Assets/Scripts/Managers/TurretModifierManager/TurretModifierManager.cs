using UnityEngine;

public class TurretModifierManager: MonoBehaviour
{
    public static TurretModifierManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ---------------- GLOBAL MODIFIERS -------------------

    public float globalTurretPlacementCooldownMultiplier = 1f;       // < 1 = reduced cooldown
    public float globalHealthMultiplier = 1f;         // boosts max health
    public float globalDamageMultiplier = 1f;         // boosts turret damage
    public float globalFireRateMultiplier = 1f;    // reduces firing delay
    public int globalProjectilesPerSalve = 1;
    public float globalProjectileSpeed = 1f;

    // Expand as needed...

    // Called by upgrades, research, player stats, etc.
    public void ApplyModifier(TurretModifier modifier)
    {
        globalTurretPlacementCooldownMultiplier *= modifier.turretPlacementCooldownMultiplier;
        globalHealthMultiplier *= modifier.healthMultiplier;
        globalDamageMultiplier *= modifier.damageMultiplier;
        globalFireRateMultiplier *= modifier.fireRateMultiplier;
        globalProjectileSpeed *= modifier.projectileSpeed;
        globalProjectilesPerSalve *= modifier.projectilesPerSalve;


    }
}

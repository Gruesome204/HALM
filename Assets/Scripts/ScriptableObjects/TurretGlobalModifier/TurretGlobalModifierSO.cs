using UnityEngine;

[CreateAssetMenu(
    fileName = "TurretGlobalModifier",
    menuName = "Game/Turret/Global Modifier"
)]
public class TurretGlobalModifierSO : ScriptableObject
{
    [Header("Global Multipliers")]
    public float turretPlacementCooldownMultiplier = 1f;
    public float healthMultiplier = 1f;
    public float damageMultiplier = 1f;
    public float fireRateMultiplier = 1f;
    public float projectileSpeedMultiplier = 1f;

    [Header("Projectile Count")]
    public int projectilesPerSalve = 0;

    [Header("UI")]
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    public void ApplyToManager()
    {
        var modifier = new TurretModifier
        {
            turretPlacementCooldownMultiplier = turretPlacementCooldownMultiplier,
            healthMultiplier = healthMultiplier,
            damageMultiplier = damageMultiplier,
            fireRateMultiplier = fireRateMultiplier,
            projectileSpeed = projectileSpeedMultiplier,
            projectilesPerSalve = projectilesPerSalve
        };

        TurretGlobalModifierManager.Instance.ApplyModifier(modifier);
    }
}



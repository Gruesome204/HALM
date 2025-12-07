using UnityEngine;

[CreateAssetMenu(fileName = "New Turret Blueprint", menuName = "Game/Turret/New Turret Blueprint")]
public class TurretBlueprint : ScriptableObject
{
    [Header("Info")]
    [Tooltip("Name of the turret.")]
    public string turretName;

    [Header("Icon")]
    [Tooltip("Icon representing the turret in UI.")]
    public Sprite towerIcon;

    [Header("Turret Level")]
    [Tooltip("Starting level of the turret.")]
    [Min(1)]
    public int baseLevel = 1;

    [Tooltip("Description of the turret for UI or tooltips.")]
    [TextArea(3, 10)]
    public string description;

    [Header("Turret Type")]
    [Tooltip("Type/category of the turret.")]
    public TurretType turretType;

    [Tooltip("Visual-only prefab shown before placing the turret.")]
    public GameObject previewPrefab;

    [Tooltip("Prefab instantiated when the turret is placed in the world.")]
    public GameObject turretPrefab;

    public enum FiringPattern
    {
        None,
        SingleShot,
        FireSalve
    }

    [Tooltip("Shooting pattern of the turret.")]
    public FiringPattern firingPattern = FiringPattern.SingleShot;

    [Tooltip("Number of projectiles fired in one salve (only for FireSalve pattern).")]
    public int projectilesPerSalve = 2;

    [Tooltip("Delay between projectiles in a salve (only for FireSalve pattern).")]
    public float delayBetweenSalveProjectiles = 0.5f;

    [Header("Stat Values")]
    [Tooltip("Base health of the turret.")]
    public float baseHealth;

    [Header("Offensive Values")]
    [Tooltip("Base damage dealt per shot.")]
    public float baseAttackDamage = 10f;

    [Header("Base Values")]
    [Tooltip("Shots fired per second.")]
    public float baseFireRate = 1f;

    [Tooltip("Time between shots calculated from fire rate.")]
    public float BaseFireCountdown => 1f / baseFireRate;

    [Tooltip("Speed of the projectile when fired.")]
    public float baseProjectileSpeed = 10f;

    [Tooltip("Attack range of the turret.")]
    public float baseAttackRange = 5f;

    [Header("Knockback Values")]
    [Tooltip("Strength of knockback applied to enemies.")]
    public float baseKnockbackStrength = 1f;

    [Tooltip("Duration of knockback applied to enemies.")]
    public float baseKnockbackDuration = 0.5f;

    [Header("Per-Level Growth")]
    [Tooltip("Health scaling factor per turret level (1.2 = +20% per level).")]
    public float baseHealthGrowthFactor = 1.2f;

    [Tooltip("Damage scaling factor per turret level (1.2 = +20% per level).")]
    public float baseDamageGrowthFactor = 1.2f;

    [Tooltip("Fire rate scaling per level (0.95 = 5% faster per level).")]
    public float baseFireRateGrowthFactor = 0.95f;

    [Tooltip("Flat increase in range per level.")]
    public float baseRangeGrowthFlat = 0.5f;

    [Header("BuyCost")]
    [Tooltip("Resources required to build the turret.")]
    public int buildingCost;

    [Header("Turret Size")]
    [Tooltip("Height of the turret in grid cells.")]
    public int height;

    [Tooltip("Length of the turret in grid cells.")]
    public int length;

    [Tooltip("Size of the turret in grid cells (length x height).")]
    public Vector2Int sizeInCells => new Vector2Int(length, height);

    [Header("Build Capacity Value")]
    [Tooltip("Capacity value the turret takes when placed.")]
    public int buildCapacityValue = 1;

    [Header("Placement Cooldown")]
    [Tooltip("Cooldown time in seconds after placing this turret.")]
    public float placementCooldown = 1.0f;
}

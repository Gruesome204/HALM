using UnityEngine;

[CreateAssetMenu(fileName = "New Turret Blueprint", menuName = "Game/Turret/New Turret Blueprint")]
public class TurretBlueprint : ScriptableObject
{
    [Header("Info")]
    public string turretName;
    [Header("Turret Level")]
    public int baseLevel;
    [TextArea(3, 10)] public string description;
    [Header("Preview")]
    public GameObject previewPrefab; // 👈 purely visual prefab

    [Header("TurretType")]
    public TurretType turretType;

    [Header("Stat Values")]
    public float baseHealth;

    [Header("Defense Values")]

    [Header("Offensive Values")]
    public float baseAttackDamage = 10f;

    [Header("Base Values")]
    public float baseFireRate = 1f;
    public float baseFireCountdown = 0f;
    public float baseProjectileSpeed = 10f;
    public float baseAttackRange = 5f;

    [Header("Knockback Values")]
    public float baseKnockbackStrength = 1f;
    public float baseKnockbackDuration = 0.5f;

    [Header("Per-Level Growth")]
    [Tooltip("Damage scaling per level (1.2 = +20% per level)")]
    public float baseHealthGrowthFactor = 1.2f;
    [Tooltip("Damage scaling per level (1.2 = +20% per level)")]
    public float baseDamageGrowthFactor = 1.2f;

    [Tooltip("Fire rate scaling per level (0.95 = 5% faster per level)")]
    public float baseFireRateGrowthFactor = 0.95f;

    [Tooltip("Flat range increase per level")]
    public float baseRangeGrowthFlat = 0.5f;

    [Header("Cost")]
    public int buildingCost;
    public GameObject turretPrefab;

    [Header("Turret Size")]
    public int height;
    public int length;

    [Header("Build Capacity Value")]
    [Tooltip("Value the Tower takes to be build")]
    public int buildCapacityValue = 1;

    [Header("Placement Cooldown")]
    [Tooltip("Cooldown time after placing this turret type (seconds).")]
    public float placementCooldown = 1.0f;

}
using UnityEngine;

[CreateAssetMenu(fileName = "New Turret Blueprint", menuName = "Game/Turret/New Turret Blueprint")]
public class TurretBlueprint : ScriptableObject
{
    [Header("Info")]
    public string turretName;
    [TextArea(3, 10)] public string description;

    [Header("TurretType")]
    public TurretType turretType;

    [Header("Stat Values")]
    public float health;

    [Header("Defense Values")]

    [Header("Offensive Values")]
    public float attackDamage = 10f;

    [Header("Base Values")]
    public float fireRate = 1f;
    public float fireCountdown = 0f;
    public float projectileSpeed = 10f;
    public float attackRange = 5f;

    [Header("Knockback Values")]
    public float knockbackStrength = 1f;
    public float knockbackDuration = 0.5f;

    [Header("Per-Level Growth")]
    [Tooltip("Damage scaling per level (1.2 = +20% per level)")]
    public float damageGrowthFactor = 1.2f;

    [Tooltip("Fire rate scaling per level (0.95 = 5% faster per level)")]
    public float fireRateGrowthFactor = 0.95f;

    [Tooltip("Flat range increase per level")]
    public float rangeGrowthFlat = 0.5f;

    [Header("Cost")]
    public int buildingCost;
    public GameObject turretPrefab; 
}
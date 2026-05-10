using UnityEngine;

public class TurretStats : MonoBehaviour
{
    [SerializeField] private TurretBlueprint baseStats;

    [Header("Level")]
    public int currentLevel = 1;

    [Header("Current Stats")]
    public float currentHealth;
    public float currentMaxHealth;
    public float currentAttackDamage;
    public float currentAttackRange;

    public float currentShotsPerSecond;
    public float currentShotInterval;

    public float currentProjectileSpeed;
    public float currentKnockbackStrength;
    public float currentKnockbackDuration;

    [Header("Scaling Factors")]
    public float healthScaleFactor;
    public float damageScaleFactor;
    public float rangeScaleFlat;
    public float shotsPerSecondScaleFactor;

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (baseStats == null)
        {
            Debug.LogWarning("No base stats assigned to TurretStats");
            return;
        }

        currentLevel = baseStats.baseLevel;

        healthScaleFactor = baseStats.baseHealthGrowthFactor;
        damageScaleFactor = baseStats.baseDamageGrowthFactor;
        rangeScaleFlat = baseStats.baseRangeGrowthFlat;
        shotsPerSecondScaleFactor = baseStats.shotsPerSecondGrowthFactor;

        currentMaxHealth = baseStats.baseHealth * GetLevelScaling(healthScaleFactor);
        currentHealth = currentMaxHealth;

        currentAttackDamage = baseStats.baseAttackDamage * GetLevelScaling(damageScaleFactor);
        currentAttackRange = baseStats.baseAttackRange * GetLevelScaling(rangeScaleFlat);

        currentShotsPerSecond =
            baseStats.baseShotsPerSecond * GetLevelScaling(shotsPerSecondScaleFactor);

        currentShotInterval = 1f / currentShotsPerSecond;

        currentProjectileSpeed = baseStats.baseProjectileSpeed;
        currentKnockbackStrength = baseStats.baseKnockbackStrength;
        currentKnockbackDuration = baseStats.baseKnockbackDuration;
    }

    private float GetLevelScaling(float factor)
    {
        return ((currentLevel - 1) * factor) + 1;
    }

    public TurretBlueprint GetBlueprint()
    {
        return baseStats;
    }
}

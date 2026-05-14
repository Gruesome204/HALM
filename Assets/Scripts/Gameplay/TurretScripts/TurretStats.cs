using UnityEngine;

public class TurretStats : MonoBehaviour
{
    [SerializeField] private TurretBlueprint baseStats;

    [Header("Level")]
    public int currentLevel = 1;

    [Header("Health")]
    public float currentHealth;
    public float currentMaxHealth;

    [Header("Combat Stats")]
    public float currentAttackDamage;
    public float currentAttackRange;

    public float currentShotsPerSecond;
    public float currentShotInterval;

    public float currentProjectileSpeed;
    public int currentProjectilePierce;

    public float currentKnockbackStrength;
    public float currentKnockbackDuration;

    [Header("Firing")]
    public int currentProjectilesPerSalve;

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

        currentMaxHealth =
            baseStats.baseHealth *
            GetLevelScaling(healthScaleFactor);

        currentHealth = currentMaxHealth;

        currentAttackDamage =
            baseStats.baseAttackDamage *
            GetLevelScaling(damageScaleFactor);

        currentAttackRange =
            baseStats.baseAttackRange +
            (rangeScaleFlat * (currentLevel - 1));

        currentShotsPerSecond =
            baseStats.baseShotsPerSecond *
            GetLevelScaling(shotsPerSecondScaleFactor);

        currentShotInterval = 1f / currentShotsPerSecond;

        currentProjectileSpeed = baseStats.baseProjectileSpeed;
        currentProjectilePierce = baseStats.baseProjectilePierceCount;

        currentKnockbackStrength = baseStats.baseKnockbackStrength;
        currentKnockbackDuration = baseStats.baseKnockbackDuration;

        currentProjectilesPerSalve = baseStats.projectilesPerSalve;
    }
    public TurretStatData CalculateFinalStats(
    TurretBlueprint blueprint,
    int level,
    TurretModifier upgrade,
    TurretGlobalModifierManager global)
    {
        if (blueprint == null)
            return default;

        float scaledDamage =
            blueprint.baseAttackDamage *
            (1 + blueprint.baseDamageGrowthFactor * (level - 1));

        float scaledShotsPerSecond =
            blueprint.baseShotsPerSecond *
            (1 + blueprint.shotsPerSecondGrowthFactor * (level - 1));

        float scaledRange =
            blueprint.baseAttackRange +
            blueprint.baseRangeGrowthFlat * (level - 1);

        float scaledProjectileSpeed = blueprint.baseProjectileSpeed;

        // upgrades (additive)
        float damageWithUpgrade = scaledDamage + (upgrade?.damageMultiplier ?? 0f);
        float shotsWithUpgrade = scaledShotsPerSecond + (upgrade?.shotsPerSecondBonus ?? 0f);
        float speedWithUpgrade = scaledProjectileSpeed + (upgrade?.projectileSpeed ?? 0f);
        float rangeWithUpgrade = scaledRange + (upgrade?.rangeBonus ?? 0f);

        int projectilesWithUpgrade =
            blueprint.projectilesPerSalve + (upgrade?.projectilesPerSalve ?? 0);

        int pierceWithUpgrade =
            blueprint.baseProjectilePierceCount + (upgrade?.piercingHits ?? 0);

        // global multipliers
        float finalDamage =
            damageWithUpgrade * (1f + (global?.globalDamageMultiplier ?? 0f));

        float finalShotsPerSecond =
            shotsWithUpgrade * (1f + (global?.globalShotsPerSecondBonus ?? 0f));

        float finalProjectileSpeed =
            speedWithUpgrade * (1f + (global?.globalProjectileSpeed ?? 0f));

        float finalRange =
            rangeWithUpgrade * (1f + (global?.globalPlacementRadiusMultiplier ?? 0f));

        int finalProjectiles =
            projectilesWithUpgrade + (global?.globalProjectilesPerSalve ?? 0);

        return new TurretStatData
        {
            damage = finalDamage,
            shotsPerSecond = finalShotsPerSecond,
            range = finalRange,
            projectileSpeed = finalProjectileSpeed,
            projectilesPerSalve = finalProjectiles,
            pierceCount = pierceWithUpgrade,
            knockbackStrength = blueprint.baseKnockbackStrength,
            knockbackDuration = blueprint.baseKnockbackDuration
        };
    }
    public void RecalculateStats(
    TurretBehaviour behaviour,
    TurretBlueprint blueprint,
    int level,
    TurretModifier upgrade,
    TurretGlobalModifierManager global)
    {
        TurretStatData finalStats =
            CalculateFinalStats(blueprint, level, upgrade, global);

        currentAttackDamage = finalStats.damage;

        currentShotsPerSecond = Mathf.Max(0.1f, finalStats.shotsPerSecond);
        currentShotInterval = 1f / currentShotsPerSecond;

        currentAttackRange = finalStats.range;
        currentProjectileSpeed = finalStats.projectileSpeed;

        currentProjectilePierce = finalStats.pierceCount;

        currentKnockbackStrength = finalStats.knockbackStrength;
        currentKnockbackDuration = finalStats.knockbackDuration;

        currentProjectilesPerSalve = finalStats.projectilesPerSalve;

        // firing pattern decision still belongs to behaviour (optional split later)
        if (behaviour != null)
        {
            behaviour.SetFiringPattern(
                currentProjectilesPerSalve > 1
                    ? TurretBlueprint.FiringPattern.FireSalve
                    : blueprint.firingPattern
            );
        }
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
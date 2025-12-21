using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [SerializeField] public EnemyBaseStats baseStats;

    [Header("Level")]
    public int currentLevel = 1;

    [Header("Current Stats - Offensive")]
    public float currentDamage;
    public float currentAttackSpeed;
    public float currentCritChance;
    public float currentCritMultiplier;
    public float currentAttackRange;

    [Header("Current Stats - Defensive")]
    public float currentHealth;
    public float currentMaxHealth;
    public float currentArmor;
    public float currentMagicResistance;
    public float currentKnockbackReduction;
    public float currentKnockbackForce;
    public float currentKnockbackDuration;

    [Header("Current Stats - Movement / Detection")]
    public float currentMovementSpeed;
    public float currentDetectionRange;
    public float currentVisionRange;
    public float currentHearingRange;
    public float currentPursueRange;

    [Header("Experience Yield")]
    public float currentExperienceYield;

    [Header("Scaling Factors")]
    public float healthScaleFactor;
    public float damageScaleFactor;
    public float speedScaleFactor;
    public float armorScaleFactor;

    public void Initialize()
    {
        if (baseStats == null)
        {
            Debug.LogWarning("No base stats assigned to EnemyStats");
            return;
        }

        // Level
        currentLevel = baseStats.baseLevel;

        // Scaling Factors
        healthScaleFactor = baseStats.baseHealthScaleFactor;
        damageScaleFactor = baseStats.baseDamageScaleFactor;
        speedScaleFactor = baseStats.baseSpeedScaleFactor;
        armorScaleFactor = baseStats.baseArmorScaleFactor;

        // Defensive Stats
        currentMaxHealth = baseStats.baseMaxHealth * GetLevelScaling(healthScaleFactor);
        currentHealth = currentMaxHealth;
        currentArmor = baseStats.baseArmor * GetLevelScaling(armorScaleFactor);
        currentMagicResistance = baseStats.baseMagicResistance; // optional: scale if needed
        currentKnockbackReduction = Mathf.Clamp01(baseStats.baseKnockbackReduction);
        currentKnockbackForce = baseStats.baseKnockbackForce;
        currentKnockbackDuration = baseStats.baseKnockbackDuration;

        // Offensive Stats
        currentDamage = baseStats.baseDamage * GetLevelScaling(damageScaleFactor);
        currentAttackSpeed = baseStats.baseAttackSpeed; // could scale with level if desired
        currentCritChance = baseStats.baseCritChance;
        currentCritMultiplier = baseStats.baseCritHitMultiplier;
        currentAttackRange = baseStats.baseAttackRange;

        // Movement / Detection
        currentMovementSpeed = baseStats.baseMovementSpeed * GetLevelScaling(speedScaleFactor);
        currentDetectionRange = baseStats.baseDetectionRange;
        currentPursueRange = baseStats.pursueRange;

        // Experience
        currentExperienceYield = baseStats.experienceYield;
    }


    private float GetLevelScaling(float factor)
    {
        float result = ((currentLevel - 1) * factor) + 1;
        return result;
    }

    public void SetLevel(int level)
    {
        currentLevel = Mathf.Max(1, level);

        // Update max stats
        currentMaxHealth = baseStats.baseMaxHealth * GetLevelScaling(healthScaleFactor);
        currentArmor = baseStats.baseArmor * GetLevelScaling(armorScaleFactor);
        currentDamage = baseStats.baseDamage * GetLevelScaling(damageScaleFactor);
        currentMovementSpeed = baseStats.baseMovementSpeed * GetLevelScaling(speedScaleFactor);

        // Optional: keep currentHealth relative to new max
        currentHealth = Mathf.Min(currentHealth, currentMaxHealth);
    }
}

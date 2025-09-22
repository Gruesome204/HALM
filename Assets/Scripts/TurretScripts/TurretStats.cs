using UnityEngine;

public class TurretStats : MonoBehaviour
{
    [SerializeField] private TurretBlueprint baseStats;

    [Header("Level")]
    public int currentLevel = 1;

    [Header("Current Stats")]
    public float currentHealth;
    public float currentMaxHealth;
    public float currentDamage;
    public float currentAttackRange;
    public float currentFireRate;
    public float currentProjectileSpeed;
    public float currentKnockbackStrength;
    public float currentKnockbackDuration;

    [Header("Scaling Factors")]
    public float healthScaleFactor;
    public float damageScaleFactor;
    public float rangeScaleFactor;
    public float fireRateScaleFactor;

    public void Initialize()
    {
        if (baseStats == null)
        {
            Debug.LogWarning("No base stats assigned to TurretStats");
            return;
        }

        currentLevel = baseStats.baseLevel;

        healthScaleFactor = baseStats.baseHealthScaleFactor;
        damageScaleFactor = baseStats.baseDamageScaleFactor;
        rangeScaleFactor = baseStats.baseRangeScaleFactor;
        fireRateScaleFactor = baseStats.baseFireRateScaleFactor;

        currentMaxHealth = baseStats.baseHealth * GetLevelScaling(healthScaleFactor);
        currentHealth = currentMaxHealth;

        currentDamage = baseStats.baseDamage * GetLevelScaling(damageScaleFactor);
        currentAttackRange = baseStats.baseRange * GetLevelScaling(rangeScaleFactor);
        currentFireRate = baseStats.baseFireRate * GetLevelScaling(fireRateScaleFactor);

        currentProjectileSpeed = baseStats.baseProjectileSpeed;
        currentKnockbackStrength = baseStats.baseKnockbackStrength;
        currentKnockbackDuration = baseStats.baseKnockbackDuration;
    }

    private float GetLevelScaling(float factor)
    {
        return ((currentLevel - 1) * factor) + 1;
    }
}

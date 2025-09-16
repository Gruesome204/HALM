using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [SerializeField] private EnemyBaseStats baseStats;

    [Header("Level")]
    public int currentLevel = 1;

    

    [Header("Current Stats")]
    public float currentHealth;
    public float currentMaxHealth;
    public float currentArmor;
    public float currentMagicResistance;
    public float currentDamage;
    public float currentMovementSpeed;
    public float currentKnockbackReduction;

    [Header("Scaling Factors")]
    public float healthScaleFactor;
    public float damageScaleFactor;
    public float speedScaleFactor;
    public float armorScaleFactor;

    [Header("Experience Yield")]
    public float currentExperienceYield;

    public void Initialize()
    {
        if (baseStats == null)
        {
            Debug.LogWarning("No base stats assigned to EnemyStats");
            return;
        }
        currentLevel = baseStats.baseLevel;

        healthScaleFactor = baseStats.baseHealthScaleFactor;
        damageScaleFactor = baseStats.baseDamageScaleFactor;
        speedScaleFactor = baseStats.baseSpeedScaleFactor;
        armorScaleFactor = baseStats.baseArmorScaleFactor;

        currentMaxHealth = baseStats.baseHealth * GetLevelScaling(healthScaleFactor);
        currentHealth = currentMaxHealth;
        currentDamage = baseStats.baseDamage * GetLevelScaling(damageScaleFactor);
        currentArmor = baseStats.baseArmor * GetLevelScaling(armorScaleFactor);
        currentMovementSpeed = baseStats.baseMovementSpeed * GetLevelScaling(speedScaleFactor);
        currentKnockbackReduction = Mathf.Clamp01(currentKnockbackReduction);
        currentExperienceYield = baseStats.experienceYield;

    }

    private float GetLevelScaling(float factor)
    {
        float result = ((currentLevel - 1) * factor) + 1;
        return result;
    }
}

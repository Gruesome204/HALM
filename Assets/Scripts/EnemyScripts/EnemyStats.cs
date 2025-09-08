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
    public float healthScaleFactor = 1.1f;
    public float damageScaleFactor = 1.1f;
    public float speedScaleFactor = 1.05f;
    public float armorScaleFactor = 1.1f;

    [Header("Experience Yield")]
    public float currentExperienceYield;

    public void Initialize()
    {
        if (baseStats == null)
        {
            Debug.LogWarning("No base stats assigned to EnemyStats");
            return;
        }

        currentMaxHealth = baseStats.baseHealth * GetLevelScaling(healthScaleFactor);
        currentHealth = currentMaxHealth;
        currentDamage = baseStats.baseDamage * GetLevelScaling(damageScaleFactor);
        currentArmor = baseStats.baseArmor * GetLevelScaling(armorScaleFactor);
        currentMovementSpeed = baseStats.baseMovementSpeed * GetLevelScaling(speedScaleFactor);
        currentKnockbackReduction = Mathf.Clamp01(currentKnockbackReduction);
        currentExperienceYield = Mathf.RoundToInt(baseStats.experienceYield * GetLevelScaling(1.1f));
    }

    private float GetLevelScaling(float factor)
    {
        float result = 1f;
        for (int i = 1; i < currentLevel; i++)
        {
            result *= factor;
        }
        return result;
    }
}

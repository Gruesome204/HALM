using UnityEngine;

public class EnemyBehaviour : MonoBehaviour, IDamagable
{
    [SerializeField]private EnemyBaseStats baseStats;

    [Header("Level")]
    public int currentLevel = 1;

    [Header("Base Stats")]
    public float currentMaxHealth;
    public float currentHealth;
    public float currentDamage;
    public float currentArmor;
    public float currentAttackSpeed;

    [Header("Crit")]
    public float currentCritChance;
    public float currentCritHitMultiplier;

    [Header("Resistances")]
    public float currentMagicResistance;

    [Header("Ranges")]
    public float currentAttackRange;

    [Header("Movement")]
    public float currentMovementSpeed;

    [Header("Detection")]
    public float currentDetectionRange;
    public float currentVisionRange;
    public float currenthearingRange;
    public float pursueRange;

    [Header("Progression")]

    public float experienceYield;

    [Header("Scaling Factors")]
    public float healthScaleFactor;
    public float damageScaleFactor;
    public float speedScaleFactor;
    public float armorScaleFactor;

    private void Awake()
    {
        InitalizeEnemyStats();
    }

    public void InitalizeEnemyStats()
    {
        if (baseStats != null)
        {
            // Initialize current stats with base stats
            currentMaxHealth = baseStats.baseHealth * GetLevelScalingFactor(healthScaleFactor);
            currentHealth = currentMaxHealth; // Set current health to max health initially
            currentDamage = baseStats.baseDamage * GetLevelScalingFactor(damageScaleFactor);
            currentArmor = baseStats.baseArmor * GetLevelScalingFactor(armorScaleFactor);
            currentMovementSpeed = baseStats.baseMovementSpeed * GetLevelScalingFactor(speedScaleFactor);
        }
        else
        {
            Debug.LogError("EnemyBaseStats component not found on " + gameObject.name);
            currentMaxHealth = 300f;
            currentHealth = 300f;
            currentDamage = 10f;
            currentArmor = 0f;
            currentMovementSpeed = 2f;

        }
    }

    float GetLevelScalingFactor(float baseFactor)
    {
        float factor = 1f;
        for (int i = 1; i < currentLevel; i++)
        {
            factor *= baseFactor;
        }
        return factor;
    }

    public bool IsInvulnerable { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    //Effect/visual/audio effect upon taking dmg
    public void OnDamageTaken(float amount)
    {
    }


    public void TakeDamage(DamageData damageData)
    {
        currentHealth -= CalculateTakenDamage_PercentageLinear(damageData.amount, currentArmor);
        Debug.Log($"{this.gameObject.name} received {damageData.amount} {damageData.type} Damage from {damageData.source.name}");
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public float CalculateTakenDamage_PercentageLinear(float incomingDamage, float armor)
    {
        float damageReductionPercentage = armor / 100f; // Assuming 1 armor point = 1% reduction
        float takenDamage = incomingDamage * (1f - Mathf.Clamp01(damageReductionPercentage));
        return takenDamage;
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}
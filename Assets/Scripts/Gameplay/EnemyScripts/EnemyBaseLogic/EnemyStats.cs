using UnityEngine;

[DefaultExecutionOrder(-100)]
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
    public float maxHealth;
    public float currentHealth; // Still store it, but only EnemyHealth modifies it
    public float currentArmor;
    public float currentMagicResistance;
    public float currentKnockbackReduction;
    public float currentKnockbackForce;
    public float currentKnockbackDuration;

    public bool isKnockbackImmune;

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

    #region Unity Callbacks
    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        // Only re-initialize if we haven't been set up yet
        if (maxHealth <= 0)
        {
            Initialize();
        }
    }
    #endregion

    public void Initialize()
    {
        if (baseStats == null)
        {
            Debug.LogWarning($"{gameObject.name}: No base stats assigned to EnemyStats");
            return;
        }

        Debug.Log($"[EnemyStats] {gameObject.name}: Initializing stats...");

        // Level
        currentLevel = baseStats.baseLevel;

        // Scaling Factors
        healthScaleFactor = baseStats.baseHealthScaleFactor;
        damageScaleFactor = baseStats.baseDamageScaleFactor;
        speedScaleFactor = baseStats.baseSpeedScaleFactor;
        armorScaleFactor = baseStats.baseArmorScaleFactor;

        // Defensive Stats - Only calculate max health, don't set current health
        maxHealth = baseStats.baseMaxHealth * GetLevelScaling(healthScaleFactor);
        // currentHealth will be set by EnemyHealth
        currentArmor = baseStats.baseArmor * GetLevelScaling(armorScaleFactor);
        currentMagicResistance = baseStats.baseMagicResistance;
        currentKnockbackReduction = Mathf.Clamp01(baseStats.baseKnockbackReduction);
        currentKnockbackForce = baseStats.baseKnockbackForce;
        currentKnockbackDuration = baseStats.baseKnockbackDuration;

        // Offensive Stats
        currentDamage = baseStats.baseDamage * GetLevelScaling(damageScaleFactor);
        currentAttackSpeed = baseStats.baseAttackSpeed;
        currentCritChance = baseStats.baseCritChance;
        currentCritMultiplier = baseStats.baseCritHitMultiplier;
        currentAttackRange = baseStats.baseAttackRange;

        // Movement / Detection
        currentMovementSpeed = baseStats.baseMovementSpeed * GetLevelScaling(speedScaleFactor);
        currentDetectionRange = baseStats.baseDetectionRange;
        currentPursueRange = baseStats.pursueRange;

        // Experience
        currentExperienceYield = baseStats.experienceYield;

        Debug.Log($"[EnemyStats] {gameObject.name}: Stats initialized - MaxHealth: {maxHealth}, Damage: {currentDamage}, Armor: {currentArmor}, Speed: {currentMovementSpeed}");
    }

    private float GetLevelScaling(float factor)
    {
        return ((currentLevel - 1) * factor) + 1;
    }

    public void SetLevel(int level)
    {
        currentLevel = Mathf.Max(1, level);

        // Update max stats
        maxHealth = baseStats.baseMaxHealth * GetLevelScaling(healthScaleFactor);
        currentArmor = baseStats.baseArmor * GetLevelScaling(armorScaleFactor);
        currentDamage = baseStats.baseDamage * GetLevelScaling(damageScaleFactor);
        currentMovementSpeed = baseStats.baseMovementSpeed * GetLevelScaling(speedScaleFactor);

        // Clamp current health to new max (let EnemyHealth handle this)
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

}
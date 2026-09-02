using UnityEngine;
using static EnemyBaseBossStatsSO;

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
    public float currentHealth;
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

    // Boss-specific properties (lazily cached)
    private EnemyBaseBossStatsSO _bossStats;
    public EnemyBaseBossStatsSO BossStats => _bossStats;

    #region Unity Callbacks
    private void Awake()
    {
        // Cache boss stats if this is a boss
        _bossStats = baseStats as EnemyBaseBossStatsSO;
        Initialize();
    }

    private void OnEnable()
    {
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

        // Use the base stats' methods for scaling (which handle boss overrides)
        maxHealth = baseStats.GetScaledHealth(currentLevel);
        currentDamage = baseStats.GetScaledDamage(currentLevel);

        // Current health will be set by EnemyHealth
        currentArmor = baseStats.baseArmor * GetLevelScaling(armorScaleFactor);
        currentMagicResistance = baseStats.baseMagicResistance;
        currentKnockbackReduction = Mathf.Clamp01(baseStats.baseKnockbackReduction);
        currentKnockbackForce = baseStats.baseKnockbackForce;
        currentKnockbackDuration = baseStats.baseKnockbackDuration;

        // Offensive Stats
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

        // Update stats using the proper scaling methods
        maxHealth = baseStats.GetScaledHealth(currentLevel);
        currentDamage = baseStats.GetScaledDamage(currentLevel);
        currentArmor = baseStats.baseArmor * GetLevelScaling(armorScaleFactor);
        currentMovementSpeed = baseStats.baseMovementSpeed * GetLevelScaling(speedScaleFactor);

        // Clamp current health to new max
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    // Helper method to check if this is a boss
    public bool IsBoss()
    {
        return _bossStats != null;
    }

    // Helper method to get boss-specific properties
    public EnemyBaseBossStatsSO GetBossStats()
    {
        return _bossStats;
    }

    // Boss-specific methods
    public PhaseConfig GetPhaseForHealth(float healthPercent)
    {
        if (_bossStats == null || !_bossStats.isMultiStageBoss)
            return null;

        foreach (var phase in _bossStats.phases)
        {
            if (healthPercent <= phase.healthThreshold)
                return phase;
        }
        return null;
    }

    public float GetEnrageDamageMultiplier()
    {
        if (_bossStats == null)
            return 1f;

        // Check if enrage timer has elapsed (you'll need to track this separately)
        // For now, return the multiplier
        return _bossStats.enrageDamageMultiplier;
    }
}
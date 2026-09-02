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

    // Current boss phase tracking
    private int _currentPhaseIndex = -1;
    public int CurrentPhaseIndex
    {
        get => _currentPhaseIndex;
        set => _currentPhaseIndex = value;
    }

    #region Unity Callbacks
    private void Awake()
    {
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

        currentLevel = baseStats.baseLevel;

        healthScaleFactor = baseStats.baseHealthScaleFactor;
        damageScaleFactor = baseStats.baseDamageScaleFactor;
        speedScaleFactor = baseStats.baseSpeedScaleFactor;
        armorScaleFactor = baseStats.baseArmorScaleFactor;

        // Use base stats methods for scaling
        maxHealth = baseStats.GetScaledHealth(currentLevel);
        currentDamage = baseStats.GetScaledDamage(currentLevel);

        currentArmor = baseStats.baseArmor * GetLevelScaling(armorScaleFactor);
        currentMagicResistance = baseStats.baseMagicResistance;
        currentKnockbackReduction = Mathf.Clamp01(baseStats.baseKnockbackReduction);
        currentKnockbackForce = baseStats.baseKnockbackForce;
        currentKnockbackDuration = baseStats.baseKnockbackDuration;

        currentAttackSpeed = baseStats.baseAttackSpeed;
        currentCritChance = baseStats.baseCritChance;
        currentCritMultiplier = baseStats.baseCritHitMultiplier;
        currentAttackRange = baseStats.baseAttackRange;

        currentMovementSpeed = baseStats.baseMovementSpeed * GetLevelScaling(speedScaleFactor);
        currentDetectionRange = baseStats.baseDetectionRange;
        currentPursueRange = baseStats.pursueRange;

        currentExperienceYield = baseStats.experienceYield;

        // Initialize boss phase tracking
        if (IsBoss() && BossStats.isMultiStageBoss)
        {
            _currentPhaseIndex = 0; // Start at Phase 1 (index 0)
        }

        Debug.Log($"[EnemyStats] {gameObject.name}: Stats initialized - MaxHealth: {maxHealth}, Damage: {currentDamage}, Armor: {currentArmor}, Speed: {currentMovementSpeed}");
    }

    private float GetLevelScaling(float factor)
    {
        return ((currentLevel - 1) * factor) + 1;
    }

    public void SetLevel(int level)
    {
        currentLevel = Mathf.Max(1, level);

        maxHealth = baseStats.GetScaledHealth(currentLevel);
        currentDamage = baseStats.GetScaledDamage(currentLevel);
        currentArmor = baseStats.baseArmor * GetLevelScaling(armorScaleFactor);
        currentMovementSpeed = baseStats.baseMovementSpeed * GetLevelScaling(speedScaleFactor);

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    // Helper methods
    public bool IsBoss()
    {
        return _bossStats != null;
    }

    public EnemyBaseBossStatsSO GetBossStats()
    {
        return _bossStats;
    }

    // Phase management
    public PhaseConfig GetCurrentPhase()
    {
        if (!IsBoss() || !BossStats.isMultiStageBoss || _currentPhaseIndex < 0 || _currentPhaseIndex >= BossStats.phases.Length)
            return null;

        return BossStats.phases[_currentPhaseIndex];
    }

    public PhaseConfig GetPhaseForHealthPercent(float healthPercent)
    {
        if (!IsBoss() || !BossStats.isMultiStageBoss || BossStats.phases == null || BossStats.phases.Length == 0)
            return null;

        // Find the phase that matches this health threshold (from highest threshold to lowest)
        for (int i = BossStats.phases.Length - 1; i >= 0; i--)
        {
            if (healthPercent <= BossStats.phases[i].healthThreshold)
                return BossStats.phases[i];
        }

        // If no phase matches, return the first phase (assumes first phase is default)
        return BossStats.phases[0];
    }

    public int GetPhaseIndexForHealthPercent(float healthPercent)
    {
        if (!IsBoss() || !BossStats.isMultiStageBoss || BossStats.phases == null || BossStats.phases.Length == 0)
            return -1;

        for (int i = BossStats.phases.Length - 1; i >= 0; i--)
        {
            if (healthPercent <= BossStats.phases[i].healthThreshold)
                return i;
        }

        return 0; // Default to first phase
    }

    public bool TryUpdatePhase(float healthPercent)
    {
        if (!IsBoss() || !BossStats.isMultiStageBoss)
            return false;

        int newPhaseIndex = GetPhaseIndexForHealthPercent(healthPercent);

        if (newPhaseIndex != _currentPhaseIndex)
        {
            _currentPhaseIndex = newPhaseIndex;
            return true; // Phase changed
        }

        return false; // No phase change
    }

    public void ApplyPhaseEffects(PhaseConfig phase)
    {
        if (phase == null) return;

        // Apply phase modifiers
        currentDamage = baseStats.GetScaledDamage(currentLevel) * phase.damageMultiplier;
        currentMovementSpeed = baseStats.baseMovementSpeed * GetLevelScaling(speedScaleFactor) * phase.speedMultiplier;

        // You can add more phase effects here
        Debug.Log($"[Boss] Applied phase effects: Damage x{phase.damageMultiplier}, Speed x{phase.speedMultiplier}");
    }

    public float GetBossEnrageMultiplier()
    {
        if (!IsBoss()) return 1f;
        return BossStats.enrageDamageMultiplier;
    }
}
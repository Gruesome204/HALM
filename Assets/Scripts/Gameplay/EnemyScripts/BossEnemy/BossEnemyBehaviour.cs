using UnityEngine;

public class BossEnemyBehaviour : EnemyBehaviour, IPausable
{

    [Header("Boss UI")]
    [SerializeField] private BossBarUI bossBarUI;
    public BossPhase CurrentPhase { get; private set; } = BossPhase.Phase1;

    // Cache the boss stats for easier access
    private EnemyBaseBossStatsSO bossStats;
    private BossPhaseConfig currentPhaseConfig;
    private int currentPhaseIndex = -1;

    // Add a flag to prevent recursive phase updates
    private bool isUpdatingPhase = false;
    // Track the last applied phase to prevent re-entering the same phase
    private int lastAppliedPhaseIndex = -1;

    // NEW: Track which phases have already been entered
    private bool[] enteredPhases;

    protected override void Awake()
    {
        // Auto-detect components if not assigned
        AutoDetectComponents();

        base.Awake();

        if (bossBarUI == null)
            bossBarUI = FindObjectOfType<BossBarUI>();
    }

    private void Start()
    {
        // Call base Start to register with GameManager and EnemySpawnManager
        base.Start();

        SetupBossBar();
        CacheBossStats();
        InitializePhaseSystem();
    }

    private void CacheBossStats()
    {
        if (stats != null && stats.baseStats is EnemyBaseBossStatsSO)
        {
            bossStats = stats.baseStats as EnemyBaseBossStatsSO;

            if (bossStats != null && bossStats.isMultiStageBoss && bossStats.phaseConfigs.Length > 0)
            {
                enteredPhases = new bool[bossStats.phaseConfigs.Length];
            }
        }
    }

    private void InitializePhaseSystem()
    {
        if (bossStats == null || !bossStats.isMultiStageBoss || bossStats.phaseConfigs.Length == 0)
        {
            // If no phases configured, use default phase 1
            CurrentPhase = BossPhase.Phase1;
            return;
        }

        // Start with phase 0 (first phase)
        ApplyPhase(0);
    }

    private void AutoDetectComponents()
    {
        // Auto-detect stats if not assigned
        if (stats == null)
        {
            stats = GetComponent<EnemyStats>();
            if (stats == null)
                Debug.LogError($"[BossEnemyBehaviour] {gameObject.name}: No EnemyStats component found!");
        }

        // Auto-detect health if not assigned
        if (health == null)
        {
            health = GetComponent<EnemyHealth>();
            if (health == null)
                Debug.LogError($"[BossEnemyBehaviour] {gameObject.name}: No EnemyHealth component found!");
        }

        // Auto-detect movement if not assigned
        if (movement == null)
        {
            movement = GetComponent<EnemyMovement>();
            if (movement == null)
                Debug.LogWarning($"[BossEnemyBehaviour] {gameObject.name}: No EnemyMovement component found!");
        }

        // Auto-detect ability behaviour if not assigned
        if (abilityBehaviour == null)
        {
            abilityBehaviour = GetComponent<EnemyAbilityBehaviour>();
            if (abilityBehaviour == null)
                Debug.LogWarning($"[BossEnemyBehaviour] {gameObject.name}: No EnemyAbilityBehaviour component found!");
        }

        // Auto-detect knockback if not assigned
        if (knockback == null)
        {
            knockback = GetComponent<EnemyKnockback>();
            if (knockback == null)
                Debug.LogWarning($"[BossEnemyBehaviour] {gameObject.name}: No EnemyKnockback component found!");
        }

        // Auto-detect attack if not assigned
        if (attack == null)
        {
            attack = GetComponent<EnemyAttack>();
            if (attack == null)
                Debug.LogWarning($"[BossEnemyBehaviour] {gameObject.name}: No EnemyAttack component found!");
        }

        // Auto-detect enemy animator if not assigned
        if (enemyAnimator == null)
        {
            enemyAnimator = GetComponent<EnemyAnimator>();
            if (enemyAnimator == null)
                Debug.LogWarning($"[BossEnemyBehaviour] {gameObject.name}: No EnemyAnimator component found!");
        }
    }

    private void SetupBossBar()
    {
        if (bossBarUI == null)
        {
            Debug.LogError("[BossEnemyBehaviour] BossBarUI not found!");
            return;
        }

        if (stats == null)
        {
            Debug.LogError("[BossEnemyBehaviour] EnemyStats not found!");
            return;
        }

        if (health == null)
        {
            Debug.LogError("[BossEnemyBehaviour] EnemyHealth not found!");
            return;
        }

        // Get the boss stats from the EnemyStats component
        EnemyBaseBossStatsSO bossStats = stats.baseStats as EnemyBaseBossStatsSO;
        if (bossStats == null)
        {
            Debug.LogError($"[BossEnemyBehaviour] {gameObject.name}: EnemyStats.baseStats is not a BossStats ScriptableObject!");
            return;
        }

        bossBarUI.SetupBossBar(bossStats);
        bossBarUI.SetHealth(health.CurrentHealth, health.MaxHealth);
        bossBarUI.ShowBossBar();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDamaged += HandleDamaged;
            health.OnHealed += HandleHealed;
            health.OnHealthChanged += HandleHealthChanged;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDamaged -= HandleDamaged;
            health.OnHealed -= HandleHealed;
            health.OnHealthChanged -= HandleHealthChanged;
        }

        if (bossBarUI != null)
            bossBarUI.HideBossBar();
    }

    protected override void HandleDamaged(DamageData damageData, KnockbackData knockbackData)
    {
        base.HandleDamaged(damageData, knockbackData);

        if (!isUpdatingPhase)
        {
            UpdatePhase();
        }
    }

    private void HandleHealed(float healAmount)
    {
        // Boss bar updates via HandleHealthChanged event
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        if (bossBarUI != null)
            bossBarUI.SetHealth(currentHealth, maxHealth);

        if (!isUpdatingPhase)
        {
            UpdatePhase();
        }
    }

    private void UpdatePhase()
    {
        // Prevent recursive phase updates
        if (isUpdatingPhase)
        {
            Debug.LogWarning($"[BossEnemyBehaviour] {gameObject.name}: Attempted recursive phase update, blocked.");
            return;
        }

        if (stats == null || health == null || bossStats == null)
            return;

        // Don't process phase updates if the boss is dead
        if (!health.IsAlive())
            return;

        float hpPercent = health.CurrentHealth / health.MaxHealth;

        // Check if we should transition to a new phase
        if (bossStats.isMultiStageBoss && bossStats.phaseConfigs.Length > 0)
        {
            int newPhaseIndex = GetNextUnenteredPhaseIndex(hpPercent);

            if (newPhaseIndex >= 0 && newPhaseIndex != currentPhaseIndex)
            {
                // Only update if the phase hasn't been entered yet
                if (!IsPhaseEntered(newPhaseIndex))
                {
                    ApplyPhase(newPhaseIndex);
                }
                else
                {
                    Debug.Log($"[BossEnemyBehaviour] {gameObject.name}: Phase {newPhaseIndex} already entered, skipping.");
                }
            }
        }
    }

    private int GetNextUnenteredPhaseIndex(float healthPercent)
    {
        if (enteredPhases == null || bossStats == null)
            return -1;

        int targetPhase = -1;

        for (int i = 0; i < bossStats.phaseConfigs.Length; i++)
        {
            float threshold = bossStats.phaseConfigs[i].healthThreshold;


            if (healthPercent <= threshold && !enteredPhases[i])
            {
                targetPhase = i;
            }
        }

        return targetPhase;
    }

    private bool IsPhaseEntered(int phaseIndex)
    {
        if (enteredPhases == null || phaseIndex < 0 || phaseIndex >= enteredPhases.Length)
            return false;
        return enteredPhases[phaseIndex];
    }

    private void ApplyPhase(int phaseIndex)
    {
        // Prevent re-entering the same phase
        if (phaseIndex == lastAppliedPhaseIndex && phaseIndex == currentPhaseIndex)
        {
            Debug.Log($"[BossEnemyBehaviour] {gameObject.name}: Phase {phaseIndex} already applied, skipping.");
            return;
        }

        if (IsPhaseEntered(phaseIndex))
        {
            Debug.Log($"[BossEnemyBehaviour] {gameObject.name}: Phase {phaseIndex} has already been entered, cannot enter again.");
            return;
        }

        // Prevent recursive phase application
        if (isUpdatingPhase)
        {
            Debug.LogWarning($"[BossEnemyBehaviour] {gameObject.name}: Recursive phase application detected, blocking.");
            return;
        }

        if (bossStats == null || phaseIndex < 0 || phaseIndex >= bossStats.phaseConfigs.Length)
        {
            Debug.LogWarning($"[BossEnemyBehaviour] {gameObject.name}: Invalid phase index: {phaseIndex}");
            return;
        }

        BossPhaseConfig newConfig = bossStats.phaseConfigs[phaseIndex];
        if (newConfig == null) return;

        // Set the flag to prevent recursive updates during phase application
        isUpdatingPhase = true;

        try
        {
            enteredPhases[phaseIndex] = true;

            // Store the current phase config
            currentPhaseConfig = newConfig;
            currentPhaseIndex = phaseIndex;

            // Track the last applied phase to prevent re-entry
            lastAppliedPhaseIndex = phaseIndex;

            CurrentPhase = (BossPhase)Mathf.Min(phaseIndex + 1, (int)BossPhase.Phase3);

            Debug.Log($"[BossEnemyBehaviour] {gameObject.name} entering phase {phaseIndex + 1}: {newConfig.phaseName}");

            // Apply phase effects with delay if specified
            if (newConfig.effectDelay > 0)
            {
                Invoke(nameof(ApplyPhaseEffects), newConfig.effectDelay);
            }
            else
            {
                ApplyPhaseEffects();
            }

            // Show phase change in UI if available
            if (bossBarUI != null)
            {
                bossBarUI.ShowPhaseChange(newConfig.phaseName, newConfig.healthThreshold);
            }
        }
        finally
        {
            // Always clear the flag after phase application, even if an exception occurs
            isUpdatingPhase = false;
        }
    }

    private void ApplyPhaseEffects()
    {
        if (currentPhaseConfig == null) return;

        // Apply heal if specified
        if (currentPhaseConfig.healAmount > 0 && health != null && health.IsAlive())
        {
            // Store current health to check if we need to re-evaluate phases
            float currentHealthPercent = health.CurrentHealth / health.MaxHealth;

            // Apply the heal
            health.Heal(currentPhaseConfig.healAmount);

            Debug.Log($"[BossEnemyBehaviour] Phase heal applied: {currentPhaseConfig.healAmount}");
            float newHealthPercent = health.CurrentHealth / health.MaxHealth;

            float currentPhaseThreshold = currentPhaseConfig.healthThreshold;
            if (newHealthPercent > currentPhaseThreshold)
            {
                Debug.Log($"[BossEnemyBehaviour] {gameObject.name}: Heal pushed health above phase threshold ({newHealthPercent:F2} > {currentPhaseThreshold:F2}). Staying in current phase.");
            }
        }

        // Apply speed multiplier
        if (movement != null)
        {
            movement.SetSpeedMultiplier(currentPhaseConfig.speedMultiplier);
        }

        // Update UI elements with phase info
        if (bossBarUI != null)
        {
            bossBarUI.SetPhaseInfo(currentPhaseConfig.phaseName, currentPhaseConfig.healthThreshold);
        }
    }

    public BossPhaseConfig GetCurrentPhaseConfig()
    {
        return currentPhaseConfig;
    }

    public int GetCurrentPhaseIndex()
    {
        return currentPhaseIndex;
    }

    public bool IsInPhase(int phaseIndex)
    {
        return currentPhaseIndex == phaseIndex;
    }

    public float GetNextPhaseThreshold()
    {
        if (bossStats == null || currentPhaseIndex < 0 || currentPhaseIndex >= bossStats.phaseConfigs.Length - 1)
            return 0f;

        return bossStats.phaseConfigs[currentPhaseIndex + 1].healthThreshold;
    }

    public bool HasPhaseBeenEntered(int phaseIndex)
    {
        return IsPhaseEntered(phaseIndex);
    }
    public bool[] GetEnteredPhases()
    {
        return enteredPhases;
    }

    public void ResetPhaseTracking()
    {
        if (enteredPhases != null)
        {
            for (int i = 0; i < enteredPhases.Length; i++)
            {
                enteredPhases[i] = false;
            }
        }
        currentPhaseIndex = -1;
        lastAppliedPhaseIndex = -1;
        CurrentPhase = BossPhase.Phase1;
    }
}
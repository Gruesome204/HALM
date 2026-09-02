using UnityEngine;

public class BossEnemyBehaviour : EnemyBehaviour
{
    [Header("Boss UI")]
    [SerializeField] private BossBarUI bossBarUI;
    [SerializeField] private EnemyStats stats;

    private bool isInitialized = false;
    private bool bossPhaseChanged = false;

    protected override void Awake()
    {
        AutoDetectComponents();
        base.Awake();

        if (bossBarUI == null)
            bossBarUI = FindObjectOfType<BossBarUI>();

        if (stats == null)
            stats = GetComponent<EnemyStats>();
    }

    private void Start()
    {
        SetupBossBar();
        isInitialized = true;

        // Initialize boss phase
        if (stats != null && stats.IsBoss() && stats.BossStats.isMultiStageBoss)
        {
            float healthPercent = health != null ? health.CurrentHealth / health.MaxHealth : 1f;
            stats.TryUpdatePhase(healthPercent);
            UpdateBossPhaseUI();
        }

        Debug.Log($"{name} initialized at phase {stats?.CurrentPhaseIndex}");
    }

    private void AutoDetectComponents()
    {
        if (stats == null)
        {
            stats = GetComponent<EnemyStats>();
            if (stats == null)
                Debug.LogError($"[BossEnemyBehaviour] {gameObject.name}: No EnemyStats component found!");
        }

        if (health == null)
        {
            health = GetComponent<EnemyHealth>();
            if (health == null)
                Debug.LogError($"[BossEnemyBehaviour] {gameObject.name}: No EnemyHealth component found!");
        }

        if (movement == null)
        {
            movement = GetComponent<EnemyMovement>();
            if (movement == null)
                Debug.LogWarning($"[BossEnemyBehaviour] {gameObject.name}: No EnemyMovement component found!");
        }

        if (abilityBehaviour == null)
        {
            abilityBehaviour = GetComponent<EnemyAbilityBehaviour>();
            if (abilityBehaviour == null)
                Debug.LogWarning($"[BossEnemyBehaviour] {gameObject.name}: No EnemyAbilityBehaviour component found!");
        }

        if (knockback == null)
        {
            knockback = GetComponent<EnemyKnockback>();
            if (knockback == null)
                Debug.LogWarning($"[BossEnemyBehaviour] {gameObject.name}: No EnemyKnockback component found!");
        }

        if (attack == null)
        {
            attack = GetComponent<EnemyAttack>();
            if (attack == null)
                Debug.LogWarning($"[BossEnemyBehaviour] {gameObject.name}: No EnemyAttack component found!");
        }

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

        if (stats == null || !stats.IsBoss())
        {
            Debug.LogError("[BossEnemyBehaviour] Boss stats not found!");
            return;
        }

        if (health == null)
        {
            Debug.LogError("[BossEnemyBehaviour] EnemyHealth not found!");
            return;
        }

        // Setup boss bar with boss stats
        bossBarUI.SetupBossBar(stats.BossStats);
        bossBarUI.SetHealth(health.CurrentHealth, health.MaxHealth);
        bossBarUI.ShowBossBar();

        // Update phase UI if multi-stage
        if (stats.BossStats.isMultiStageBoss && stats.CurrentPhaseIndex >= 0)
        {
            bossBarUI.ForcePhaseChange(stats.CurrentPhaseIndex);
        }
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDamaged += HandleDamaged;
            health.OnHealthChanged += HandleHealthChanged;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDamaged -= HandleDamaged;
            health.OnHealthChanged -= HandleHealthChanged;
        }

        if (bossBarUI != null)
            bossBarUI.HideBossBar();
    }

    protected override void HandleDamaged(DamageData damageData, KnockbackData knockbackData)
    {
        if (!isInitialized) return;

        // Apply enrage damage multiplier if applicable
        if (stats != null && stats.IsBoss())
        {
            // Check if enrage timer is active (you'll need to track this)
            // For now, just apply the multiplier if enrageTimer > 0
            if (stats.BossStats.enrageTimer > 0)
            {
                damageData.amount *= stats.GetBossEnrageMultiplier();
            }
        }

        base.HandleDamaged(damageData, knockbackData);
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        if (!isInitialized) return;

        // Update boss bar
        if (bossBarUI != null)
            bossBarUI.SetHealth(currentHealth, maxHealth);

        // Check for phase changes
        if (stats != null && stats.IsBoss() && stats.BossStats.isMultiStageBoss)
        {
            float healthPercent = currentHealth / maxHealth;

            if (stats.TryUpdatePhase(healthPercent))
            {
                // Phase changed!
                var phase = stats.GetCurrentPhase();
                if (phase != null)
                {
                    Debug.Log($"[Boss] Phase changed to: {phase.phaseName}");

                    // Apply phase effects
                    stats.ApplyPhaseEffects(phase);

                    // Update UI
                    UpdateBossPhaseUI();

                    // Heal boss on phase entry if configured
                    if (phase.healOnEnter > 0 && health != null)
                    {
                        health.Heal(phase.healOnEnter);
                    }

                    // Trigger phase entry effects
                    if (phase.phaseEntryEffects != null)
                    {
                        foreach (var effect in phase.phaseEntryEffects)
                        {
                            if (effect != null)
                                Instantiate(effect, transform.position, Quaternion.identity);
                        }
                    }

                    // Show phase announcement
                    if (!string.IsNullOrEmpty(phase.phaseAnnouncement))
                    {
                        // Show announcement in UI (you'll need to implement this)
                        Debug.Log($"<color=red>BOSS ANNOUNCEMENT:</color> {phase.phaseAnnouncement}");
                    }

                    // Unlock new abilities
                    if (phase.unlockedAbilities != null && abilityBehaviour != null)
                    {
                        foreach (var ability in phase.unlockedAbilities)
                        {
                            // Unlock ability (you'll need to implement this)
                            Debug.Log($"[Boss] Unlocked ability: {ability.abilityName}");
                        }
                    }
                }
            }
        }
    }

    private void UpdateBossPhaseUI()
    {
        if (bossBarUI != null && stats != null && stats.IsBoss())
        {
            bossBarUI.ForcePhaseChange(stats.CurrentPhaseIndex);
        }
    }

    // Public method to manually force a phase change (for testing or events)
    public void ForcePhaseChange(int phaseIndex)
    {
        if (stats != null && stats.IsBoss() && stats.BossStats.isMultiStageBoss)
        {
            stats.CurrentPhaseIndex = phaseIndex;
            UpdateBossPhaseUI();

            var phase = stats.GetCurrentPhase();
            if (phase != null)
            {
                stats.ApplyPhaseEffects(phase);
                Debug.Log($"[Boss] Forced phase change to: {phase.phaseName}");
            }
        }
    }
}
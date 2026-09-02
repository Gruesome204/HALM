using UnityEngine;

public class BossEnemyBehaviour : EnemyBehaviour
{
    [Header("Boss Phases")]
    [SerializeField] private float phase2HealthThreshold = 0.5f;
    [SerializeField] private float phase3HealthThreshold = 0.3f;

    [Header("Boss UI")]
    [SerializeField] private BossBarUI bossBarUI;
    [SerializeField] private EnemyBaseBossStatsSO bossStats;

    public BossPhase CurrentPhase { get; private set; } = BossPhase.Phase1;
    private float currentAggressionMultiplier = 1f;
    private bool isInitialized = false; // ✅ Track initialization

    protected override void Awake()
    {
        AutoDetectComponents();
        base.Awake();

        if (bossBarUI == null)
            bossBarUI = FindObjectOfType<BossBarUI>();
    }

    private void Start()
    {
        SetupBossBar();
        isInitialized = true; // ✅ Mark as initialized

        // If using new SO system, override thresholds
        if (bossStats != null && bossStats.isMultiStageBoss && bossStats.phases.Length > 0)
        {
            // Update thresholds from SO
            if (bossStats.phases.Length >= 2)
            {
                phase2HealthThreshold = bossStats.phases[1].healthThreshold;

                if (bossStats.phases.Length >= 3)
                    phase3HealthThreshold = bossStats.phases[2].healthThreshold;
            }
        }

        // ✅ Force initial phase to Phase 1
        CurrentPhase = BossPhase.Phase1;
        Debug.Log($"{name} initialized with {CurrentPhase} at full health ({health?.CurrentHealth}/{health?.MaxHealth})");
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

        if (bossStats == null)
        {
            Debug.LogError("[BossEnemyBehaviour] BossStats not assigned!");
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
        // ✅ Skip phase check if not initialized
        if (!isInitialized) return;

        // Apply phase damage multiplier
        if (bossStats != null && bossStats.isMultiStageBoss)
        {
            var phaseConfig = bossStats.GetPhase((int)CurrentPhase - 1);
            if (phaseConfig != null && damageData.amount > 0)
            {
                damageData.amount *= phaseConfig.damageMultiplier;
            }
        }

        base.HandleDamaged(damageData, knockbackData);
        UpdatePhase();
    }

    private void HandleHealed(float healAmount)
    {
        // Boss bar updates via HandleHealthChanged event
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        // ✅ Skip phase check if not initialized
        if (!isInitialized) return;

        if (bossBarUI != null)
            bossBarUI.SetHealth(currentHealth, maxHealth);

        UpdatePhase();
    }

    private void UpdatePhase()
    {
        // ✅ Don't check phases if not initialized
        if (!isInitialized)
        {
            Debug.Log("Skipping phase check - not initialized yet");
            return;
        }

        if (stats == null || health == null)
        {
            Debug.LogWarning("Stats or Health is null, cannot update phase");
            return;
        }

        float hpPercent = health.CurrentHealth / health.MaxHealth;
        Debug.Log($"UpdatePhase called - HP: {hpPercent:P0}, Current Phase: {CurrentPhase}");

        // ✅ Don't trigger phase if at full health and already in Phase 1
        if (hpPercent >= 0.99f && CurrentPhase == BossPhase.Phase1)
        {
            Debug.Log("Boss at full health, staying in Phase 1");
            return;
        }

        // If using new SO system, check all phases
        if (bossStats != null && bossStats.isMultiStageBoss && bossStats.phases.Length > 0)
        {
            int newPhaseIndex = bossStats.GetPhaseForHealthPercent(hpPercent, CurrentPhase);
            BossPhase newPhase = (BossPhase)newPhaseIndex;

            // ✅ Only enter phase if it's different from current
            if (newPhase != CurrentPhase)
            {
                Debug.Log($"Phase change detected: {CurrentPhase} -> {newPhase} (HP: {hpPercent:P0})");
                EnterPhase(newPhase);
            }
            else
            {
                Debug.Log($"Staying in {CurrentPhase} (HP: {hpPercent:P0})");
            }

            return;
        }

        // Fallback: Legacy phase system
        if (hpPercent <= phase3HealthThreshold && CurrentPhase != BossPhase.Phase3)
            EnterPhase(BossPhase.Phase3);
        else if (hpPercent <= phase2HealthThreshold && CurrentPhase == BossPhase.Phase1)
            EnterPhase(BossPhase.Phase2);
    }

    private void EnterPhase(BossPhase newPhase)
    {
        // ✅ Don't enter phase if already in it
        if (CurrentPhase == newPhase)
        {
            Debug.Log($"Already in {newPhase}, skipping entry");
            return;
        }

        // ✅ Don't enter Phase 1 (it's the default)
        if (newPhase == BossPhase.Phase1)
        {
            Debug.Log("Cannot enter Phase 1 - this is the default phase");
            return;
        }

        CurrentPhase = newPhase;
        Debug.Log($"<color=cyan>★★★ {name} entered {newPhase} ★★★</color>");

        if (bossBarUI != null && bossStats != null)
        {
            int phaseIndex = (int)newPhase - 1;
            bossBarUI.ForcePhaseChange(phaseIndex);
        }

        // Apply phase effects
        var phaseEffects = bossStats?.GetPhase((int)newPhase - 1);
        if (phaseEffects != null)
        {
            ApplyPhaseEffects(phaseEffects);

            // Show announcement if any
            if (!string.IsNullOrEmpty(phaseEffects.phaseAnnouncement))
            {
                Debug.Log($"<color=red>⚠️ Boss Announcement:</color> {phaseEffects.phaseAnnouncement}");
            }
        }
        else
        {
            // Fallback to legacy phase logic
            switch (newPhase)
            {
                case BossPhase.Phase2:
                    OnPhase2();
                    break;
                case BossPhase.Phase3:
                    OnPhase3();
                    break;
            }
        }
    }

    private void ApplyPhaseEffects(PhaseConfig phaseConfig)
    {
        Debug.Log($"<color=yellow>Applying Phase {CurrentPhase} Effects:</color> " +
                  $"Aggression: {phaseConfig.aggressionMultiplier}, " +
                  $"Damage: {phaseConfig.damageMultiplier}, " +
                  $"Speed: {phaseConfig.speedMultiplier}, " +
                  $"Heal: {phaseConfig.healOnEnter}");

        // Apply aggression multiplier
        if (abilityBehaviour != null)
            abilityBehaviour.SetAggressionMultiplier(phaseConfig.aggressionMultiplier);

        // Apply speed multiplier
        if (movement != null)
        {
            Debug.Log($"Speed multiplier {phaseConfig.speedMultiplier} would be applied to movement");
        }

        // Apply damage multiplier (handled in HandleDamaged)
        currentAggressionMultiplier = phaseConfig.aggressionMultiplier;

        // Heal the boss
        if (health != null)
        {
            if (phaseConfig.healOnEnter > 0)
            {
                float healAmount = phaseConfig.healOnEnter;
                health.Heal(healAmount);
                Debug.Log($"<color=green>❤️ Boss healed for {healAmount} HP! Current HP: {health.CurrentHealth}/{health.MaxHealth}</color>");
            }
            else
            {
                Debug.Log($"No heal configured for this phase (healOnEnter = {phaseConfig.healOnEnter})");
            }
        }
        else
        {
            Debug.LogError("Health component is null! Cannot heal boss.");
        }

        // Spawn entry effects
        if (phaseConfig.phaseEntryEffects != null && phaseConfig.phaseEntryEffects.Length > 0)
        {
            foreach (var effect in phaseConfig.phaseEntryEffects)
            {
                if (effect != null)
                    Instantiate(effect, transform.position, Quaternion.identity);
            }
        }

        // Unlock new abilities
        if (abilityBehaviour != null && phaseConfig.unlockedAbilities != null && phaseConfig.unlockedAbilities.Length > 0)
        {
            foreach (var ability in phaseConfig.unlockedAbilities)
            {
                Debug.Log($"Would unlock ability: {ability.abilityName}");
            }
        }
    }

    private void OnPhase2()
    {
        if (abilityBehaviour != null)
            abilityBehaviour.SetAggressionMultiplier(1.5f);

        if (health != null)
        {
            health.Heal(200f);
            Debug.Log($"<color=green>❤️ Legacy Phase 2: Boss healed for 200 HP! Current HP: {health.CurrentHealth}/{health.MaxHealth}</color>");
        }
    }

    private void OnPhase3()
    {
        if (abilityBehaviour != null)
            abilityBehaviour.SetAggressionMultiplier(2f);

        if (health != null)
        {
            health.Heal(200f);
            Debug.Log($"<color=green>❤️ Legacy Phase 3: Boss healed for 200 HP! Current HP: {health.CurrentHealth}/{health.MaxHealth}</color>");
        }
    }

    // Public method to manually test healing
    public void TestHeal(float amount)
    {
        if (health != null)
        {
            health.Heal(amount);
            Debug.Log($"<color=green>Test Heal: {amount} HP applied. Current HP: {health.CurrentHealth}/{health.MaxHealth}</color>");
        }
    }
}
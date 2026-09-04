using UnityEngine;

public class BossEnemyBehaviour : EnemyBehaviour
{

    [Header("Boss UI")]
    [SerializeField] private BossBarUI bossBarUI;
    public BossPhase CurrentPhase { get; private set; } = BossPhase.Phase1;

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
        SetupBossBar();
    }

    /// <summary>
    /// Automatically detects and assigns required components if they're not set in the Inspector
    /// </summary>
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
        UpdatePhase();
    }

    private void HandleHealed(float healAmount)
    {
        // Boss bar updates via HandleHealthChanged event
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        if (bossBarUI != null)
            bossBarUI.SetHealth(currentHealth, maxHealth);

        UpdatePhase();
    }

    private void UpdatePhase()
    {
        if (stats == null || health == null) return;

        float hpPercent = health.CurrentHealth / health.MaxHealth;

    }

}
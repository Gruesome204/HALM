using UnityEngine;

public class BossEnemyBehaviour : EnemyBehaviour
{
    [Header("Boss Phases")]
    [SerializeField] private float phase2HealthThreshold = 0.5f;
    [SerializeField] private float phase3HealthThreshold = 0.3f;

    [Header("Boss UI")]
    [SerializeField] private BossBarUI bossBarUI;
    [SerializeField] private EnemyStats stats;
    public BossPhase CurrentPhase { get; private set; } = BossPhase.Phase1;
    private bool isInitialized = false; 

    protected override void Awake()
    {
        AutoDetectComponents();
        base.Awake();

        if (bossBarUI == null)
            bossBarUI = FindObjectOfType<BossBarUI>();

        if (stats == null)
            stats = FindObjectOfType<EnemyStats>();
    }

    private void Start()
    {
        SetupBossBar();
        isInitialized = true; 

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

        if (stats == null)
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

        bossBarUI.SetupBossBar(stats.baseBossStats);
        bossBarUI.SetHealth(health.CurrentHealth, health.MaxHealth);
        bossBarUI.ShowBossBar();
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

        base.HandleDamaged(damageData, knockbackData);
    }


    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {

        if (!isInitialized) return;

        if (bossBarUI != null)
            bossBarUI.SetHealth(currentHealth, maxHealth);

    }
}
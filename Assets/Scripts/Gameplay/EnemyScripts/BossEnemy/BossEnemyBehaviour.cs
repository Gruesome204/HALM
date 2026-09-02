using UnityEngine;

public class BossEnemyBehaviour : EnemyBehaviour
{
    [Header("Boss Phases")]
    [SerializeField] private float phase2HealthThreshold = 0.5f;
    [SerializeField] private float phase3HealthThreshold = 0.3f;

    [Header("Boss UI")]
    [SerializeField] private BossBarUI bossBarUI; // Drag your BossBarUI prefab here
    [SerializeField] private EnemyBaseBossStatsSO bossStats; // Assign in Inspector

    public BossPhase CurrentPhase { get; private set; } = BossPhase.Phase1;

    protected void Awake()
    {
        base.Awake();

        // Find boss bar if not assigned
        if (bossBarUI == null)
            bossBarUI = FindObjectOfType<BossBarUI>();
    }

    private void Start()
    {
        SetupBossBar();
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

        // Setup the boss bar
        bossBarUI.SetupBossBar(bossStats);

        // Set initial health
        float maxHealth = stats.maxHealth;
        float currentHealth = stats.currentHealth;
        bossBarUI.SetHealth(currentHealth, maxHealth);

        // Show the bar
        bossBarUI.ShowBossBar();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDamaged += HandleDamaged;
            health.OnHealed += HandleHealed; // Add this event
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDamaged -= HandleDamaged;
            health.OnHealed -= HandleHealed;
        }

        // Hide boss bar when boss dies/disabled
        if (bossBarUI != null)
            bossBarUI.HideBossBar();
    }

    protected override void HandleDamaged(DamageData damageData, KnockbackData knockbackData)
    {
        base.HandleDamaged(damageData, knockbackData);

        float hpPercent = stats.currentHealth / stats.maxHealth;

        // Update boss bar
        if (bossBarUI != null)
            bossBarUI.SetHealth(stats.currentHealth, stats.maxHealth);

        // Phase transitions
        if (hpPercent <= phase3HealthThreshold && CurrentPhase != BossPhase.Phase3)
            EnterPhase(BossPhase.Phase3);
        else if (hpPercent <= phase2HealthThreshold && CurrentPhase == BossPhase.Phase1)
            EnterPhase(BossPhase.Phase2);
    }

    private void HandleHealed(float healAmount)
    {
        // Update boss bar when healed
        if (bossBarUI != null)
            bossBarUI.SetHealth(stats.currentHealth, stats.maxHealth);
    }

    private void EnterPhase(BossPhase newPhase)
    {
        CurrentPhase = newPhase;
        Debug.Log($"{name} entered {newPhase}");

        // Update boss bar phase
        if (bossBarUI != null && bossStats != null)
        {
            int phaseIndex = (int)newPhase - 1;
            bossBarUI.ForcePhaseChange(phaseIndex);
        }

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

    private void OnPhase2()
    {
        abilityBehaviour.SetAggressionMultiplier(1.5f);
        var spider = GetComponent<EnemyStats>();
        spider.Heal(200f);
    }

    private void OnPhase3()
    {
        abilityBehaviour.SetAggressionMultiplier(2f);
        var spider = GetComponent<EnemyStats>();
        spider.Heal(200f);
    }

    private void ChangeBossColor(Color newColor)
    {
        var sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
            sprite.color = newColor;
    }
}
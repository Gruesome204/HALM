using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour, IDamagable, IParryable
{
    #region Serialized Fields
    [Header("References")]
    [SerializeField] private EnemyStats stats;
    [SerializeField] private EnemyMovement movement;

    [Header("UI")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private BossBarUI bossBarUIPrefab;

    [Header("Damage Flash")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float flashDuration = 0.3f;
    #endregion

    #region Private Fields
    private Canvas canvas;
    private MaterialPropertyBlock mpb;
    private Coroutine flashRoutine;
    private BossBarUI currentBossBarUI;
    private BossEnemyBehaviour bossBehaviour;
    #endregion

    #region Public Properties
    public bool IsInvulnerable { get; set; }
    public EnemyStats Stats => stats;
    #endregion

    #region Events
    public event Action<EnemyHealth, DamageData> OnDeath;
    public event Action<DamageData, KnockbackData> OnDamaged;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();
        spriteRenderer ??= GetComponentInChildren<SpriteRenderer>();
        mpb = new MaterialPropertyBlock();

        if (stats == null || stats.baseStats == null)
        {
            Debug.LogError($"{gameObject.name} has no stats assigned!");
            return;
        }

        // Initialize health BEFORE setting up UI
        stats.currentHealth = stats.maxHealth;
        Debug.Log($"[EnemyHealth] {gameObject.name}: Health initialized: {stats.currentHealth}/{stats.maxHealth}");

        canvas = FindObjectOfType<Canvas>();

        // Get boss behaviour if it exists
        bossBehaviour = GetComponent<BossEnemyBehaviour>();

        Debug.Log($"[EnemyHealth] {gameObject.name}: BossBehaviour found: {(bossBehaviour != null ? "Yes" : "No")}");

        SetupHealthUI();
    }
    #endregion

    #region Public Methods
    public void TakeDamage(DamageData damageData, KnockbackData knockbackData)
    {
        if (IsInvulnerable || stats == null) return;

        OnDamaged?.Invoke(damageData, knockbackData);

        float damage = CalculateTakenDamage(damageData);
        stats.currentHealth = Mathf.Clamp(stats.currentHealth - damage, 0f, stats.maxHealth);

        PlayDamageFlash();
        UpdateHealthUI();

        Debug.Log($"{gameObject.name} took {damage} {damageData.type} damage with {knockbackData.knockbackStrength} knockback.");

        if (stats.currentHealth <= 0)
            Die(damageData);
    }

    public void Die(DamageData damageData)
    {
        OnDeath?.Invoke(this, damageData);
    }

    public void OnDamageTaken(float amount) { }

    public bool IsAlive() => stats != null && stats.currentHealth > 0;

    public Transform GetTransform() => transform;

    public TargetType GetTargetType() => TargetType.Enemy;

    public void OnParried(GameObject source, float counterDamage)
    {
        TakeDamage(new DamageData
        {
            amount = counterDamage,
            type = DamageData.DamageType.Physical,
            source = source
        }, new KnockbackData { knockbackStrength = 0 });

        Debug.Log($"{gameObject.name} was parried and took {counterDamage} damage!");
    }

    public void UpdateHealthBar()
    {
        if (stats == null || stats.maxHealth <= 0) return;

        else if (healthBar != null)
        {
            float healthNormalized = stats.currentHealth / stats.maxHealth;
            healthBar.SetValueWithoutNotify(healthNormalized);
        }
    }
    #endregion

    #region Private Methods
    private void SetupHealthUI()
    {
        canvas ??= FindObjectOfType<Canvas>();

        Debug.Log($"[EnemyHealth] {gameObject.name}: Setting up health UI. BossBehaviour: {(bossBehaviour != null ? "Yes" : "No")}, BossBarPrefab: {(bossBarUIPrefab != null ? "Yes" : "No")}, Canvas: {(canvas != null ? "Yes" : "No")}");
        Debug.Log($"[EnemyHealth] {gameObject.name}: Current health: {stats.currentHealth}, Max health: {stats.maxHealth}");

        if (healthBar != null)
        {
            healthBar.minValue = 0f;
            healthBar.maxValue = 1f;
            healthBar.value = 1f;
        }

        // Setup boss bar if this is a boss
        if (bossBehaviour != null && bossBarUIPrefab != null && canvas != null)
        {
            Debug.Log($"[EnemyHealth] {gameObject.name}: Conditions met for boss bar setup. Creating boss bar UI...");
            SetupBossBarUI();
        }
        else
        {
            if (bossBehaviour == null)
                Debug.Log($"[EnemyHealth] {gameObject.name}: Skipping boss bar - No BossEnemyBehaviour found");
            else if (bossBarUIPrefab == null)
                Debug.Log($"[EnemyHealth] {gameObject.name}: Skipping boss bar - bossBarUIPrefab is null (assign in Inspector)");
            else if (canvas == null)
                Debug.Log($"[EnemyHealth] {gameObject.name}: Skipping boss bar - No Canvas found in scene");
        }
    }

    private void SetupBossBarUI()
    {
        Debug.Log($"[EnemyHealth] {gameObject.name}: Entering SetupBossBarUI()");

        // Check if we have boss stats
        if (stats.baseStats is EnemyBaseBossStatsSO bossStats)
        {
            Debug.Log($"[EnemyHealth] {gameObject.name}: Stats is EnemyBaseBossStatsSO - Name: {bossStats.bossBarName}");
            Debug.Log($"[EnemyHealth] {gameObject.name}: Health values - Current: {stats.currentHealth}, Max: {stats.maxHealth}");

            // Ensure health values are valid before setting up the bar
            if (stats.maxHealth <= 0)
            {
                Debug.LogError($"[EnemyHealth] {gameObject.name}: maxHealth is {stats.maxHealth}! This will cause issues. Check EnemyStats initialization.");
                return;
            }

            // Instantiate boss bar UI
            currentBossBarUI = Instantiate(bossBarUIPrefab, canvas.transform);
            Debug.Log($"[EnemyHealth] {gameObject.name}: BossBarUI instantiated");

            // Setup the boss bar with stats
            currentBossBarUI.SetupBossBar(bossStats);
            Debug.Log($"[EnemyHealth] {gameObject.name}: SetupBossBar called");

            // Set the name from boss stats
            if (!string.IsNullOrEmpty(bossStats.bossBarName))
            {
                Debug.Log($"[EnemyHealth] {gameObject.name}: Setting boss name to '{bossStats.bossBarName}'");
                currentBossBarUI.SetBossName(bossStats.bossBarName);
                Debug.Log($"[EnemyHealth] {gameObject.name}: Boss name set successfully!");
            }
            else
            {
                Debug.LogWarning($"[EnemyHealth] {gameObject.name}: bossStats.bossBarName is null or empty!");
            }

            // Initial health update with valid values
            float healthPercent = stats.currentHealth / stats.maxHealth;
            Debug.Log($"[EnemyHealth] {gameObject.name}: Setting health: {stats.currentHealth}/{stats.maxHealth} ({healthPercent:P0})");
            currentBossBarUI.SetHealth(stats.currentHealth, stats.maxHealth);
        }
        else
        {
            Debug.LogWarning($"[EnemyHealth] {gameObject.name}: Has BossEnemyBehaviour but stats.baseStats is not EnemyBaseBossStatsSO! Type: {stats.baseStats?.GetType().Name ?? "null"}");
        }
    }

    private float CalculateTakenDamage(DamageData data)
    {
        float dmg = data.amount;

        switch (data.type)
        {
            case DamageData.DamageType.Physical:
                dmg -= stats.currentArmor;
                break;
            case DamageData.DamageType.Magical:
                float resist = stats.currentMagicResistance / 100f;
                dmg *= (1 - Mathf.Clamp01(resist));
                break;
        }

        return Mathf.Max(dmg, 0f);
    }

    private void PlayDamageFlash()
    {
        if (spriteRenderer == null) return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator DamageFlashRoutine()
    {
        spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = Color.white;
        flashRoutine = null;
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            float healthNormalized = stats.currentHealth / stats.maxHealth;
            healthBar.SetValueWithoutNotify(healthNormalized);
        }

        // Update boss bar if it exists
        if (currentBossBarUI != null && stats != null)
        {
            if (stats.maxHealth > 0)
            {
                currentBossBarUI.SetHealth(stats.currentHealth, stats.maxHealth);
            }
            else
            {
                Debug.LogWarning($"[EnemyHealth] {gameObject.name}: Cannot update boss bar - maxHealth is {stats.maxHealth}");
            }
        }
    }
    #endregion
}
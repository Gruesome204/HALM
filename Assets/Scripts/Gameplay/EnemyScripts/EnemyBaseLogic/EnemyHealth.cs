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

        stats.currentHealth = stats.maxHealth;
        canvas = FindObjectOfType<Canvas>();

        // Get boss behaviour if it exists
        bossBehaviour = GetComponent<BossEnemyBehaviour>();

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

        if (healthBar != null)
        {
            healthBar.minValue = 0f;
            healthBar.maxValue = 1f;
            healthBar.value = 1f;
        }

        // Setup boss bar if this is a boss
        if (bossBehaviour != null && bossBarUIPrefab != null && canvas != null)
        {
            SetupBossBarUI();
        }
    }

    private void SetupBossBarUI()
    {
        // Check if we have boss stats
        if (stats.baseStats is EnemyBaseBossStatsSO bossStats)
        {
            // Instantiate boss bar UI
            currentBossBarUI = Instantiate(bossBarUIPrefab, canvas.transform);

            // Setup the boss bar with stats
            currentBossBarUI.SetupBossBar(bossStats);

            // Set the name from boss stats
            if (!string.IsNullOrEmpty(bossStats.bossBarName))
            {
                currentBossBarUI.SetBossName(bossStats.bossBarName);
            }

            // Initial health update
            float healthPercent = stats.currentHealth / stats.maxHealth;
            currentBossBarUI.SetHealth(stats.currentHealth, stats.maxHealth);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} has BossEnemyBehaviour but no EnemyBaseBossStatsSO assigned to stats.baseStats");
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
            currentBossBarUI.SetHealth(stats.currentHealth, stats.maxHealth);
        }
    }
    #endregion
}
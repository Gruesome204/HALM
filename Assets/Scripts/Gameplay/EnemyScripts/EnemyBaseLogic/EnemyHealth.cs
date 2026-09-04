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

    [Header("Damage Flash")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float flashDuration = 0.3f;
    #endregion

    #region Private Fields
    private MaterialPropertyBlock mpb;
    private Coroutine flashRoutine;
    private bool isInitialized;
    #endregion

    #region Public Properties
    public bool IsInvulnerable { get; set; }
    public EnemyStats Stats => stats;
    public float CurrentHealth => stats?.currentHealth ?? 0;
    public float MaxHealth => stats?.maxHealth ?? 0;
    public float HealthPercent => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0;
    #endregion

    #region Events
    public event Action<EnemyHealth, DamageData> OnDeath;
    public event Action<DamageData, KnockbackData> OnDamaged;
    public event Action<float> OnHealed;
    public event Action<float, float> OnHealthChanged; // current, max
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();
        spriteRenderer ??= GetComponentInChildren<SpriteRenderer>();
        mpb = new MaterialPropertyBlock();

        if (stats == null || stats.baseStats == null)
        {
            Debug.LogError($"[EnemyHealth] {gameObject.name} has no stats assigned!");
            return;
        }

        InitializeHealth();
    }
    #endregion

    #region Public Methods
    public void InitializeHealth()
    {
        if (isInitialized) return;

        // Set current health to max health (EnemyStats only calculated maxHealth)
        stats.currentHealth = stats.maxHealth;
        isInitialized = true;

       // Debug.Log($"[EnemyHealth] {gameObject.name}: Health: {stats.currentHealth}/{stats.maxHealth}");

        SetupHealthUI();
        OnHealthChanged?.Invoke(stats.currentHealth, stats.maxHealth);
    }

    public void TakeDamage(DamageData damageData, KnockbackData knockbackData)
    {
        if (IsInvulnerable || stats == null || !isInitialized) return;

        OnDamaged?.Invoke(damageData, knockbackData);

        float damage = CalculateTakenDamage(damageData);
        stats.currentHealth = Mathf.Clamp(stats.currentHealth - damage, 0f, stats.maxHealth);

        PlayDamageFlash();
        UpdateHealthUI();
        OnHealthChanged?.Invoke(stats.currentHealth, stats.maxHealth);

        //Debug.Log($"{gameObject.name} took {damage} {damageData.type} damage. Health: {stats.currentHealth}/{stats.maxHealth}");

        if (stats.currentHealth <= 0)
            Die(damageData);
    }

    public void Heal(float amount)
    {
        if (stats == null || stats.currentHealth <= 0 || !isInitialized) return;

        float oldHealth = stats.currentHealth;
        stats.currentHealth = Mathf.Min(stats.currentHealth + amount, stats.maxHealth);
        float actualHeal = stats.currentHealth - oldHealth;

        if (actualHeal > 0)
        {
            OnHealed?.Invoke(actualHeal);
            UpdateHealthUI();
            OnHealthChanged?.Invoke(stats.currentHealth, stats.maxHealth);

            Debug.Log($"{gameObject.name} healed for {actualHeal}. Health: {stats.currentHealth}/{stats.maxHealth}");
        }
    }

    public void HealPercent(float percent)
    {
        float healAmount = stats.maxHealth * Mathf.Clamp01(percent);
        Heal(healAmount);
    }

    public void SetHealth(float health)
    {
        if (stats == null) return;

        stats.currentHealth = Mathf.Clamp(health, 0f, stats.maxHealth);
        UpdateHealthUI();
        OnHealthChanged?.Invoke(stats.currentHealth, stats.maxHealth);
    }

    public void SetMaxHealth(float newMaxHealth)
    {
        if (stats == null) return;

        stats.maxHealth = Mathf.Max(0, newMaxHealth);
        stats.currentHealth = Mathf.Min(stats.currentHealth, stats.maxHealth);
        UpdateHealthUI();
        OnHealthChanged?.Invoke(stats.currentHealth, stats.maxHealth);
    }

    public void ResetHealth()
    {
        if (stats == null) return;

        stats.currentHealth = stats.maxHealth;
        UpdateHealthUI();
        OnHealthChanged?.Invoke(stats.currentHealth, stats.maxHealth);
    }

    public void Die(DamageData damageData)
    {
        OnDeath?.Invoke(this, damageData);
    }

    public void OnDamageTaken(float amount) { }

    public bool IsAlive() => stats != null && stats.currentHealth > 0 && isInitialized;

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
        UpdateHealthUI();
    }
    #endregion

    #region Private Methods
    private void SetupHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.minValue = 0f;
            healthBar.maxValue = 1f;
            healthBar.value = stats.maxHealth > 0 ? stats.currentHealth / stats.maxHealth : 1f;
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
        if (healthBar != null && stats != null && stats.maxHealth > 0)
        {
            float healthNormalized = stats.currentHealth / stats.maxHealth;
            healthBar.SetValueWithoutNotify(healthNormalized);
        }
    }
    #endregion
}
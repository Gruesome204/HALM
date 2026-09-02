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
    #endregion

    #region Public Properties
    public bool IsInvulnerable { get; set; }
    public EnemyStats Stats => stats;
    #endregion

    #region Events
    public event Action<EnemyHealth, DamageData> OnDeath;
    public event Action<DamageData, KnockbackData> OnDamaged;
    public event System.Action<float> OnHealed;
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

        // Health is already initialized in EnemyStats.Awake()
        // But ensure it's set correctly
        if (stats.currentHealth <= 0)
        {
            stats.currentHealth = stats.maxHealth;
        }

        Debug.Log($"[EnemyHealth] {gameObject.name}: Health: {stats.currentHealth}/{stats.maxHealth}");

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

        Debug.Log($"{gameObject.name} took {damage} {damageData.type} damage. Health: {stats.currentHealth}/{stats.maxHealth}");

        if (stats.currentHealth <= 0)
            Die(damageData);
    }

    public void Heal(float amount)
    {
        if (stats == null || stats.currentHealth <= 0) return;

        float oldHealth = stats.currentHealth;
        stats.currentHealth = Mathf.Min(stats.currentHealth + amount, stats.maxHealth);
        float actualHeal = stats.currentHealth - oldHealth;

        if (actualHeal > 0)
        {
            OnHealed?.Invoke(actualHeal);
            UpdateHealthUI();

            Debug.Log($"{gameObject.name} healed for {actualHeal}. Health: {stats.currentHealth}/{stats.maxHealth}");
        }
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
        UpdateHealthUI();
    }
    #endregion

    #region Private Methods
    private void SetupHealthUI()
    {
        // Setup regular health bar
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
        // Update regular health bar
        if (healthBar != null && stats != null && stats.maxHealth > 0)
        {
            float healthNormalized = stats.currentHealth / stats.maxHealth;
            healthBar.SetValueWithoutNotify(healthNormalized);
        }
    }
    #endregion
}
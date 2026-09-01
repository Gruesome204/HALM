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
    [SerializeField] private BossBarUI bossBarUIInstance;

    [Header("Damage Flash")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float flashDuration = 0.3f;
    #endregion

    #region Private Fields
    private Canvas canvas;
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
        if (bossBarUIInstance != null)
            Destroy(bossBarUIInstance.gameObject);
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

    public void UpdatePhaseName(string phaseName)
    {
        if (bossBarUIInstance != null)
            bossBarUIInstance.SetBossName(phaseName);
    }

    public void UpdateHealthBar()
    {
        if (stats == null || stats.maxHealth <= 0) return;

        float healthNormalized = stats.currentHealth / stats.maxHealth;

        if (bossBarUIInstance != null)
        {
            bossBarUIInstance.SetHealth(stats.currentHealth);
            if (!string.IsNullOrEmpty(stats.baseStats.baseName))
                bossBarUIInstance.SetBossName(stats.baseStats.baseName);
        }
        else if (healthBar != null)
        {
            healthBar.SetValueWithoutNotify(healthNormalized);
        }
    }
    #endregion

    #region Private Methods
    private void SetupHealthUI()
    {
        canvas ??= FindObjectOfType<Canvas>();

        if (stats.baseStats.enemyType == EnemyType.Boss)
        {
            bossBarUIInstance ??= GetComponentInChildren<BossBarUI>();

            if (bossBarUIInstance == null && bossBarUIPrefab != null)
                bossBarUIInstance = Instantiate(bossBarUIPrefab, canvas.transform);

            if (bossBarUIInstance != null)
                bossBarUIInstance.SetupBossBar(stats.baseStats);
            else
                Debug.LogWarning($"{name} has no BossBarUI assigned or found!");
        }

        if (healthBar != null)
        {
            healthBar.minValue = 0f;
            healthBar.maxValue = 1f;
            healthBar.value = 1f;
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
        if (bossBarUIInstance != null)
            bossBarUIInstance.SetHealth(stats.currentHealth);
        else if (healthBar != null)
            healthBar.SetValueWithoutNotify(stats.currentHealth / stats.maxHealth);
    }
    #endregion
}
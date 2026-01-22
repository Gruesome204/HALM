using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class EnemyHealth : MonoBehaviour, IDamagable, IParryable
{
    [Header("Stats & Movement")]
    public EnemyStats stats;
    public EnemyMovement movement;

    [Header("UI")]
    public Slider healthBar;                // Normal health bar
    public BossBarUI bossBarUIPrefab;       // Optional prefab for runtime instantiation
    [SerializeField] private BossBarUI bossBarUIInstance; // Now assignable in inspector
    private Canvas canvas;

    [Header("Damage Flash")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float flashDuration = 0.3f;

    private MaterialPropertyBlock mpb;
    private Coroutine flashRoutine;

    public event Action<EnemyHealth, DamageData> OnDeath;
    public event Action<DamageData, KnockbackData> OnDamaged;

    public bool IsInvulnerable { get; set; }


    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();

        spriteRenderer ??= GetComponentInChildren<SpriteRenderer>();

        mpb = new MaterialPropertyBlock();

        // Make sure stats are valid
        if (stats == null || stats.baseStats == null)
        {
            Debug.LogError($"{gameObject.name} has no stats assigned!");
            return;
        }

        stats.currentHealth = stats.maxHealth;

        canvas = FindObjectOfType<Canvas>();

        SetupHealthUI();
    }

    /// <summary>
    /// Sets up the appropriate health UI depending on enemy type.
    /// </summary>
    private void SetupHealthUI()
    {

        canvas ??= FindObjectOfType<Canvas>();

        if (stats.baseStats.enemyType == EnemyType.Boss)
        {
            // If no instance assigned in inspector, try to find one in children
            if (bossBarUIInstance == null)
            {
                bossBarUIInstance = GetComponentInChildren<BossBarUI>();
            }

            // If still null, instantiate prefab
            if (bossBarUIInstance == null && bossBarUIPrefab != null)
            {
                bossBarUIInstance = Instantiate(bossBarUIPrefab, canvas.transform);
            }

            if (bossBarUIInstance != null)
            {
                bossBarUIInstance.SetupBossBar(stats.baseStats);
            }
            else
            {
                Debug.LogWarning($"{name} has no BossBarUI assigned or found!");
            }
        }

        // Update health dynamically
        if (bossBarUIInstance != null)
        { 
            healthBar.minValue = 0f;
            healthBar.maxValue = 1f;
            healthBar.value = 1f;
        }
        else if (healthBar != null)
        {
            healthBar.minValue = 0f;
            healthBar.maxValue = 1f;
            healthBar.value = 1f;
        }
    }

    public void TakeDamage(DamageData damageData, KnockbackData knockbackData)
    {
        if (IsInvulnerable || stats == null) return;

        OnDamaged?.Invoke(damageData, knockbackData);

        float damage = CalculateTakenDamage(damageData);
        stats.currentHealth -= damage;
        stats.currentHealth = Mathf.Clamp(stats.currentHealth, 0f, stats.maxHealth);

        PlayDamageFlash();

        // Update the correct health UI
        if (bossBarUIInstance != null)
            bossBarUIInstance.SetHealth(stats.currentHealth);
        else if (healthBar != null)
        {
            healthBar.SetValueWithoutNotify(
                stats.currentHealth / stats.maxHealth
            );
        }

        Debug.Log($"{gameObject.name} took {damage} {damageData.type} damage with {knockbackData.knockbackStrength} knockback.");

        if (stats.currentHealth <= 0)
            Die(damageData);
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

    public void Die(DamageData damageData)
    {
        // Notify listeners
        OnDeath?.Invoke(this, damageData);
        if (bossBarUIInstance != null)
            Destroy(bossBarUIInstance.gameObject);
    }

    public void OnDamageTaken(float amount)
    {
    }

    private void PlayDamageFlash()
    {
        if (spriteRenderer == null)
            return;

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
        
    public bool IsAlive()
    {
        return stats.currentHealth > 0;
    }

    public Transform GetTransform()
    {
        return transform;
    }
        
    public TargetType GetTargetType()
    {
        return TargetType.Enemy;
    }
    public void OnParried(GameObject source, float counterDamage)
    {
        // Take counter damage
        TakeDamage(new DamageData
        {
            amount = counterDamage,
            type = DamageData.DamageType.Physical,
            source = source
        }, new KnockbackData { knockbackStrength = 0 });

        // Optional: play parry hit effect
        Debug.Log($"{gameObject.name} was parried and took {counterDamage} damage!");
    }
    public void UpdatePhaseName(string phaseName)
    {
        if (bossBarUIInstance != null)
            bossBarUIInstance.SetBossName(phaseName);
    }
    public void UpdateHealthBar()
    {
        float healthNormalized = 0f;

        if (stats == null || stats.maxHealth <= 0)
            return;

        healthNormalized = stats.currentHealth / stats.maxHealth;

        if (bossBarUIInstance != null)
        {
            bossBarUIInstance.SetHealth(stats.currentHealth);

            if (!string.IsNullOrEmpty(stats.baseStats.baseName))
            {
                bossBarUIInstance.SetBossName(stats.baseStats.baseName);
            }
        }
        else if (healthBar != null)
        {
            healthBar.SetValueWithoutNotify(healthNormalized);
        }
    }
}

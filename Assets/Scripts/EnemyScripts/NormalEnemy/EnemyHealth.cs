using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class EnemyHealth : MonoBehaviour, IDamagable
{
    [Header("Stats & Movement")]
    public EnemyStats stats;
    public EnemyMovement movement;

    [Header("UI")]
    public Slider healthBar;                // Normal health bar
    public BossBarUI bossBarUIPrefab;       // Boss health bar prefab
    private BossBarUI bossBarUIInstance;    // Runtime instance of boss bar
    private Canvas canvas;

    public event Action<EnemyHealth, DamageData> OnDeath;
    public event Action<DamageData, KnockbackData> OnDamaged;

    public bool IsInvulnerable { get; set; }


    private void Start()
    {
        movement = GetComponent<EnemyMovement>();

        // Make sure stats are valid
        if (stats == null || stats.baseStats == null)
        {
            Debug.LogError($"{gameObject.name} has no stats assigned!");
            return;
        }

        stats.currentHealth = stats.currentMaxHealth;

        canvas = FindObjectOfType<Canvas>();

        SetupHealthUI();
    }

    /// <summary>
    /// Sets up the appropriate health UI depending on enemy type.
    /// </summary>
    private void SetupHealthUI()
    {
        if (stats.baseStats.enemyType == EnemyType.Boss && bossBarUIPrefab != null)
        {
            bossBarUIInstance = Instantiate(bossBarUIPrefab, canvas.transform);
            bossBarUIInstance.target = transform;
            bossBarUIInstance.SetupBossBar(stats.baseStats);
        }

        // Update health dynamically
        if (bossBarUIInstance != null)
            bossBarUIInstance.SetHealth(stats.currentHealth);

        else if (healthBar != null)
        {
            // Standard health bar for normal enemies
            healthBar.minValue = 0;
            healthBar.maxValue = stats.currentMaxHealth;
            healthBar.value = stats.currentHealth;
        }
    }

    public void TakeDamage(DamageData damageData, KnockbackData knockbackData)
    {
        if (IsInvulnerable || stats == null) return;

        OnDamaged?.Invoke(damageData, knockbackData);

        float damage = CalculateTakenDamage(damageData);
        stats.currentHealth -= damage;
        stats.currentHealth = Mathf.Clamp(stats.currentHealth, 0f, stats.currentMaxHealth);

        // Update the correct health UI
        if (bossBarUIInstance != null)
            bossBarUIInstance.SetHealth(stats.currentHealth);
        else if (healthBar != null)
            healthBar.SetValueWithoutNotify(stats.currentHealth);

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
}

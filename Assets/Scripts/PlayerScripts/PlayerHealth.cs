using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamagable, IInvulnerable
{
    [Header("References")]
    public PlayerStats stats;
    public Slider healthBar;

    //Event does nothing yet
    public event Action<PlayerHealth, DamageData> OnDeath;
    public event Action<float> OnHealthChanged;
    public event Action<DamageData, KnockbackData> OnDamageTakenEvent;
    public event Action OnParrySuccess;
    public bool IsInvulnerable { get; set; }
    private float invulnTimer;

    public SpriteRenderer sr { get; set; }

    private void Awake()
    {
        if (stats == null)
            stats = GetComponent<PlayerStats>();

        if (sr == null)
            sr = GetComponentInChildren<SpriteRenderer>();

        if (sr == null)
            Debug.LogWarning("No SpriteRenderer found on player or its children!");
    }

    private void Start()
    {
        UpdateHealthBar();
    }

    private void Update()
    {
        if (!IsInvulnerable) return;
        invulnTimer -= Time.deltaTime;
        if (invulnTimer <= 0f)
            IsInvulnerable = false;

        if (IsInvulnerable)
            sr.enabled = Mathf.FloorToInt(Time.time * 15) % 2 == 0;
        else
            sr.enabled = true;
    }

    public void TakeDamage(DamageData damageData, KnockbackData knockbackData)
    {
        if (IsInvulnerable || stats == null) return;

        // Fire parry/damage callbacks
        OnDamageTakenEvent?.Invoke(damageData, knockbackData);

        float damage = CalculateTakenDamage(damageData);
        ApplyDamage(damage, damageData);
    }
    public void Heal(float amount)
    {
        if (!IsAlive()) return;

        stats.currentHealth = Mathf.Clamp(
            stats.currentHealth + amount,
            0,
            stats.currentMaxHealth
        );

        UpdateHealthBar();
        OnHealthChanged?.Invoke(stats.currentHealth);
    }
    private void ApplyDamage(float damage, DamageData damageData)
    {
        stats.currentHealth = Mathf.Clamp(
            stats.currentHealth - damage,
            0,
            stats.currentMaxHealth
        );

        UpdateHealthBar();
        OnHealthChanged?.Invoke(stats.currentHealth);

        Debug.Log($"Player took {damage} {damageData.type} damage. Remaining: {stats.currentHealth}");

        if (stats.currentHealth <= 0)
            Die(damageData);
    }

    public void Die(DamageData damageData)
    {
        if (IsInvulnerable) return; // safety check
        OnDeath?.Invoke(this, damageData);

        Debug.Log("Player died.");
        // Add more logic later (disable movement, play animation, etc.)
    }
    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.SetValueWithoutNotify(stats.currentHealth / stats.currentMaxHealth);
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
                float resistPercent = Mathf.Clamp01(stats.currentMagicResistance / 100f);
                dmg *= (1f - resistPercent);
                break;
        }

        return Mathf.Max(dmg, 0f);
    }

    //Show UI effects upon taking Damage
    public void OnDamageTaken(float amount)
    {
  
    }

    public bool IsAlive()
    {
        return stats != null && stats.currentHealth > 0;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public TargetType GetTargetType()
    {
        return TargetType.Player;
    }
    public void CallParrySuccess()
    {
        OnParrySuccess?.Invoke();
    }

    public void SetInvulnerable(float duration)
    {
        if (duration <= 0f) return;

        IsInvulnerable = true;
        invulnTimer = duration;
    }
}

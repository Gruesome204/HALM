using System;
using UnityEngine;
using UnityEngine.UI;
public class EnemyHealth : MonoBehaviour, IDamagable
{
    public EnemyStats stats;
    public EnemyMovement movement;
    public Slider healthBar;

    public event Action<EnemyHealth, DamageData> OnDeath;
    private void Start()
    {
        movement = GetComponent<EnemyMovement>();

        if (healthBar != null)
        {
            healthBar.minValue = 0;
            healthBar.maxValue = stats.currentMaxHealth;
            healthBar.value = stats.currentHealth;
        }
    }
    public bool IsInvulnerable { get; set; }

    public void TakeDamage(DamageData damageData, KnockbackData knockbackData)
    {
        movement.SetTargetPlayer();
        if (IsInvulnerable || stats == null) return;

        float damage = CalculateTakenDamage(damageData);
        stats.currentHealth -= damage;
        healthBar?.SetValueWithoutNotify(stats.currentHealth);
        Debug.Log($"{gameObject.name} took {damage} {damageData.type} damage.");
        //Debug.Log($"Health after damage: {stats.currentHealth}");
        if (stats.currentHealth <= 0)
        {
            Die(damageData);
        }
    }

    public void Die(DamageData damageData)
    {
        // Notify listeners
        OnDeath?.Invoke(this, damageData);
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

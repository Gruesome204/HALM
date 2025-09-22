using System;
using UnityEngine;
using UnityEngine.UI;

public class TurretHealth : MonoBehaviour, IDamagable
{
    TurretStats stats;
    [Header("UI")]
    public Slider healthBar;

    public bool IsInvulnerable { get; set; }
    public event Action<TurretHealth, DamageData> OnDeath;

    public void TakeDamage(DamageData damageData, KnockbackData knockbackData)
    {

        if (IsInvulnerable || stats == null) return;

        stats.currentHealth -= damageData.amount;
        healthBar?.SetValueWithoutNotify(stats.currentHealth / stats.currentMaxHealth);
        Debug.Log($"{gameObject.name} took {damageData.amount} {damageData.type} damage.");
        Debug.Log($"Health after damage: {stats.currentHealth}");
        if (stats.currentHealth <= 0)
        {
            Die(damageData);
        }
    }

    public void OnDamageTaken(float amount)
    {
        throw new NotImplementedException();
    }

    public void Die(DamageData damageData)
    {
        throw new NotImplementedException();
    }
}

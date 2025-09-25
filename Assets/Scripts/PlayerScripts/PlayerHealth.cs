using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamagable
{
    public PlayerStats stats;
    public Slider healthBar;
    public event Action<PlayerHealth, DamageData> OnDeath;

    public bool IsInvulnerable { get; set; }

    public void TakeDamage(DamageData damageData, KnockbackData knockbackData)
    {
        if (IsInvulnerable || stats == null) return;

        float damage = CalculateTakenDamage(damageData);
        stats.currentHealth -= damage;

        healthBar?.SetValueWithoutNotify(stats.currentHealth / stats.currentMaxHealth);

        Debug.Log($"Player took {damage} {damageData.type} damage. Remaining: {stats.currentHealth}");

        if (stats.currentHealth <= 0)
        {
            Die(damageData);
        }
    }

    public void Die(DamageData damageData)
    {
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
        throw new NotImplementedException();
    }

    public bool IsAlive()
    {
        throw new NotImplementedException();
    }

    public Transform GetTransform()
    {
        throw new NotImplementedException();
    }

    public TargetType GetTargetType()
    {
        throw new NotImplementedException();
    }
}

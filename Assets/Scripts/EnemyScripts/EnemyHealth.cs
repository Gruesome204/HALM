using System;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour, IDamagable
{
    public EnemyStats stats;
    public Slider healthBar;
    public event Action OnDeath;

    public bool IsInvulnerable { get; set; }

    public void TakeDamage(DamageData damageData, KnockbackData knockbackData)
    {
        if (IsInvulnerable || stats == null) return;

        float damage = CalculateTakenDamage(damageData);
        stats.currentHealth -= damage;
        healthBar?.SetValueWithoutNotify(stats.currentHealth / stats.currentMaxHealth);
        Debug.Log($"{gameObject.name} took {damage} {damageData.type} damage.");

        if (stats.currentHealth <= 0)
        {
            OnDeath?.Invoke();
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

    public void OnDamageTaken(float amount)
    {
        throw new System.NotImplementedException();
    }
}

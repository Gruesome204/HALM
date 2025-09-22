using System;
using UnityEngine;
using UnityEngine.UI;

public class TurretHealth : MonoBehaviour, IDamagable
{

    [Header("UI")]
    public Slider healthBar;

    public bool IsInvulnerable { get; set; }
    public event Action<TurretHealth, DamageData> OnDeath;

    private void Awake()
    {
        currentHealth = maxHealth;
        if (healthBar != null)
            healthBar.value = 1f;
    }

    public void TakeDamage(DamageData damageData, KnockbackData knockbackData)
    {
        if (IsInvulnerable) return;

        float damage = damageData.amount;
        currentHealth -= damage;

        if (healthBar != null)
            healthBar.SetValueWithoutNotify(currentHealth / maxHealth);

        Debug.Log($"{gameObject.name} took {damage} damage. Health left: {currentHealth}");

        if (currentHealth <= 0)
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

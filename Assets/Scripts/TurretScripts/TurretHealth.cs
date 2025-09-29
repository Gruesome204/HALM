using System;
using UnityEngine;
using UnityEngine.UI;

public class TurretHealth : MonoBehaviour, IDamagable
{
    [Header("Stats")]
    [SerializeField] private TurretStats stats;

    [Header("UI")]
    private Slider healthBar;             // Will be assigned when prefab is spawned
    private HealthBarFollow healthBarFollow;
    public bool IsInvulnerable { get; set; }
    public event Action<TurretHealth, DamageData> OnDeath;
    private void Start()
    {
        if (stats != null)
        {
            stats = GetComponent<TurretStats>();
        }
    }


    public void TakeDamage(DamageData damageData, KnockbackData knockbackData)
    {

        if (IsInvulnerable || stats == null) return;

        stats.currentHealth -= damageData.amount;
        stats.currentHealth = Mathf.Clamp(stats.currentHealth, 0, stats.currentMaxHealth);
        UpdateHealthBar();

        Debug.Log($"{gameObject.name} took {damageData.amount} {damageData.type} damage. Remaining HP: {stats.currentHealth}");

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
        Debug.Log($"{gameObject.name} has been destroyed!");
        OnDeath?.Invoke(this, damageData);

        if (healthBarFollow != null)
            Destroy(healthBarFollow.gameObject); // clean up UI

        Destroy(gameObject); ;
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

    public float GetCurrentHealth() => stats.currentHealth;
    public float GetMaxHealth() => stats.currentMaxHealth;

    public void AttachHealthBar(GameObject healthBarPrefab, Vector3 offset)
    {
        if (healthBarPrefab == null) return;

        GameObject bar = Instantiate(healthBarPrefab);
        healthBar = bar.GetComponentInChildren<Slider>();
        healthBarFollow = bar.GetComponent<HealthBarFollow>();

        if (healthBar != null)
        {
            healthBar.minValue = 0;
            healthBar.maxValue = stats.currentMaxHealth;
            healthBar.value = stats.currentHealth;
        }

        if (healthBarFollow != null)
        {
            healthBarFollow.target = this.transform;
            healthBarFollow.offset = offset;
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
            healthBar.value = stats.currentHealth;
    }
}

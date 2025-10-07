using System;
using UnityEngine;
using UnityEngine.UI;

public class TurretHealth : MonoBehaviour, IDamagable
{
    [Header("Stats")]
    [SerializeField] private TurretStats stats;

    [Header("UI")]
    private Slider healthBar;             // Will be assigned when prefab is spawned
    public HealthBarFollow healthBarFollow;
    public bool IsInvulnerable { get; set; }
    public event Action<TurretHealth, DamageData> OnDeath;
    private void Start()
    {
        if (stats == null)
            stats = GetComponent<TurretStats>();
    }
    public void Initialize(TurretBlueprint turretBlueprint)
    {
        if (turretBlueprint == null || stats == null) return;

        stats.currentHealth = stats.currentMaxHealth;

        UpdateHealthBar();
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
        Debug.Log($"{gameObject.name} was hit for {amount} damage");
    }

    public void Die(DamageData damageData)
    {
        Debug.Log($"{gameObject.name} has been destroyed!");
        OnDeath?.Invoke(this, damageData);

        if (healthBarFollow != null)
            Destroy(healthBarFollow.gameObject); // clean up UI
        TurretPlacementController.Instance?.UnregisterTurret(this);

        Destroy(gameObject); ;
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
        return TargetType.Turret;
    }

    public float GetCurrentHealth() => stats.currentHealth;
    public float GetMaxHealth() => stats.currentMaxHealth;

    public void AttachHealthBar(GameObject healthBarPrefab, Vector3 offset)
    {
        if (healthBarPrefab == null) return;

        GameObject bar = Instantiate(healthBarPrefab, this.transform);
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

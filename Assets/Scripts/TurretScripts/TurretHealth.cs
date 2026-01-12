    using System;
using UnityEngine;
using UnityEngine.UI;
using static DamageData;

public class TurretHealth : MonoBehaviour, IDamagable
{
    [Header("Stats")]
    [SerializeField] private TurretStats stats;

    [Header("UI")]
    private Slider healthBar;   // Will be assigned when prefab is spawned
    public HealthBarFollow healthBarFollow;


    [Header("Settings")]
    [Tooltip("Cooldown between collisions to avoid rapid repeated damage.")]
    [SerializeField] private float collisionDamageCooldown = 1f;


    private float lastCollisionTime;
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

        stats.currentMaxHealth = turretBlueprint.baseHealth + TurretGlobalModifierManager.Instance.globalHealthMultiplier;
        stats.currentHealth = stats.currentMaxHealth;

        RecalculateStatsAfterModifiers();
        UpdateHealthBar();
    }

    public void RecalculateStatsAfterModifiers()
    {
        float healthPercent = stats.currentHealth / stats.currentMaxHealth;

        stats.currentMaxHealth = stats.GetBlueprint().baseHealth + TurretGlobalModifierManager.Instance.globalHealthMultiplier;
        stats.currentHealth = Mathf.Max(1, stats.currentMaxHealth * healthPercent);
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
     
    }

    public void Die(DamageData damageData)
    {
        Debug.Log($"{gameObject.name} has been destroyed!");
        OnDeath?.Invoke(this, damageData);

        if (healthBarFollow != null)
        Destroy(healthBarFollow.gameObject); // clean up UI

        // Unregister turret
        TurretPlacementController.Instance?.UnregisterTurret(this);

        var placable = GetComponentInParent<PlacableObject>();
        var behaviour = GetComponentInParent<TurretBehaviour>();

        //Removes the tower from the grid 
        if (placable != null && behaviour != null)
        {
            GridManager.Instance.RemoveObject(
                transform.parent.gameObject,
                placable.currentGridCoordinates,
                behaviour.turretBlueprint.sizeInCells);
            Debug.Log("Remove turret from grid");
        }
        
        //Removes the visual etc..
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


    private void OnCollisionEnter2D(Collision2D collision)
    {
       // Debug.Log("Test Collision");
        if (Time.time < lastCollisionTime + collisionDamageCooldown) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyStats enemyStats = collision.gameObject.GetComponentInChildren<EnemyStats>();
            if (enemyStats != null)
            {
                var damageData = new DamageData
                {
                    amount = stats.currentAttackDamage,
                    type = DamageType.Physical,
                    source = collision.gameObject
                };

                TakeDamage(damageData, new KnockbackData());
                lastCollisionTime = Time.time;

                Debug.Log($"{gameObject.name} collided with {collision.gameObject.name} and took {stats.currentAttackDamage} damage.");
            }
        }
    }
}

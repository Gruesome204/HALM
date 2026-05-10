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
    [SerializeField] private float damageTickRate = 0.5f; // damage every 0.5s
    private float lastDamageTime;

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
        stats.currentMaxHealth = turretBlueprint.baseHealth * (1f + TurretGlobalModifierManager.Instance.globalHealthMultiplier);
        stats.currentHealth = stats.currentMaxHealth;

        RecalculateStatsAfterModifiers();
        UpdateHealthBar();
    }

    public void RecalculateStatsAfterModifiers()
    {
        float healthPercent = stats.currentHealth / stats.currentMaxHealth;

        stats.currentMaxHealth = stats.GetBlueprint().baseHealth * (1f + TurretGlobalModifierManager.Instance.globalHealthMultiplier);
        stats.currentHealth = Mathf.Max(1, stats.currentMaxHealth * healthPercent);
    }

    public void TakeDamage(DamageData damageData, KnockbackData knockbackData)
    {

        Debug.Log($"Attack Damage Received = {damageData.amount}");
        if (IsInvulnerable || stats == null) return;
        stats.currentHealth -= damageData.amount;
        //stats.currentHealth = Mathf.Clamp(stats.currentHealth, 0, stats.currentMaxHealth);
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


    private void OnCollisionStay2D(Collision2D collision)
    {
        if (Time.time < lastDamageTime + damageTickRate) return;

        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Boss"))
        {
            EnemyStats enemyStats = collision.gameObject.GetComponentInChildren<EnemyStats>();
            if (enemyStats != null)
            {
                // Base damage
                float damageAmount = enemyStats.currentDamage;

                // Check if the enemy is a boss
                if (collision.gameObject.TryGetComponent(out BossEnemyBehaviour boss))
                {
                    damageAmount *= 2f; // Double damage for bosses
                }
                var damageData = new DamageData
                {
                    amount = damageAmount,
                    type = DamageData.DamageType.Physical,
                    source = collision.gameObject
                };


                TakeDamage(damageData, new KnockbackData());
                lastDamageTime = Time.time;

                Debug.Log($"{gameObject.name} collided with {collision.gameObject.name} and took {stats.currentAttackDamage} damage.");
            }
        }
    }
}

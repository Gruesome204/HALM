    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static DamageData;

public class EnemyBehaviour : MonoBehaviour, IDamagable
{
    [SerializeField]private EnemyBaseStats baseStats;
    public Slider healthBarSlider;

    public GameObject target;
    private Rigidbody2D rb;

    private bool isKnockedBack = false;

    public bool IsInvulnerable { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }


    [Header("Info")]
    public string enemyName;
    public string enemyDescription;

    [Header("Level")]
    public int currentLevel = 1;

    [Header("Movement")]
    public float currentMovementSpeed;

    [Header("Defensive Stats")]
    public float currentMaxHealth;
    public float currentHealth;
    public float currentArmor;
    public float currentMagicResistance;

    public float currentKnockbackReduction;
    public float currentKnockbackForce = 10f;
    public float currentKnockbackDuration = 0.5f;

    [Header("Offensive Stats")]
    public float currentDamage;
    public float currentAttackSpeed;
    public float currentCritChance;
    public float currentCritHitMultiplier;
    public float currentAttackRange;

    [Header("Detection")]
    public float currentDetectionRange;
    public float currentVisionRange;
    public float currenthearingRange;
    public float pursueRange;

    [Header("Progression")]

    public float experienceYield;

    [Header("Scaling Factors")]
    public float healthScaleFactor;
    public float damageScaleFactor;
    public float speedScaleFactor;
    public float armorScaleFactor;

    private void Awake()
    {
        InitalizeEnemyStats();

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found on " + gameObject.name + ". Enemy movement will not work.");
        }
        if (target == null)
        {
            // You might want to implement a way to find the target (e.g., the player) here.
            // For example, by tag:
            // target = GameObject.FindGameObjectWithTag("Player")?.transform;
            Debug.LogWarning("Target not assigned for " + gameObject.name + ". Enemy will not move.");
        }
    }

    private void FixedUpdate() // Using FixedUpdate for physics-based movement
    {
        if (!isKnockedBack) // ONLY move if not currently knocked back
        {
            MoveGlobal(); // Or MoveInRange(), whichever is active
        }

    }

    public void MoveInRange()
    {
        if (target != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);

            if (distanceToTarget <= pursueRange)
            {
                // Calculate the direction to the target
                Vector2 direction = (target.transform.position - transform.position).normalized;

                // Move the enemy towards the target
                rb.linearVelocity = direction * currentMovementSpeed;
            }
            else
            {
                // Stop moving if the target is out of pursue range
                rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            // If no target is assigned, stop moving
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void MoveGlobal()
    {
        if (target != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);

            // Calculate the direction to the target
            Vector2 direction = (target.transform.position - transform.position).normalized;

            // Move the enemy towards the target
            rb.linearVelocity = direction * currentMovementSpeed;
        }
        else
        {
            // Stop moving if the target is out of pursue range
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void InitalizeEnemyStats()
    {
        if (baseStats != null)
        {
            // Initialize current stats with base stats
            currentMaxHealth = baseStats.baseHealth * GetLevelScalingFactor(healthScaleFactor);
            currentHealth = currentMaxHealth; // Set current health to max health initially
            currentDamage = baseStats.baseDamage * GetLevelScalingFactor(damageScaleFactor);
            currentArmor = baseStats.baseArmor * GetLevelScalingFactor(armorScaleFactor);
            currentMovementSpeed = baseStats.baseMovementSpeed * GetLevelScalingFactor(speedScaleFactor);
            currentKnockbackReduction = Mathf.Clamp01(currentKnockbackReduction);
        }
        else
        {
            Debug.LogError("EnemyBaseStats component not found on " + gameObject.name);
            currentMaxHealth = 100f;
            currentHealth = currentMaxHealth;
            currentDamage = 10f;
            currentArmor = 0f;
            currentMovementSpeed = 2f;

        }
    }

    float GetLevelScalingFactor(float baseFactor)
    {
        float factor = 1f;
        for (int i = 1; i < currentLevel; i++)
        {
            factor *= baseFactor;
        }
        return factor;
    }

    //Effect/visual/audio effect upon taking dmg
    public void OnDamageTaken(float amount)
    {
    }

    public void TakeDamage(DamageData damageData,KnockbackData knockbackData)
    {
        if (isKnockedBack) return; // Prevent multiple knockbacks at once

        float finalDamage = CalculateTakenDamage(damageData);
        currentHealth -= finalDamage;
        Debug.Log($"{this.gameObject.name} received {finalDamage} {damageData.type} Damage from {damageData.source.name}");
        //Add Knockback to this enemy
        StartCoroutine(KnockbackRoutine(knockbackData.direction));
        UpdateHealthBar();
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    void UpdateHealthBar()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth / currentMaxHealth; // Update the Slider value
        }
    }

    public float CalculateTakenDamage(DamageData damageData)
    {
        float takenDamage = damageData.amount;

        switch (damageData.type)
        {
            case DamageData.DamageType.Physical:
                takenDamage -= currentArmor; // Flat reduction
                takenDamage = Mathf.Max(takenDamage, 0f);
                break;

            case DamageData.DamageType.Magical:
                float magicReductionPercent = currentMagicResistance / 100f;
                takenDamage *= (1f - Mathf.Clamp01(magicReductionPercent));
                break;

            // Add more types if needed
            default:
                break;
        }

        return takenDamage;
    }
    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    private IEnumerator KnockbackRoutine(Vector2 direction)
    {
        Debug.Log("Start Knockback");
        isKnockedBack = true;
        rb.linearVelocity = Vector2.zero; // Optional: Reset velocity before applying new force

        // Apply knockback reduction
        float adjustedKnockbackForce = currentKnockbackForce * (1f - Mathf.Clamp01(currentKnockbackReduction));
        rb.AddForce(direction.normalized * adjustedKnockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(currentKnockbackDuration);

        rb.linearVelocity = Vector2.zero; // Optional: Stop the enemy after knockback
        isKnockedBack = false;
    }

}
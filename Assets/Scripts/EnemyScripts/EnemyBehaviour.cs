using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBehaviour : MonoBehaviour, IDamagable
{
    [SerializeField]private EnemyBaseStats baseStats;
    public Slider healthBarSlider;

    public GameObject target;
    private Rigidbody2D rb;

    public float knockbackForce = 10f;
    public float knockbackDuration = 0.5f;
    private bool isKnockedBack = false;

    [Header("Level")]
    public int currentLevel = 1;

    [Header("Base Stats")]
    public float currentMaxHealth;
    public float currentHealth;
    public float currentDamage;
    public float currentArmor;
    public float currentAttackSpeed;

    [Header("Crit")]
    public float currentCritChance;
    public float currentCritHitMultiplier;

    [Header("Resistances")]
    public float currentMagicResistance;

    [Header("Ranges")]
    public float currentAttackRange;

    [Header("Movement")]
    public float currentMovementSpeed;

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
        }
        else
        {
            Debug.LogError("EnemyBaseStats component not found on " + gameObject.name);
            currentMaxHealth = 300f;
            currentHealth = 300f;
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

    public bool IsInvulnerable { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    //Effect/visual/audio effect upon taking dmg
    public void OnDamageTaken(float amount)
    {
    }
    public void TakeDamage(DamageData damageData,KnockbackData knockbackData)
    {
        if (isKnockedBack) return; // Prevent multiple knockbacks at once

        currentHealth -= CalculateTakenDamage_PercentageLinear(damageData.amount, currentArmor);
        Debug.Log($"{this.gameObject.name} received {damageData.amount} {damageData.type} Damage from {damageData.source.name}");
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

    public float CalculateTakenDamage_PercentageLinear(float incomingDamage, float armor)
    {
        float damageReductionPercentage = armor / 100f; // Assuming 1 armor point = 1% reduction
        float takenDamage = incomingDamage * (1f - Mathf.Clamp01(damageReductionPercentage));
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
        rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        rb.linearVelocity = Vector2.zero; // Optional: Stop the enemy after knockback
        isKnockedBack = false;
        // Re-enable enemy AI/movement here if it was disabled
    }

}
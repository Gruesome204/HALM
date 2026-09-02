using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProjectileBehaviour : MonoBehaviour
{
    private DamageData damageData;
    private GameObject owner; // The turret that fired this projectile

    public float knockbackStrength;
    public float knockbackDuration;
    public Vector2 direction;
    private Rigidbody2D rb;

    public int remainingPierces;

    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private LayerMask wallLayer;

    [Header("Chain Lightning")]
    public int chainBounceCount = 0;
    public float chainBounceRange = 5f;
    private Transform chainTarget;
    private int currentBounces = 0;
    private List<Transform> hitEnemies = new List<Transform>(); // Track hit enemies for chain
    [SerializeField] private LayerMask obstacleLayer; // Layer for obstacles that block chain

    [Header("Homing")]
    public float homingStrength = 2f;
    private Transform homingTarget;
    private bool isHoming = false;

    [Header("AOE")]
    public bool isAOE = false;
    public float aoeRadius = 3f;
    [SerializeField] private GameObject aoeEffectPrefab; // Optional: visual effect for AOE

    // Add a flag to prevent multiple hits on same enemy
    private HashSet<GameObject> hitEnemySet = new HashSet<GameObject>();

    public void InitializePiercing(int pierces)
    {
        remainingPierces = pierces;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Handle homing behavior
        if (isHoming && homingTarget != null)
        {
            Vector2 directionToTarget = (homingTarget.position - transform.position).normalized;
            Vector2 currentVelocity = rb.linearVelocity;

            // Smoothly rotate towards target
            Vector2 newVelocity = Vector2.Lerp(currentVelocity, directionToTarget * currentVelocity.magnitude, homingStrength * Time.deltaTime);
            rb.linearVelocity = newVelocity;

            // Rotate projectile to face movement direction
            if (rb.linearVelocity.sqrMagnitude > 0.01f)
            {
                transform.up = rb.linearVelocity.normalized;
            }
        }
        else
        {
            // Original rotation behavior
            if (rb.linearVelocity.sqrMagnitude > 0.01f)
            {
                transform.up = rb.linearVelocity.normalized;
            }
        }
    }

    public void SetOwner(GameObject turret, float damageAmount)
    {
        owner = turret;
        damageData = new DamageData
        {
            source = turret,
            amount = damageAmount,
            type = DamageData.DamageType.Physical
        };
    }

    // Chain Lightning Methods
    public void SetChainTarget(Transform target)
    {
        chainTarget = target;
        hitEnemies.Clear();
        currentBounces = 0;
        // Add the initial target to hit list
        if (target != null)
            hitEnemies.Add(target);
    }

    // Homing Methods
    public void SetHomingTarget(Transform target)
    {
        homingTarget = target;
        isHoming = true;
    }

    public void SetLayerMasks(LayerMask target, LayerMask wall, LayerMask obstacle)
    {
        targetLayer = target;
        wallLayer = wall;
        obstacleLayer = obstacle;
    }

    public void SetAOE(GameObject effectPrefab)
    {
        aoeEffectPrefab = effectPrefab;
    }

    public void SetPiercing(int pierces)
    {
        remainingPierces = pierces;
    }

    public void SetKnockback(float strength, float duration)
    {
        knockbackStrength = strength;
        knockbackDuration = duration;
    }

    public void SetChainLightning(int bounceCount, float bounceRange)
    {
        chainBounceCount = bounceCount;
        chainBounceRange = bounceRange;
    }

    public void SetHoming(float strength, Transform target)
    {
        homingStrength = strength;
        SetHomingTarget(target);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if hit a wall
        if (((1 << other.gameObject.layer) & wallLayer) != 0)
        {
            // Chain lightning stops at walls
            Destroy(gameObject);
            return;
        }

        if (((1 << other.gameObject.layer) & targetLayer) == 0)
            return;
        if (other.gameObject == owner)
            return;

        // Prevent double-hitting the same enemy (important for chain lightning)
        if (hitEnemySet.Contains(other.gameObject))
            return;

        // Try to get the IDamagable interface from the collided object
        IDamagable damagable = other.GetComponent<IDamagable>();

        if (damagable != null)
        {
            // Add to hit set to prevent double hits
            hitEnemySet.Add(other.gameObject);

            // Calculate direction for knockback
            Rigidbody2D targetRb = other.GetComponent<Rigidbody2D>();
            if (targetRb != null)
            {
                direction = (targetRb.transform.position - transform.position).normalized;
            }

            // Handle AOE damage
            if (isAOE)
            {
                DealAOEDamage(other.transform.position);
                Destroy(gameObject);
                return;
            }

            // Apply damage with knockback
            KnockbackData knockbackData = new KnockbackData
            {
                knockbackStrength = knockbackStrength,
                knockbackDuration = knockbackDuration,
                direction = direction,
            };

            damagable.TakeDamage(damageData, knockbackData);

            // Chain Lightning Logic
            if (chainBounceCount > 0 && currentBounces < chainBounceCount)
            {
                HandleChainLightning(other.transform);
                // Don't destroy - let chain continue
            }
            else
            {
                // Handle piercing
                remainingPierces--;
                if (remainingPierces < 0)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    private void HandleChainLightning(Transform currentTarget)
    {
        currentBounces++;

        // Find nearest enemy in range that hasn't been hit yet
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(
            currentTarget.position,
            chainBounceRange,
            targetLayer
        );

        Transform nearestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Collider2D enemyCollider in enemiesInRange)
        {
            if (enemyCollider.gameObject == owner)
                continue;

            // Skip if already hit
            if (hitEnemySet.Contains(enemyCollider.gameObject))
                continue;

            // Check line of sight for chain
            if (!HasLineOfSight(currentTarget.position, enemyCollider.transform.position))
                continue;

            float distance = Vector2.Distance(currentTarget.position, enemyCollider.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = enemyCollider.transform;
            }
        }

        if (nearestEnemy != null)
        {
            // Create chain visual effect (lightning bolt)
            CreateChainVisual(currentTarget.position, nearestEnemy.position);

            // Deal damage to the new target
            IDamagable damagable = nearestEnemy.GetComponent<IDamagable>();
            if (damagable != null)
            {
                // Optionally reduce damage for chain bounces
                float chainDamage = damageData.amount * 0.8f; // 20% damage reduction per bounce
                DamageData chainDamageData = new DamageData
                {
                    source = owner,
                    amount = chainDamage,
                    type = DamageData.DamageType.Physical
                };

                // Knockback for chain lightning (maybe reduced)
                KnockbackData knockbackData = new KnockbackData
                {
                    knockbackStrength = knockbackStrength * 0.5f,
                    knockbackDuration = knockbackDuration * 0.5f,
                    direction = (nearestEnemy.position - currentTarget.position).normalized,
                };

                damagable.TakeDamage(chainDamageData, knockbackData);

                // Mark as hit
                hitEnemySet.Add(nearestEnemy.gameObject);

                // Continue the chain if possible
                if (currentBounces < chainBounceCount)
                {
                    HandleChainLightning(nearestEnemy);
                }
                else
                {
                    // Chain complete, destroy projectile
                    Destroy(gameObject);
                }
            }
        }
        else
        {
            // No more enemies to chain to, destroy projectile
            Destroy(gameObject);
        }
    }

    // Line of sight check for chain lightning
    private bool HasLineOfSight(Vector2 origin, Vector2 target)
    {
        Vector2 direction = (target - origin);
        float distance = direction.magnitude;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            direction.normalized,
            distance,
            obstacleLayer
        );

        // Debug visualization
        Debug.DrawRay(
            origin,
            direction.normalized * distance,
            hit.collider == null ? Color.green : Color.red,
            0.5f
        );

        return hit.collider == null;
    }

    private void CreateChainVisual(Vector2 start, Vector2 end)
    {
        // You can implement a visual effect here
        // For example, instantiate a line renderer or particle effect
        Debug.DrawLine(start, end, Color.cyan, 0.5f);

        // Optional: Instantiate a lightning prefab
        // GameObject lightningEffect = Instantiate(lightningPrefab, (start + end) / 2, Quaternion.identity);
        // lightningEffect.GetComponent<LineRenderer>().SetPositions(new Vector3[] { start, end });
    }

    private void DealAOEDamage(Vector2 center)
    {
        // Create AOE visual effect
        if (aoeEffectPrefab != null)
        {
            Instantiate(aoeEffectPrefab, center, Quaternion.identity);
        }

        // Find all enemies in AOE radius
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(
            center,
            aoeRadius,
            targetLayer
        );

        foreach (Collider2D enemyCollider in enemiesInRange)
        {
            if (enemyCollider.gameObject == owner)
                continue;

            IDamagable damagable = enemyCollider.GetComponent<IDamagable>();
            if (damagable != null)
            {
                // Knockback away from center
                Vector2 knockbackDirection = (enemyCollider.transform.position - (Vector3)center).normalized;

                KnockbackData knockbackData = new KnockbackData
                {
                    knockbackStrength = knockbackStrength * 0.5f, // Reduced for AOE
                    knockbackDuration = knockbackDuration * 0.5f,
                    direction = knockbackDirection,
                };

                // You might want to reduce damage for enemies at the edge of AOE
                float distance = Vector2.Distance(center, enemyCollider.transform.position);
                float damageMultiplier = 1f - (distance / aoeRadius) * 0.5f; // 50% reduction at edge

                DamageData aoeDamageData = new DamageData
                {
                    source = owner,
                    amount = damageData.amount * damageMultiplier,
                    type = DamageData.DamageType.Physical
                };

                damagable.TakeDamage(aoeDamageData, knockbackData);
            }
        }
    }

    // Called when projectile is destroyed
    private void OnDestroy()
    {
        // If it's an AOE projectile and wasn't triggered, still deal AOE damage
        if (isAOE && !hitEnemySet.Contains(owner) && !hitEnemySet.Contains(null))
        {
            // Only trigger if it was destroyed by hitting a wall or timeout
            DealAOEDamage(transform.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (isAOE)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, aoeRadius);
        }

        if (chainBounceCount > 0)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, chainBounceRange);
        }
    }
}
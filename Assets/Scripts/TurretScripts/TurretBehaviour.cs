using System;
using System.Collections;
using UnityEngine;

public class TurretBehaviour : MonoBehaviour
{
    public TurretBlueprint turretBlueprint;
    public GameObject projectilePrefab;

    public Transform firePoint; // A point on the turret where projectiles spawn

    [Header("Values")]
    public float currentFireRate;
    public float currentFireCountdown = 0f;
    public float currentProjectileSpeed;
    public float currentAttackRange;
    public float currentAttackDamage;
    public float currentKnockbackStrength;
    public float currentKnockbackDuration;

    private Transform targetEnemy;

    [Header("Targeting Settings")] // Optional: for better organization in Inspector
    public LayerMask enemyLayer; // New variable to select the enemy layer

    public enum FiringPattern
    {
        SingleShot,
        FireSalve
    }
    public FiringPattern currentFiringPattern = FiringPattern.SingleShot;

    [Header("Fire Salve Settings")]
    public int projectilesPerSalve = 2; // Number of projectiles in a salve
    public float delayBetweenSalveProjectiles = 0.5f; // Delay between each projectile in a salve


    void Start()
    {
        if (turretBlueprint != null)
        {
            InitializeFromBlueprint();
        }
    }
    public void InitializeFromBlueprint()
    {
        currentFireCountdown = turretBlueprint.fireCountdown;
        currentAttackRange = turretBlueprint.attackRange;
        currentFireRate = turretBlueprint.fireRate;
        currentProjectileSpeed = turretBlueprint.projectileSpeed;
        currentKnockbackStrength = turretBlueprint.knockbackStrength;
        currentKnockbackDuration = turretBlueprint.knockbackDuration;
    }

    void Update()
    {
        FindTarget();
        if (targetEnemy != null)
        {
            // *** ADD THIS TURRET ROTATION LOGIC HERE ***
            // Calculate the direction from the turret's position to the target enemy's position
            Vector3 directionToTarget = targetEnemy.position - transform.position;

            // Calculate the angle in degrees for 2D rotation (around Z-axis)
            // Mathf.Atan2 returns the angle in radians between the X-axis and a 2D vector (y, x).
            // Multiply by Mathf.Rad2Deg to convert radians to degrees.
            float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;

            // Apply the rotation to the turret.
            // Assuming your turret's front is aligned with the positive X-axis when its Z-rotation is 0.
            // If your turret sprite is oriented differently (e.g., facing up when rotation is 0),
            // you might need to add an offset: Quaternion.Euler(0, 0, angle - 90f);
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

            if (currentFireCountdown <= 0f)
            {
                // Call the appropriate shooting pattern
                switch (currentFiringPattern)
                {
                    case FiringPattern.SingleShot:
                        ShootSingleProjectile();
                        break;
                    case FiringPattern.FireSalve:
                        StartCoroutine(ShootFireSalve());
                        break;
                }
                currentFireCountdown = turretBlueprint.fireCountdown / currentFireRate;
            }
            currentFireCountdown -= Time.deltaTime;
        }
        else
        {
            // Optionally do something when no target is in range
            // For example, reset turret rotation to default or stop rotating.
        }
    }

    //TODO: Add different Shooting Patterns
    void FindTarget()
    {
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, currentAttackRange);

        // Initialize shortestDistance to a very large value
        float shortestDistance = Mathf.Infinity;
        // Temporarily store the closest enemy found in this iteration
        Transform closestEnemyInThisScan = null;

        foreach (Collider2D enemyCollider in enemiesInRange)
        {
            EnemyBehaviour enemy = enemyCollider.GetComponent<EnemyBehaviour>();
            if (enemy != null)
            {
                float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    closestEnemyInThisScan = enemy.transform; // Assign to the temporary variable
                }
            }
        }

        // Assign the closest enemy found (or null if none) to the class-level targetEnemy
        this.targetEnemy = closestEnemyInThisScan;
    }

    void ShootSingleProjectile()
    {
        if (projectilePrefab != null && firePoint != null && targetEnemy != null)
        {
            GameObject projectileObject = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

            //SetOwner of the bullet(Turret) and damage
            projectileObject.GetComponent<BallProjectileBehaviour>().SetOwner(this.gameObject, currentAttackDamage);

            projectileObject.GetComponent<BallProjectileBehaviour>().knockbackStrength = currentKnockbackStrength;
            projectileObject.GetComponent<BallProjectileBehaviour>().knockbackDuration = currentKnockbackDuration;


            Rigidbody2D projectileRb = projectileObject.GetComponent<Rigidbody2D>();
            if (projectileRb != null)
            {
                Vector3 directionToTarget = (targetEnemy.position - firePoint.position).normalized;
                projectileRb.linearVelocity = directionToTarget * currentProjectileSpeed;
                Destroy(projectileObject, 5f);
            }
            else
            {
                Debug.LogWarning("Projectile prefab does not have a Rigidbody component. It won't move.");
                Destroy(projectileObject);
            }
        }
    }
    // Coroutine for shooting a fire salve
    IEnumerator ShootFireSalve()
    {
        for (int i = 0; i < projectilesPerSalve; i++)
        {
            ShootSingleProjectile(); // Reuse the single projectile shooting logic
            yield return new WaitForSeconds(delayBetweenSalveProjectiles);
            currentFireCountdown = 0f;
            // If the target is lost during a salve, stop firing
            if (targetEnemy == null)
            {
                yield break;
            }
        }
    }

    // This method will always draw the Gizmo in the Scene view
    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, currentAttackRange);
    //}

    // You can remove OnDrawGizmosSelected if you only want the always-on Gizmo
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, currentAttackRange);
    }

}
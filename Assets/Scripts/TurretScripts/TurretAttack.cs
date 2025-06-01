using System;
using System.Collections;
using UnityEngine;

public class TurretAttack : MonoBehaviour
{
    public TurretBlueprint turretBlueprint;
    public GameObject projectilePrefab;
    public Transform firePoint; // A point on the turret where projectiles spawn

    private float currentfireCountdown = 0f;
    private float currentAttackRange;
    private float currentFireRate;
    private float currentProjectileSpeed;
    private float currentKnockbackStrength;
    private float currentKnockbackDuration;

    private Transform targetEnemy;

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
        currentfireCountdown = turretBlueprint.fireCountdown;
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
            if (currentfireCountdown <= 0f)
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
                currentfireCountdown = 1f / currentFireRate;
            }
            currentfireCountdown -= Time.deltaTime;
        }
        else
        {
            // Optionally do something when no target is in range
        }
    }

    //TODO: Add different Shooting Patterns
    void FindTarget()
    {
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, currentAttackRange);
        Transform targetEnemy = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Collider2D enemyCollider in enemiesInRange)
        {
            EnemyBehaviour enemy = enemyCollider.GetComponent<EnemyBehaviour>();
            if (enemy != null)
            {
                float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    targetEnemy = enemy.transform;
                }
            }
        }

        this.targetEnemy = targetEnemy;
    }
    void ShootSingleProjectile()
    {
        if (projectilePrefab != null && firePoint != null && targetEnemy != null)
        {
            GameObject projectileObject = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            projectileObject.GetComponent<BallProjectileBehaviour>().sourceOfDamage = this.gameObject;
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
            currentfireCountdown = 0f;
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
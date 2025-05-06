using System;
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

    private Transform targetEnemy;
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
    }


    void Update()
    {
        FindTarget();

        if (targetEnemy != null)
        {
            if (currentfireCountdown <= 0f)
            {
                Shoot();
                currentfireCountdown = 1f / currentFireRate;
            }
            currentfireCountdown -= Time.deltaTime;
        }
        else
        {
            // Optionally do something when no target is in range
        }
    }

    void FindTarget()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, currentAttackRange);
        Transform bestTarget = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Collider enemyCollider in enemiesInRange)
        {
            EnemyBehaviour enemy = enemyCollider.GetComponent<EnemyBehaviour>();
            if (enemy != null)
            {
                float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    bestTarget = enemy.transform;
                }
            }
        }

        targetEnemy = bestTarget;
    }

    void Shoot()
    {
        if (projectilePrefab != null && firePoint != null && targetEnemy != null)
        {
            // Instantiate the projectile
            GameObject projectileObject = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            projectileObject.GetComponent<BallProjectileBehaviour>().sourceOfDamage = this.gameObject;
            projectileObject.GetComponent<BallProjectileBehaviour>().sourceOfDamage = this.gameObject;

            // Get the Rigidbody component of the projectile (assuming it has one)
            Rigidbody2D projectileRb = projectileObject.GetComponent<Rigidbody2D>();
            if (projectileRb != null)
            {
                // Calculate the direction to the target
                Vector3 directionToTarget = (targetEnemy.position - firePoint.position).normalized;

                // Apply force to the projectile to move it towards the target
                projectileRb.linearVelocity = directionToTarget * currentProjectileSpeed;

                // Optionally, you can add code here to make the projectile clean up after some time
                Destroy(projectileObject, 1f); // Example: Destroy after 5 seconds
            }
            else
            {
                Debug.LogWarning("Projectile prefab does not have a Rigidbody component. It won't move.");
                Destroy(projectileObject); // Destroy immediately if no Rigidbody
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
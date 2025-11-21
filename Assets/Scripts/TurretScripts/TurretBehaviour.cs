using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurretBehaviour : MonoBehaviour, IPausable
{
    public TurretBlueprint turretBlueprint;
    public GameObject projectilePrefab;

    public Transform firePoint; // A point on the turret where projectiles spawn

    [SerializeField] private GameObject healthBarPrefab;

    [Header("Values")]
    public float currentFireRate;
    public float currentFireCountdown;
    public float currentProjectileSpeed;
    public float currentAttackRange;
    public float currentAttackDamage;
    public float currentKnockbackStrength;
    public float currentKnockbackDuration;

    private Transform targetEnemy;

    [Header("Targeting Settings")] // Optional: for better organization in Inspector
    public LayerMask enemyLayer; // New variable to select the enemy layer

    private TurretBlueprint.FiringPattern currentFiringPattern;
    private bool isShootingSalve;
    private int projectilesPerSalve; // Number of projectiles in a salve
    private float delayBetweenSalveProjectiles; // Delay between each projectile in a salve

    private bool isPaused;


    private void OnEnable() => GameManager.Instance?.RegisterPausable(this);
    private void OnDisable() => GameManager.Instance?.UnregisterPausable(this);

    public void OnPause()
    {
        isPaused = true;
        // Stop moving, stop attacking, etc.
    }

    public void OnResume()
    {
        isPaused = false;
        // Resume AI
    }


    void Start()
    {
        if (turretBlueprint != null)
        {
            InitializeFromBlueprint();
        }

        var health = GetComponent<TurretHealth>();
        if (health != null)
        {
            health.AttachHealthBar(healthBarPrefab, new Vector3(0, 1.5f, 0));
        }

    }
    public void InitializeFromBlueprint()
    {
        currentFireCountdown = turretBlueprint.BaseFireCountdown;
        currentAttackRange = turretBlueprint.baseAttackRange;
        currentFireRate = turretBlueprint.baseFireRate;
        currentProjectileSpeed = turretBlueprint.baseProjectileSpeed;
        currentKnockbackStrength = turretBlueprint.baseKnockbackStrength;
        currentKnockbackDuration = turretBlueprint.baseKnockbackDuration;

        currentFiringPattern = turretBlueprint.firingPattern;
        projectilesPerSalve = turretBlueprint.projectilesPerSalve;
        delayBetweenSalveProjectiles = turretBlueprint.delayBetweenSalveProjectiles;
    }

    void Update()
    {
        if (isPaused) return;

        FindTarget();
        if (targetEnemy != null)
        {
            if (currentFireCountdown <= 0f)
            {
                // Call the appropriate shooting pattern
                switch (currentFiringPattern)
                {
                    case TurretBlueprint.FiringPattern.SingleShot:
                        ShootProjectileAt(targetEnemy);
                        break;
                    case TurretBlueprint.FiringPattern.FireSalve:
                        StartCoroutine(ShootFireSalve());
                        break;
                }
                currentFireCountdown = turretBlueprint.BaseFireCountdown;
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
        Collider2D[] enemiesInRange =
            Physics2D.OverlapCircleAll(transform.position, currentAttackRange, enemyLayer);

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

    private List<Transform> GetEnemiesInRange()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, currentAttackRange, enemyLayer);
        List<Transform> enemies = new List<Transform>();

        foreach (Collider2D hit in hits)
        {
            EnemyBehaviour enemy = hit.GetComponent<EnemyBehaviour>();
            if (enemy != null)
                enemies.Add(enemy.transform);
        }

        return enemies;
    }


    // Coroutine for shooting a fire salve
    IEnumerator ShootFireSalve()
    {
        if (isShootingSalve) yield break;
        isShootingSalve = true;

        float delay = delayBetweenSalveProjectiles;

        // snapshot of all enemies in range at the moment the salve starts
        List<Transform> targets = GetEnemiesInRange();

        if (targets.Count == 0)
        {
            isShootingSalve = false;
            yield break;
        }

        // Optional: shuffle targets for more dynamic salve
        for (int t = 0; t < targets.Count; t++)
        {
            int r = UnityEngine.Random.Range(t, targets.Count);
            var temp = targets[t];
            targets[t] = targets[r];
            targets[r] = temp;
        }

        int targetIndex = 0;

        for (int i = 0; i < projectilesPerSalve; i++)
        {
            // Pause-safe waiting
            while (isPaused)
                yield return null;

            // Filter alive and in-range targets
            targets.RemoveAll(t => t == null || Vector2.Distance(transform.position, t.position) > currentAttackRange);
            if (targets.Count == 0)
                break;

            Transform currentTarget = targets[targetIndex % targets.Count];

            ShootProjectileAt(currentTarget);

            // Move to next target in the next shot
            targetIndex++;

            // Wait between shots
            float elapsed = 0f;
            while (elapsed < delay)
            {
                if (!isPaused)
                    elapsed += Time.deltaTime;

                yield return null;
            }
        }

        // normal cooldown after salve
        currentFireCountdown = turretBlueprint.BaseFireCountdown;

        isShootingSalve = false;
    }

    void ShootProjectileAt(Transform target)
    {
        if (!projectilePrefab || !firePoint || target == null) return;

        GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        var projectile = projectileObj.GetComponent<BallProjectileBehaviour>();
        var rb = projectileObj.GetComponent<Rigidbody2D>();

        if (projectile == null || rb == null)
        {
            Debug.LogWarning("Projectile prefab is missing components.");
            Destroy(projectileObj);
            return;
        }

        projectile.SetOwner(gameObject, currentAttackDamage);
        projectile.knockbackStrength = currentKnockbackStrength;
        projectile.knockbackDuration = currentKnockbackDuration;

        Vector2 direction = (target.position - firePoint.position).normalized;
        rb.linearVelocity = direction * currentProjectileSpeed;

        Destroy(projectileObj, 5f);
    }

    void ShootSingleProjectile()
    {
        if (targetEnemy != null)
            ShootProjectileAt(targetEnemy);
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
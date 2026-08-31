    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TurretBehaviour : MonoBehaviour, IPausable
    {
        [Header("References")]
        public TurretBlueprint turretBlueprint;
        private TurretStats stats;
        public GameObject currentProjectileType;
        public Transform firePoint;
        [SerializeField] private GameObject healthBarPrefab;

        public float currentShotCooldown;

        private Transform targetEnemy;

        [Header("Targeting Settings")] // Optional: for better organization in Inspector
        public LayerMask enemyLayer; // New variable to select the enemy layer

        [Header("Line of Sight")]
        [SerializeField] private LayerMask obstacleLayer;

        private FiringPattern currentFiringPattern;
        private bool salveInProgress;
        public int projectilesPerSalve; // Number of projectiles in a salve
        private float delayBetweenSalveProjectiles; // Delay between each projectile in a salve
        private int burstShotsFired;
        private bool burstInProgress;
        private float sprayTimer;
        private bool sprayActive;
        private Coroutine sprayCoroutine;

    private bool isPaused;

        private TurretGlobalModifierManager global;
        private TurretUpgradeChoiceManager upgrades;


        private void OnEnable()
        {
            GameManager.Instance?.RegisterPausable(this);
        }

        private void OnDisable()
        {
                GameManager.Instance?.UnregisterPausable(this);
        }

        // Pause system
        public void OnPause() => isPaused = true;
        public void OnResume() => isPaused = false;

        private void Awake()
        {
            global = TurretGlobalModifierManager.Instance;
            upgrades = TurretUpgradeChoiceManager.Instance;
            stats = GetComponent<TurretStats>();
        }


        void Start()
        {
            if (turretBlueprint != null && currentProjectileType == null)
                currentProjectileType = turretBlueprint.turretProjectileType;

            currentFiringPattern = turretBlueprint?.firingPattern ?? FiringPattern.SingleShot;
            delayBetweenSalveProjectiles = turretBlueprint?.delayBetweenSalveProjectiles ?? 0.1f;

            var health = GetComponent<TurretHealth>();
            if (health != null)
            {
                health.AttachHealthBar(healthBarPrefab, new Vector3(0, 1.5f, 0));
            }
        }

        void Update()
        {
            if (isPaused) return;

            currentShotCooldown -= Time.deltaTime;

            if (currentShotCooldown > 0f)
                return;

            FindTarget();
            if (targetEnemy == null)
                return;

            Fire();
        }

        void FindTarget()
            {
                Collider2D[] enemiesInRange =
               Physics2D.OverlapCircleAll(
        transform.position,
        stats.currentAttackRange,
        enemyLayer);

            // Initialize shortestDistance to a very large value
            float shortestDistance = Mathf.Infinity;
                // Temporarily store the closest enemy found in this iteration
                Transform closestEnemyInThisScan = null;

                foreach (Collider2D enemyCollider in enemiesInRange)
                {
                    EnemyBehaviour enemy = enemyCollider.GetComponent<EnemyBehaviour>();

                    if (enemy != null)
                    {
                        // NEW CHECK
                        if (!HasLineOfSight(enemy.transform))
                            continue;

                        float distanceToEnemy =
                            Vector2.Distance(transform.position, enemy.transform.position);

                        if (distanceToEnemy < shortestDistance)
                        {
                            shortestDistance = distanceToEnemy;
                            closestEnemyInThisScan = enemy.transform;
                        }
                    }
                }

            // Assign the closest enemy found (or null if none) to the class-level targetEnemy
            this.targetEnemy = closestEnemyInThisScan;
            }

        private List<Transform> GetEnemiesInRange()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, stats.currentAttackRange, enemyLayer);
            List<Transform> enemies = new List<Transform>();

            foreach (Collider2D hit in hits)
            {
                EnemyBehaviour enemy = hit.GetComponent<EnemyBehaviour>();
                if (enemy != null)
                    enemies.Add(enemy.transform);
            }

            return enemies;
        }

    private void Fire()
    {
        switch (currentFiringPattern)
        {
            case FiringPattern.SingleShot:
                ShootProjectileAt(targetEnemy);
                ResetFiringCooldown();
                break;

            case FiringPattern.FireSalve:
                if (!salveInProgress)
                    StartCoroutine(FireSalveWithCooldown());
                break;

            case FiringPattern.BurstFire:
                if (!burstInProgress)
                    StartCoroutine(FireBurstWithCooldown());
                break;

            case FiringPattern.ScatterShot:
                FireScatterShot(targetEnemy);
                ResetFiringCooldown();
                break;

            case FiringPattern.ChainLightning:
                FireChainLightning(targetEnemy);
                ResetFiringCooldown();
                break;

            case FiringPattern.HomingMissile:
                FireHomingProjectile(targetEnemy);
                ResetFiringCooldown();
                break;

            case FiringPattern.AOEShot:
                FireAOEProjectile(targetEnemy);
                ResetFiringCooldown();
                break;

            case FiringPattern.SprayShot:
                if (sprayCoroutine == null)
                    sprayCoroutine = StartCoroutine(FireSprayShot());
                break;
        }
    }
    private bool HasLineOfSight(Transform target)
            {
                if (target == null)
                    return false;

                Vector2 origin = firePoint.position;
                Vector2 direction = (target.position - firePoint.position);

                float distance = direction.magnitude;

                RaycastHit2D hit = Physics2D.Raycast(
                    origin,
                    direction.normalized,
                    distance,
                    obstacleLayer
                );

                Debug.DrawRay(
                    origin,
                    direction.normalized * distance,
                    hit.collider == null ? Color.green : Color.red
                );

                return hit.collider == null;
            }

        void ShootProjectileAt(Transform target)
            {
                if (currentProjectileType == null || target == null || firePoint == null)
                    return;

                GameObject projectileObj = Instantiate(
                    currentProjectileType,
                    firePoint.position,
                    firePoint.rotation
                );
                var projectile = projectileObj.GetComponent<ProjectileBehaviour>();
                var rb = projectileObj.GetComponent<Rigidbody2D>();

                if (projectile == null || rb == null)
                {
                    Debug.LogWarning("Projectile prefab is missing components.");
                    Destroy(projectileObj);
                    return;
                }

            projectile.SetOwner(gameObject,stats.currentAttackDamage);
            projectile.knockbackStrength = stats.currentKnockbackStrength;
            projectile.knockbackDuration = stats.currentKnockbackDuration;
            projectile.InitializePiercing(stats.currentProjectilePierce);
            Vector2 direction = (target.position - firePoint.position).normalized;
            rb.linearVelocity = direction * stats.currentProjectileSpeed;

            Destroy(projectileObj, 5f);

                // <-- Play shooting sound
                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlayTowerShoot();
            }

            private void ResetFiringCooldown()
            {
            currentShotCooldown = stats.currentShotInterval;
        }
        private IEnumerator FireSalveWithCooldown()
        {
            salveInProgress = true;

            // Get all enemies in range at start
            List<Transform> targets = GetEnemiesInRange();
            if (targets.Count == 0)
            {
                salveInProgress = false;
                yield break;
            }

            // Shuffle targets for dynamic salve
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

                // Remove dead or out-of-range targets
                targets.RemoveAll(t => t == null || Vector2.Distance(transform.position, t.position) > stats.currentAttackRange);
                if (targets.Count == 0)
                    break;

                Transform currentTarget = targets[targetIndex % targets.Count];

                if (currentTarget != null && HasLineOfSight(currentTarget))
                {
                    ShootProjectileAt(currentTarget);
                }
                targetIndex++;

                // Wait between shots
                float elapsed = 0f;
                while (elapsed < delayBetweenSalveProjectiles)
                {
                    if (!isPaused)
                        elapsed += Time.deltaTime;
                    yield return null;
                }
            }

            ResetFiringCooldown(); // cooldown applied after full salve
            salveInProgress = false;
        }
        public void SetFiringPattern(FiringPattern pattern)
        {
            currentFiringPattern = pattern;
        }

    private IEnumerator FireBurstWithCooldown()
    {
        burstInProgress = true;
        int shotsFired = 0;

        while (shotsFired < turretBlueprint.burstCount)
        {
            while (isPaused)
                yield return null;

            // Find target for each burst shot
            FindTarget();
            if (targetEnemy != null && HasLineOfSight(targetEnemy))
            {
                ShootProjectileAt(targetEnemy);
            }

            shotsFired++;

            if (shotsFired < turretBlueprint.burstCount)
            {
                float elapsed = 0f;
                while (elapsed < turretBlueprint.burstDelay)
                {
                    if (!isPaused)
                        elapsed += Time.deltaTime;
                    yield return null;
                }
            }
        }

        ResetFiringCooldown();
        burstInProgress = false;
    }
    // Scatter Shot
    private void FireScatterShot(Transform target)
    {
        if (target == null || firePoint == null)
            return;

        int projectileCount = turretBlueprint.scatterCount;
        float angleSpread = turretBlueprint.scatterAngle;

        Vector2 baseDirection = (target.position - firePoint.position).normalized;
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;

        for (int i = 0; i < projectileCount; i++)
        {
            float angleOffset = UnityEngine.Random.Range(-angleSpread / 2, angleSpread / 2);
            float angle = baseAngle + angleOffset;

            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            // Create projectile with direction
            GameObject projectileObj = CreateProjectile(direction);
            if (projectileObj != null)
            {
                // Apply scatter-specific modifications if needed
                ProjectileBehaviour projectile = projectileObj.GetComponent<ProjectileBehaviour>();
                // Optional: Reduce damage for scatter shots
                // projectile.SetOwner(gameObject, stats.currentAttackDamage * 0.7f);
            }
        }
    }

    // Chain Lightning
    private void FireChainLightning(Transform firstTarget)
    {
        if (firstTarget == null || firePoint == null)
            return;

        // Create first projectile
        GameObject projectileObj = CreateProjectile(
            (firstTarget.position - firePoint.position).normalized
        );

        if (projectileObj != null)
        {
            ProjectileBehaviour projectile = projectileObj.GetComponent<ProjectileBehaviour>();
            if (projectile != null)
            {
                // Set chain properties
                projectile.SetOwner(gameObject, stats.currentAttackDamage);
                projectile.chainBounceCount = turretBlueprint.chainBounceCount;
                projectile.chainBounceRange = turretBlueprint.chainBounceRange;

                // Set target for chain
                projectile.SetChainTarget(firstTarget);
            }
        }
    }

    // Homing Missile
    private void FireHomingProjectile(Transform target)
    {
        if (target == null || firePoint == null)
            return;

        Vector2 initialDirection = (target.position - firePoint.position).normalized;
        GameObject projectileObj = CreateProjectile(initialDirection);

        if (projectileObj != null)
        {
            ProjectileBehaviour projectile = projectileObj.GetComponent<ProjectileBehaviour>();
            if (projectile != null)
            {
                projectile.SetOwner(gameObject, stats.currentAttackDamage);
                projectile.homingStrength = turretBlueprint.homingStrength;
                projectile.SetHomingTarget(target);
            }
        }
    }

    // AOE Shot
    private void FireAOEProjectile(Transform target)
    {
        if (target == null || firePoint == null)
            return;

        Vector2 direction = (target.position - firePoint.position).normalized;
        GameObject projectileObj = CreateProjectile(direction);

        if (projectileObj != null)
        {
            ProjectileBehaviour projectile = projectileObj.GetComponent<ProjectileBehaviour>();
            if (projectile != null)
            {
                projectile.SetOwner(gameObject, stats.currentAttackDamage);
                projectile.isAOE = true;
                projectile.aoeRadius = turretBlueprint.aoeRadius;
            }
        }
    }

    // Spray Shot
    private IEnumerator FireSprayShot()
    {
        sprayActive = true;
        float fireInterval = 1f / turretBlueprint.sprayRate;

        while (sprayActive)
        {
            while (isPaused)
                yield return null;

            // Get enemies in range
            List<Transform> targets = GetEnemiesInRange();
            if (targets.Count > 0)
            {
                // Pick closest enemy
                Transform nearestTarget = GetNearestTarget(targets);
                if (nearestTarget != null && HasLineOfSight(nearestTarget))
                {
                    // Fire with spray cone
                    FireSprayShotAt(nearestTarget);
                }
            }

            float elapsed = 0f;
            while (elapsed < fireInterval)
            {
                if (!isPaused)
                    elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }

    private void FireSprayShotAt(Transform target)
    {
        if (target == null || firePoint == null)
            return;

        Vector2 baseDirection = (target.position - firePoint.position).normalized;
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;

        // Random spread within spray cone
        float angleOffset = UnityEngine.Random.Range(-turretBlueprint.sprayAngle / 2, turretBlueprint.sprayAngle / 2);
        float angle = baseAngle + angleOffset;

        Vector2 direction = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        );

        CreateProjectile(direction);
    }

    // Helper method to create projectile with direction
    private GameObject CreateProjectile(Vector2 direction)
    {
        if (currentProjectileType == null || firePoint == null)
            return null;

        GameObject projectileObj = Instantiate(
            currentProjectileType,
            firePoint.position,
            Quaternion.identity
        );

        var projectile = projectileObj.GetComponent<ProjectileBehaviour>();
        var rb = projectileObj.GetComponent<Rigidbody2D>();

        if (projectile == null || rb == null)
        {
            Debug.LogWarning("Projectile prefab is missing components.");
            Destroy(projectileObj);
            return null;
        }

        projectile.SetOwner(gameObject, stats.currentAttackDamage);
        projectile.knockbackStrength = stats.currentKnockbackStrength;
        projectile.knockbackDuration = stats.currentKnockbackDuration;
        projectile.InitializePiercing(stats.currentProjectilePierce);

        rb.linearVelocity = direction * stats.currentProjectileSpeed;

        Destroy(projectileObj, 5f);

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayTowerShoot();

        return projectileObj;
    }

    // Helper method to get nearest target
    private Transform GetNearestTarget(List<Transform> targets)
    {
        if (targets == null || targets.Count == 0)
            return null;

        Transform nearest = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Transform target in targets)
        {
            float distance = Vector2.Distance(transform.position, target.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearest = target;
            }
        }

        return nearest;
    }

    // Stop spray when pattern changes
    public void StopSpray()
    {
        sprayActive = false;
        if (sprayCoroutine != null)
        {
            StopCoroutine(sprayCoroutine);
            sprayCoroutine = null;
        }
    }

    void OnDrawGizmosSelected()
            {
                Gizmos.color = Color.red;
                if (stats != null)
                {
                    Gizmos.DrawWireSphere(
                        transform.position,
                        stats.currentAttackRange);
                }
            }

    }
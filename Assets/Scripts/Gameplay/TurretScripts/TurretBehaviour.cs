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

        private TurretBlueprint.FiringPattern currentFiringPattern;
        private bool salveInProgress;
        public int projectilesPerSalve; // Number of projectiles in a salve
        private float delayBetweenSalveProjectiles; // Delay between each projectile in a salve

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

        currentFiringPattern = turretBlueprint?.firingPattern ?? TurretBlueprint.FiringPattern.SingleShot;
        delayBetweenSalveProjectiles = turretBlueprint?.delayBetweenSalveProjectiles ?? 0.1f;

        var health = GetComponent<TurretHealth>();
        if (health != null)
        {
            health.AttachHealthBar(healthBarPrefab, new Vector3(0, 1.5f, 0));
        }
    }

    private void RecalculateStatsFromLevelManager()
    {
        if (turretBlueprint == null || TurretLevelManager.Instance == null)
            return;

        int level = TurretLevelManager.Instance.GetLevel(turretBlueprint.turretType);
        RecalculateStats(level);
    }

    public TurretStatData CalculateFinalStats(
        int level,
        TurretModifier upgrade,
        TurretGlobalModifierManager global)
    {
        if (turretBlueprint == null) return default;

        // Level scaling
        float scaledDamage = turretBlueprint.baseAttackDamage * (1 + turretBlueprint.baseDamageGrowthFactor * (level - 1));
        float scaledShotsPerSecond =
            turretBlueprint.baseShotsPerSecond *
            (1 + turretBlueprint.shotsPerSecondGrowthFactor * (level - 1));
        float scaledRange = turretBlueprint.baseAttackRange + turretBlueprint.baseRangeGrowthFlat * (level - 1);
        float scaledProjectileSpeed = turretBlueprint.baseProjectileSpeed;

        // --- Apply upgrades (additive) first ---
        float damageWithUpgrade = scaledDamage + (upgrade?.damageMultiplier ?? 0f);
        float shotsPerSecondWithUpgrade =
            scaledShotsPerSecond + (upgrade?.shotsPerSecondBonus ?? 0f);
        float projectileSpeedWithUpgrade = scaledProjectileSpeed + (upgrade?.projectileSpeed ?? 0f);
        float rangeWithUpgrade = scaledRange + (upgrade?.rangeBonus ?? 0f);

        int projectilesWithUpgrade = turretBlueprint.projectilesPerSalve + (upgrade?.projectilesPerSalve ?? 0);
        int pierceWithUpgrade = turretBlueprint.baseProjectilePierceCount + (upgrade?.piercingHits ?? 0);

        // --- Apply global modifiers (percentage) ---
        float finalDamage = damageWithUpgrade * (1f + (global?.globalDamageMultiplier ?? 0f));
        float finalShotsPerSecond =
       shotsPerSecondWithUpgrade *
       (1f + (global?.globalShotsPerSecondBonus ?? 0f));
        float finalProjectileSpeed = projectileSpeedWithUpgrade * (1f + (global?.globalProjectileSpeed ?? 0f));
        float finalRange = rangeWithUpgrade * (1f + (global?.globalPlacementRadiusMultiplier ?? 0f));
        int finalProjectiles = projectilesWithUpgrade + (global?.globalProjectilesPerSalve ?? 0);

        Debug.Log($"T{turretBlueprint.turretName} upgraded! Level {level} | " +
                  $"Damage={finalDamage}, FireRate={finalShotsPerSecond}, Range={finalRange}, " +
                  $"Projectiles={finalProjectiles}, ProjSpeed={finalProjectileSpeed}");

        return new TurretStatData
        {
            damage = finalDamage,
            shotsPerSecond = finalShotsPerSecond,
            range = finalRange,
            projectileSpeed = finalProjectileSpeed,
            projectilesPerSalve = finalProjectiles,
            pierceCount = pierceWithUpgrade,
            knockbackStrength = turretBlueprint.baseKnockbackStrength,
            knockbackDuration = turretBlueprint.baseKnockbackDuration
        };
    }

    public void RecalculateStats(int level)
    {
        TurretModifier upgrade =
            upgrades != null
                ? upgrades.GetCombinedModifier(turretBlueprint.turretType)
                : null;

        TurretStatData finalStats =
            CalculateFinalStats(level, upgrade, global);

        stats.currentAttackDamage = finalStats.damage;

        stats.currentShotsPerSecond =
            Mathf.Max(0.1f, finalStats.shotsPerSecond);

        stats.currentShotInterval =
            1f / stats.currentShotsPerSecond;

        stats.currentAttackRange = finalStats.range;

        stats.currentProjectileSpeed =
            finalStats.projectileSpeed;

        stats.currentProjectilePierce =
            finalStats.pierceCount;

        stats.currentKnockbackStrength =
            finalStats.knockbackStrength;

        stats.currentKnockbackDuration =
            finalStats.knockbackDuration;

        stats.currentProjectilesPerSalve =
            finalStats.projectilesPerSalve;

        // Determine firing mode
        if (stats.currentProjectilesPerSalve > 1)
        {
            currentFiringPattern =
                TurretBlueprint.FiringPattern.FireSalve;
        }
        else
        {
            currentFiringPattern =
                turretBlueprint.firingPattern;
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
            case TurretBlueprint.FiringPattern.SingleShot:
                ShootProjectileAt(targetEnemy);
                ResetFiringCooldown();
                break;

            case TurretBlueprint.FiringPattern.FireSalve:
                if (!salveInProgress)
                    StartCoroutine(FireSalveWithCooldown());
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
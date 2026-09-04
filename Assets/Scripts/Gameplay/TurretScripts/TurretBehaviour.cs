using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBehaviour : MonoBehaviour, IPausable
{
    #region Serialized Fields
    [Header("References")]
    [SerializeField] private TurretBlueprint turretBlueprint;
    [SerializeField] private GameObject currentProjectile;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject healthBarPrefab;

    [Header("Targeting Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Firing Pattern Settings")]
    [SerializeField] private int projectilesPerSalve = 3;
    [SerializeField] private float delayBetweenSalveProjectiles = 0.1f;
    #endregion

    #region Public Properties
    // Public accessor for turretBlueprint (read-only)
    public TurretBlueprint TurretBlueprint
    {
        get => turretBlueprint;
        set => turretBlueprint = value; // Add setter
    }
    // Public accessor for firePoint if needed elsewhere
    public Transform FirePoint => firePoint;
    #endregion

    #region Private Fields
    private TurretStats stats;
    private float currentShotCooldown;
    private Transform targetEnemy;
    private bool isPaused;

    // Firing pattern state
    private FiringPattern currentFiringPattern;
    private bool salveInProgress;
    private bool burstInProgress;
    private bool sprayActive;
    private Coroutine sprayCoroutine;

    // Managers
    private TurretGlobalModifierManager globalModifierManager;
    private TurretUpgradeChoiceManager upgradeManager;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        InitializeManagers();
        stats = GetComponent<TurretStats>();
    }

    private void Start()
    {
        InitializeProjectileType();
        InitializeFiringPattern();
        AttachHealthBar();
    }

    private void OnEnable()
    {
        GameManager.Instance?.RegisterPausable(this);
    }

    private void OnDisable()
    {
        GameManager.Instance?.UnregisterPausable(this);
        StopSpray();
    }

    private void Update()
    {
        if (isPaused) return;

        UpdateCooldown();

        if (CanFire())
        {
            FindTarget();
            if (targetEnemy != null)
            {
                ExecuteFiringPattern();
            }
        }
    }
    #endregion

    #region Initialization
    private void InitializeManagers()
    {
        globalModifierManager = TurretGlobalModifierManager.Instance;
        upgradeManager = TurretUpgradeChoiceManager.Instance;
    }

    private void InitializeProjectileType()
    {
        if (turretBlueprint != null && currentProjectile == null)
        {
            currentProjectile = turretBlueprint.turretProjectile;
        }
    }

    private void InitializeFiringPattern()
    {
        currentFiringPattern = turretBlueprint?.firingPattern ?? FiringPattern.SingleShot;
        delayBetweenSalveProjectiles = turretBlueprint?.delayBetweenSalveProjectiles ?? 0.1f;
        projectilesPerSalve = turretBlueprint?.projectilesPerSalve ?? projectilesPerSalve;
    }

    private void AttachHealthBar()
    {
        TurretHealth health = GetComponent<TurretHealth>();
        if (health != null)
        {
            health.AttachHealthBar(healthBarPrefab, new Vector3(0, 1.5f, 0));
        }
    }
    #endregion

    #region Pause System
    public void OnPause() => isPaused = true;
    public void OnResume() => isPaused = false;
    #endregion

    #region Targeting
    private void FindTarget()
    {
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(
            transform.position,
            stats.currentAttackRange,
            enemyLayer);

        Transform closestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Collider2D enemyCollider in enemiesInRange)
        {
            EnemyBehaviour enemy = enemyCollider.GetComponent<EnemyBehaviour>();
            if (enemy == null) continue;

            if (!HasLineOfSight(enemy.transform)) continue;

            float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                closestEnemy = enemy.transform;
            }
        }

        targetEnemy = closestEnemy;
    }

    private List<Transform> GetEnemiesInRange()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            stats.currentAttackRange,
            enemyLayer);

        List<Transform> enemies = new List<Transform>();
        foreach (Collider2D hit in hits)
        {
            EnemyBehaviour enemy = hit.GetComponent<EnemyBehaviour>();
            if (enemy != null)
            {
                enemies.Add(enemy.transform);
            }
        }
        return enemies;
    }

    private Transform GetNearestTarget(List<Transform> targets)
    {
        if (targets == null || targets.Count == 0) return null;

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

    private bool HasLineOfSight(Transform target)
    {
        if (target == null) return false;

        Vector2 origin = firePoint.position;
        Vector2 direction = (target.position - firePoint.position);
        float distance = direction.magnitude;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            direction.normalized,
            distance,
            obstacleLayer);

        Debug.DrawRay(origin, direction.normalized * distance,
            hit.collider == null ? Color.green : Color.red);

        return hit.collider == null;
    }
    #endregion

    #region Firing Logic
    private bool CanFire()
    {
        return currentShotCooldown <= 0f;
    }

    private void UpdateCooldown()
    {
        if (currentShotCooldown > 0f)
        {
            currentShotCooldown -= Time.deltaTime;
        }
    }

    private void ExecuteFiringPattern()
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

    private void ResetFiringCooldown()
    {
        currentShotCooldown = stats.currentShotInterval;
    }
    #endregion

    #region Projectile Creation
    private GameObject CreateProjectile(Vector2 direction)
    {
        if (currentProjectile == null || firePoint == null)
            return null;

        GameObject projectileObj = Instantiate(
            currentProjectile,
            firePoint.position,
            Quaternion.identity);

        ProjectileBehaviour projectile = projectileObj.GetComponent<ProjectileBehaviour>();
        Rigidbody2D rb = projectileObj.GetComponent<Rigidbody2D>();

        if (projectile == null || rb == null)
        {
            Debug.LogWarning("Projectile prefab is missing required components.");
            Destroy(projectileObj);
            return null;
        }

        // Initialize projectile
        projectile.SetOwner(gameObject, stats.currentAttackDamage);
        projectile.knockbackStrength = stats.currentKnockbackStrength;
        projectile.knockbackDuration = stats.currentKnockbackDuration;
        projectile.InitializePiercing(stats.currentProjectilePierce);

        rb.linearVelocity = direction * stats.currentProjectileSpeed;

        Destroy(projectileObj, 5f);

        SoundManager.Instance?.PlayTowerShoot();

        return projectileObj;
    }

    private void ShootProjectileAt(Transform target)
    {
        if (target == null || firePoint == null) return;

        Vector2 direction = (target.position - firePoint.position).normalized;
        CreateProjectile(direction);
    }
    #endregion

    #region Firing Patterns
    private IEnumerator FireSalveWithCooldown()
    {
        salveInProgress = true;

        List<Transform> targets = GetEnemiesInRange();
        if (targets.Count == 0)
        {
            salveInProgress = false;
            yield break;
        }

        // Shuffle targets
        ShuffleList(targets);

        int targetIndex = 0;

        for (int i = 0; i < projectilesPerSalve; i++)
        {
            yield return new WaitWhile(() => isPaused);

            // Clean up targets
            targets.RemoveAll(t => t == null ||
                Vector2.Distance(transform.position, t.position) > stats.currentAttackRange);

            if (targets.Count == 0) break;

            Transform currentTarget = targets[targetIndex % targets.Count];
            if (currentTarget != null && HasLineOfSight(currentTarget))
            {
                ShootProjectileAt(currentTarget);
            }
            targetIndex++;

            yield return new WaitForSeconds(delayBetweenSalveProjectiles);
        }

        ResetFiringCooldown();
        salveInProgress = false;
    }

    private IEnumerator FireBurstWithCooldown()
    {
        burstInProgress = true;
        int shotsFired = 0;

        while (shotsFired < turretBlueprint.burstCount)
        {
            yield return new WaitWhile(() => isPaused);

            FindTarget();
            if (targetEnemy != null && HasLineOfSight(targetEnemy))
            {
                ShootProjectileAt(targetEnemy);
            }

            shotsFired++;

            if (shotsFired < turretBlueprint.burstCount)
            {
                yield return new WaitForSeconds(turretBlueprint.burstDelay);
            }
        }

        ResetFiringCooldown();
        burstInProgress = false;
    }

    private IEnumerator FireSprayShot()
    {
        sprayActive = true;
        float fireInterval = 1f / turretBlueprint.sprayRate;

        while (sprayActive)
        {
            yield return new WaitWhile(() => isPaused);

            List<Transform> targets = GetEnemiesInRange();
            if (targets.Count > 0)
            {
                Transform nearestTarget = GetNearestTarget(targets);
                if (nearestTarget != null && HasLineOfSight(nearestTarget))
                {
                    FireSprayShotAt(nearestTarget);
                }
            }

            yield return new WaitForSeconds(fireInterval);
        }
    }

    // Specific pattern implementations
    private void FireScatterShot(Transform target)
    {
        if (target == null || firePoint == null) return;

        Vector2 baseDirection = (target.position - firePoint.position).normalized;
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;

        for (int i = 0; i < turretBlueprint.scatterCount; i++)
        {
            float angleOffset = UnityEngine.Random.Range(-turretBlueprint.scatterAngle / 2, turretBlueprint.scatterAngle / 2);
            float angle = baseAngle + angleOffset;

            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            CreateProjectile(direction);
        }
    }

    private void FireChainLightning(Transform firstTarget)
    {
        if (firstTarget == null || firePoint == null) return;

        GameObject projectileObj = CreateProjectile((firstTarget.position - firePoint.position).normalized);
        if (projectileObj != null)
        {
            ProjectileBehaviour projectile = projectileObj.GetComponent<ProjectileBehaviour>();
            if (projectile != null)
            {
                projectile.SetOwner(gameObject, stats.currentAttackDamage);
                projectile.chainBounceCount = turretBlueprint.chainBounceCount;
                projectile.chainBounceRange = turretBlueprint.chainBounceRange;
                projectile.SetChainTarget(firstTarget);
            }
        }
    }

    private void FireHomingProjectile(Transform target)
    {
        if (target == null || firePoint == null) return;

        GameObject projectileObj = CreateProjectile((target.position - firePoint.position).normalized);
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

    private void FireAOEProjectile(Transform target)
    {
        if (target == null || firePoint == null) return;

        GameObject projectileObj = CreateProjectile((target.position - firePoint.position).normalized);
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

    private void FireSprayShotAt(Transform target)
    {
        if (target == null || firePoint == null) return;

        Vector2 baseDirection = (target.position - firePoint.position).normalized;
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
        float angleOffset = UnityEngine.Random.Range(-turretBlueprint.sprayAngle / 2, turretBlueprint.sprayAngle / 2);
        float angle = baseAngle + angleOffset;

        Vector2 direction = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        );

        CreateProjectile(direction);
    }
    #endregion

    #region Public Methods
    public void SetFiringPattern(FiringPattern pattern)
    {
        currentFiringPattern = pattern;
        if (pattern != FiringPattern.SprayShot)
        {
            StopSpray();
        }
    }

    public void StopSpray()
    {
        sprayActive = false;
        if (sprayCoroutine != null)
        {
            StopCoroutine(sprayCoroutine);
            sprayCoroutine = null;
        }
    }

    public TurretType GetTurretType()
    {
        return turretBlueprint?.turretType ?? TurretType.ArcherTower;
    }
    public void SetTurretBlueprint(TurretBlueprint blueprint)
    {
        turretBlueprint = blueprint;
        InitializeProjectileType();
        InitializeFiringPattern();
    }
    #endregion

    #region Helpers
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
    #endregion

    #region Debug
    private void OnDrawGizmosSelected()
    {
        if (stats == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stats.currentAttackRange);
    }
    #endregion
}
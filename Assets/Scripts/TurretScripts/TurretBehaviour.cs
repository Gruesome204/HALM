using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBehaviour : MonoBehaviour, IPausable
{
    [Header("References")]
    public TurretBlueprint turretBlueprint;
    public GameObject currentProjectileType;
    public Transform firePoint;
    [SerializeField] private GameObject healthBarPrefab;

    [Header("Values")]

    public float currentAttackDamage;
    public float currentFireRate;
    public float currentProjectileSpeed;
    public float currentAttackRange;

    public float currentFireCountdown;
    public float currentKnockbackStrength;
    public float currentKnockbackDuration;

    private Transform targetEnemy;

    [Header("Targeting Settings")] // Optional: for better organization in Inspector
    public LayerMask enemyLayer; // New variable to select the enemy layer

    private TurretBlueprint.FiringPattern currentFiringPattern;
    private bool salveInProgress;
    public int projectilesPerSalve; // Number of projectiles in a salve
    private float delayBetweenSalveProjectiles; // Delay between each projectile in a salve

    private bool isPaused;


    private void OnEnable()
    {
        GameManager.Instance?.RegisterPausable(this);
        if (TurretGlobalModifierManager.Instance != null)
            TurretGlobalModifierManager.Instance.OnModifiersChanged += RecalculateStats;
        RecalculateStats();
    }

    private void OnDisable()
    {
        GameManager.Instance?.UnregisterPausable(this);
        if (TurretGlobalModifierManager.Instance != null)
            TurretGlobalModifierManager.Instance.OnModifiersChanged -= RecalculateStats;
    }

    // Pause system
    public void OnPause() => isPaused = true;
    public void OnResume() => isPaused = false;


    void Start()
    {
        currentFiringPattern = turretBlueprint?.firingPattern ?? TurretBlueprint.FiringPattern.SingleShot;
        delayBetweenSalveProjectiles = turretBlueprint?.delayBetweenSalveProjectiles ?? 0.1f;

        var health = GetComponent<TurretHealth>();
        if (health != null)
        {
            health.AttachHealthBar(healthBarPrefab, new Vector3(0, 1.5f, 0));
        }

    }
    public void RecalculateStats()
    {
        if (turretBlueprint == null) return;
        var global = TurretGlobalModifierManager.Instance;

        float globalFireRateMult = global?.globalFireRateMultiplier ?? 1f;
        float globalDamageMult = global?.globalDamageMultiplier ?? 1f;
        float globalProjectileSpeed = Mathf.Max(0.01f, global?.globalProjectileSpeed ?? 1f);
        int globalExtraProjectiles = global?.globalProjectilesPerSalve ?? 0;

        currentAttackDamage = turretBlueprint.baseAttackDamage * globalDamageMult;
        currentFireRate = turretBlueprint.baseFireRate * globalFireRateMult;
        currentFireCountdown = turretBlueprint.BaseFireCountdown / globalFireRateMult;
        currentAttackRange = turretBlueprint.baseAttackRange;

        currentProjectileSpeed =
            turretBlueprint.baseProjectileSpeed *
            TurretUpgradeChoiceManager.Instance.GetProjectileSpeedMultiplier(turretBlueprint.turretType) *
            globalProjectileSpeed;

        int upgradeExtraProjectiles =
            TurretUpgradeChoiceManager.Instance.GetProjectilesPerSalve(turretBlueprint.turretType);

        projectilesPerSalve =
            turretBlueprint.projectilesPerSalve
            + globalExtraProjectiles
            + upgradeExtraProjectiles;

        currentProjectileType = turretBlueprint.turretProjectileType;

        currentKnockbackStrength = turretBlueprint.baseKnockbackStrength;
        currentKnockbackDuration = turretBlueprint.baseKnockbackDuration;
    }

    void Update()
    {
        if (isPaused) return;

        currentFireCountdown -= Time.deltaTime;

        if (currentFireCountdown > 0f || salveInProgress)
            return;
        FindTarget();
        if (targetEnemy == null)
            return;

        Fire();
    }

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
        if (salveInProgress)
            yield break;

        salveInProgress = true;

        float delay = delayBetweenSalveProjectiles;

        // snapshot of all enemies in range at the moment the salve starts
        List<Transform> targets = GetEnemiesInRange();

        if (targets.Count == 0)
        {
            salveInProgress = false;
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


        salveInProgress = false;
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
                StartCoroutine(ShootFireSalve());
                ResetFiringCooldown();
                break;
        }
    }

    void ShootProjectileAt(Transform target)
    {
        if (currentProjectileType == null || target == null || firePoint == null)
            return;

    TurretModifier modifier =
    TurretUpgradeChoiceManager.Instance.GetCombinedModifier(turretBlueprint.turretType);

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

        projectile.SetOwner(gameObject, currentAttackDamage);
        projectile.knockbackStrength = currentKnockbackStrength;
        projectile.knockbackDuration = currentKnockbackDuration;

        int pierceCount = turretBlueprint.baseProjectilePierceCount + modifier.piercingHits;
        projectile.InitializePiercing(pierceCount);
        Vector2 direction = (target.position - firePoint.position).normalized;
        rb.linearVelocity = direction * currentProjectileSpeed;

        Destroy(projectileObj, 5f);

        // <-- Play shooting sound
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayShoot();
    }

    private void ResetFiringCooldown()
    {
        currentFireCountdown = 1f / currentFireRate;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, currentAttackRange);
    }

}
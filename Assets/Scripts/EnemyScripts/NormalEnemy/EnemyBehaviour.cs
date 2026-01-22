using System.Linq;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour, IPausable
{
    #region References
    [Header("References")]
    protected EnemyStats stats;
    protected EnemyHealth health;
    protected EnemyMovement movement;
    protected EnemyKnockback knockback;
    protected EnemyAttack attack;
    protected EnemyAbilityBehaviour abilityBehaviour;
    protected EnemyAnimator enemyAnimator;
    #endregion

    #region Targeting
    [Header("Targeting")]
    [SerializeField] private float groupAggroRadius = 20f;
    [SerializeField] private float loseAggroMultiplier = 1.5f;

    private static GameObject cachedPlayer;
    public GameObject target;
    private bool isAggroed;
    #endregion

    #region Ability & Combat Settings
    [Header("Ability Settings")]
    [Tooltip("Time between ability usage attempts (seconds).")]
    [SerializeField] private float abilityCheckInterval = 1.0f;

    private float nextAttackTime = 0.5f;
    private float nextAbilityTime = 0f;
    private bool isPaused;
    [SerializeField] private float stopBuffer = 0.2f;
    #endregion


    #region Turret Damage Tracking
    private bool aggroedByTurret = false;
    private float turretAggroDuration = 5f; // seconds to keep aggro after turret hit
    private float turretAggroTimer = 0f;
    #endregion

    #region Unity Callbacks
    protected virtual void Awake()
    {
        // Cache components
        stats = GetComponent<EnemyStats>();
        health = GetComponent<EnemyHealth>();
        movement = GetComponent<EnemyMovement>();
        knockback = GetComponent<EnemyKnockback>();
        attack = GetComponent<EnemyAttack>();
        abilityBehaviour = GetComponent<EnemyAbilityBehaviour>();
        enemyAnimator = GetComponent<EnemyAnimator>();

        stats.Initialize();

        // Subscribe to health events
        health.OnDeath += HandleDeath;
        health.OnDamaged += HandleDamaged;
    }

    private void Start()
    {
        GameManager.Instance?.RegisterPausable(this);
        //Auto register in enemySpawnManager
        EnemySpawnManager.Instance.RegisterEnemy(this.gameObject);
        if (GameManager.Instance == null)
            Debug.LogWarning("GameManager not ready yet, EnemyBehaviour won't receive pause events");
    }

    private void OnDisable() => GameManager.Instance?.UnregisterPausable(this);

    private void Update()
    {
        if (isPaused || (knockback != null && knockback.IsKnockedBack))
            return;

        if (aggroedByTurret)
        {
            turretAggroTimer -= Time.deltaTime;
            if (turretAggroTimer <= 0f)
                aggroedByTurret = false;
        }

        CheckProximityAggro();


        if (target == null) return;

        HandleMovementTarget(target);
        TryAttack(target);
        TryUseAbilities(target);

        CheckLoseAggro();
    }
    #endregion

    #region Movement & Targeting
    private void HandleMovementTarget(GameObject target)
    {
        float distance = Vector2.Distance(transform.position, target.transform.position);

        if (distance <= stats.currentAttackRange - stopBuffer)
            movement.Stop();
        else
            SetMovementTarget(target);
    }

    private void SetMovementTarget(GameObject newTarget)
    {
        if (movement.target == newTarget) return;
        movement.target = newTarget;
    }

    public GameObject AcquirePlayerTarget()
    {
        // Always find the player, do not rely on static
        if (target == null || !target.activeInHierarchy)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player;
                cachedPlayer = player; // keep cached for other enemies
            }
            else
            {
                Debug.LogWarning($"{name}: Player not found in scene!");
            }
        }
        return target;
    }
    private void CheckProximityAggro()
    {
        AcquirePlayerTarget();
        // Ensure we have a target
        if (target == null) return;

        float distance = Vector2.Distance(transform.position, target.transform.position);
        if (target == null)
            return;
        // If within detection range OR aggroed by turret, set aggro
        if (!isAggroed && (distance <= stats.currentDetectionRange || aggroedByTurret))
        {
            SetAggro(target);
        }

        // Alert nearby enemies only if this enemy is aggroed
        if (isAggroed)
            AlertNearbyEnemies();
    }


    private void AlertNearbyEnemies()
    {
        if (target == null) return;

        // Detect all colliders within radius (temporarily without LayerMask for testing)
        var hits = Physics2D.OverlapCircleAll(transform.position, groupAggroRadius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out EnemyBehaviour enemy) && enemy != this)
            {   
                // Only alert if the other enemy is not already aggroed or has no target
                if (!enemy.isAggroed || enemy.target == null)
                {
                    enemy.SetAggro(target);
                }
            }
        }
    }

    private void SetAggro(GameObject newTarget)
    {
        if (newTarget == null) return;

        isAggroed = true;
        movement.isAggroed = true;
        target = newTarget;

        // Sync movement target
        movement.target = newTarget;

        // Sync ability target
        abilityBehaviour?.SetTarget(newTarget);

       // Debug.Log($"{name} set aggro on {newTarget.name}");
    }


    private void ClearAggro()
    {
        isAggroed = false;
        movement.Stop();
        movement.isAggroed = false;
        abilityBehaviour?.SetTarget(null);
    }

    private void CheckLoseAggro()
    {
        if (!isAggroed || target == null || aggroedByTurret) return;

        float loseDistance = stats.currentDetectionRange * loseAggroMultiplier;
        float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);

        if (distanceToTarget > loseDistance)
        {
            ClearAggro();
        }
    }
    #endregion

    #region Combat
    private void TryAttack(GameObject target)
    {
        if (attack == null || target == null) return;

        float attackCooldown = 1f / Mathf.Max(0.01f, stats.currentAttackSpeed);
        if (Time.time < nextAttackTime) return;

        float distance = Vector2.Distance(transform.position, target.transform.position);
        if (distance <= stats.currentAttackRange)
        {
            attack.PerformAttack(target, isPaused);
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void TryUseAbilities(GameObject target)
    {
        if (abilityBehaviour == null || target == null || AbilityManager.Instance == null) return;
        if (Time.time < nextAbilityTime) return;

        var abilities = AbilityManager.Instance.GetAbilities(gameObject);
        if (abilities == null || abilities.Count == 0) return;

        float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);

        var usable = abilities
            .Select((a, i) => new { ability = a, index = i })
            .Where(x => x.ability != null && x.ability.CanUse(gameObject, target)
                                        && distanceToTarget <= x.ability.ability.range)
            .OrderByDescending(x => x.ability.ability.priority)
            .FirstOrDefault();

        if (usable == null) return;

        if (AbilityManager.Instance.TryUseAbility(gameObject, usable.index, target))
            nextAbilityTime = Time.time + abilityCheckInterval;
    }
    #endregion

    #region Event Handlers
    protected virtual void HandleDamaged(DamageData damageData, KnockbackData knockbackData)
    {
        if (isPaused) return;
        enemyAnimator?.PlayHit();

        // Ensure we have a valid player target
        if (cachedPlayer == null)
            cachedPlayer = GameObject.FindGameObjectWithTag("Player");

        if (cachedPlayer == null)
        {
            Debug.LogWarning($"{name} took damage but no player found!");
            return;
        }

        // Check if damage came from a turret
        if (damageData.source != null && damageData.source.TryGetComponent<TurretLevelBehaviour>(out _))
        {
            aggroedByTurret = true;
            turretAggroTimer = turretAggroDuration;
        }

        SetAggro(target);
        AlertNearbyEnemies();
        
    }

    private void HandleDeath(EnemyHealth enemyHealth, DamageData damageData)
    {
        enemyAnimator?.PlayDeath();
        DropResources();

        if (damageData.source != null &&
            damageData.source.TryGetComponent<TurretBehaviour>(out var turretBehaviour))
        {
            TurretBlueprint blueprint = turretBehaviour.turretBlueprint;

            if (blueprint != null)
            {
                TurretLevelManager.Instance.AddXP(
                    blueprint.turretType,
                    stats.currentExperienceYield
                );

                Debug.Log($"XP added to {blueprint.turretType}: {stats.currentExperienceYield}");
            }
        }

        EnemySpawnManager.Instance.UnregisterEnemy(gameObject);
        Destroy(gameObject);
    }
    #endregion

    #region Resource Handling
    private void AddResourceToGameData(ResourceType type, int amount)
    {
        var gameDataSO = GameManager.Instance.gameDataSO;

        switch (type)
        {
            case ResourceType.Currency:
                gameDataSO.gameCurrency += amount;
                break;
            case ResourceType.Wood:
                gameDataSO.woodResource += amount;
                break;
            case ResourceType.Stone:
                gameDataSO.steinResource += amount;
                break;
            case ResourceType.Metal:
                gameDataSO.metallResource += amount;
                break;
            case ResourceType.Pulver:
                gameDataSO.pulverResource += amount;
                break;
            default:
                Debug.LogWarning($"Unhandled resource type: {type}");
                break;
        }
    }

    private void DropResources()
    {
        var drops = stats.baseStats.resourceDrops;
        if (drops == null || drops.Length == 0) return;

        foreach (var drop in drops)
        {
            float roll = UnityEngine.Random.Range(0f, 1f);
            if (roll <= drop.dropChance)
            {
                int amount = UnityEngine.Random.Range(drop.minAmount, drop.maxAmount + 1);
                if (amount > 0)
                {
                    GameManager.Instance.gameDataSO.AddResource(drop.resourceType, amount);
                    Debug.Log($"Dropped {amount} {drop.resourceType} (roll: {roll})");
                }
            }
        }

    }
    #endregion

    #region Pause Handling
    public void OnPause()
    {
        isPaused = true;
        movement.SetPaused(true);
        abilityBehaviour?.OnPause();
        GetComponent<Animator>().enabled = false;
    }

    public void OnResume()
    {
        isPaused = false;
        movement.SetPaused(false);
        abilityBehaviour?.OnResume();
        nextAttackTime = Time.time;
        nextAbilityTime = Time.time;
        GetComponent<Animator>().enabled = true;
    }
    #endregion
}
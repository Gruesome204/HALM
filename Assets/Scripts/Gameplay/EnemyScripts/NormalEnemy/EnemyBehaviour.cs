using System.Linq;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour, IPausable
{
    #region Serialized Fields
    [Header("References")]
    [SerializeField] private float groupAggroRadius = 20f;
    [SerializeField] private float loseAggroMultiplier = 1.5f;

    [Header("Ability Settings")]
    [SerializeField] private float abilityCheckInterval = 1.0f;
    [SerializeField] private float stopBuffer = 0.2f;

    [Header("Turret Aggro")]
    [SerializeField] private float turretAggroDuration = 5f;
    #endregion

    #region Protected Fields (Changed from private to protected)
    // Components - now accessible to derived classes
    protected EnemyStats stats;
    protected EnemyHealth health;
    protected EnemyMovement movement;
    protected EnemyKnockback knockback;
    protected EnemyAttack attack;
    protected EnemyAbilityBehaviour abilityBehaviour;
    protected EnemyAnimator enemyAnimator;

    // Targeting
    protected static GameObject cachedPlayer;
    protected bool isAggroed;
    protected bool isPaused;

    // Turret Aggro
    protected bool aggroedByTurret;
    protected float turretAggroTimer;

    // Combat Timing
    protected float nextAttackTime = 0.5f;
    protected float nextAbilityTime;
    #endregion

    #region Public Properties
    public GameObject target { get; protected set; }

    // Add public properties for components that derived classes might need
    public EnemyStats Stats => stats;
    public EnemyHealth Health => health;
    public EnemyMovement Movement => movement;
    public EnemyAbilityBehaviour AbilityBehaviour => abilityBehaviour;
    #endregion

    #region Unity Callbacks
    protected virtual void Awake()
    {
        CacheComponents();
        stats.Initialize();

        health.OnDeath += HandleDeath;
        health.OnDamaged += HandleDamaged;
    }

    private void Start()
    {
        GameManager.Instance?.RegisterPausable(this);
        EnemySpawnManager.Instance?.RegisterEnemy(gameObject);

        if (GameManager.Instance == null)
            Debug.LogWarning("GameManager not ready yet, EnemyBehaviour won't receive pause events");
    }

    private void OnDisable()
    {
        GameManager.Instance?.UnregisterPausable(this);
    }

    private void Update()
    {
        if (ShouldSkipUpdate()) return;

        UpdateTurretAggroTimer();
        CheckProximityAggro();

        if (target == null) return;

        HandleMovementTarget(target);
        TryAttack(target);
        TryUseAbilities(target);
        CheckLoseAggro();
    }
    #endregion

    #region Public Methods
    public GameObject AcquirePlayerTarget()
    {
        if (target == null || !target.activeInHierarchy)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player;
                cachedPlayer = player;
            }
            else
            {
                Debug.LogWarning($"{name}: Player not found in scene!");
            }
        }
        return target;
    }

    public bool IsAggroedByTurret() => aggroedByTurret;

    public virtual void SetAggro(GameObject newTarget)
    {
        if (newTarget == null) return;

        isAggroed = true;
        movement.isAggroed = true;
        target = newTarget;
        movement.target = newTarget;
        abilityBehaviour?.SetTarget(newTarget);
    }

    public virtual void ClearAggro()
    {
        isAggroed = false;
        movement.Stop();
        movement.isAggroed = false;
        abilityBehaviour?.SetTarget(null);
    }

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

        if (aggroedByTurret && turretAggroTimer <= 0)
            aggroedByTurret = false;
    }
    #endregion

    #region Protected Methods - Initialization
    protected virtual void CacheComponents()
    {
        stats = GetComponent<EnemyStats>();
        health = GetComponent<EnemyHealth>();
        movement = GetComponent<EnemyMovement>();
        knockback = GetComponent<EnemyKnockback>();
        attack = GetComponent<EnemyAttack>();
        abilityBehaviour = GetComponent<EnemyAbilityBehaviour>();
        enemyAnimator = GetComponent<EnemyAnimator>();
    }
    #endregion

    #region Protected Methods - Update Helpers
    protected virtual bool ShouldSkipUpdate()
    {
        return isPaused || (knockback != null && knockback.IsKnockedBack);
    }

    protected virtual void UpdateTurretAggroTimer()
    {
        if (aggroedByTurret)
        {
            turretAggroTimer -= Time.deltaTime;
            if (turretAggroTimer <= 0f)
                aggroedByTurret = false;
        }
    }
    #endregion

    #region Protected Methods - Targeting & Aggro
    protected virtual void CheckProximityAggro()
    {
        AcquirePlayerTarget();
        if (target == null) return;

        float distance = Vector2.Distance(transform.position, target.transform.position);

        if (!isAggroed && (distance <= stats.currentDetectionRange || aggroedByTurret))
            SetAggro(target);

        if (isAggroed)
            AlertNearbyEnemies();
    }

    protected virtual void AlertNearbyEnemies()
    {
        if (target == null) return;

        var hits = Physics2D.OverlapCircleAll(transform.position, groupAggroRadius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out EnemyBehaviour enemy) && enemy != this)
            {
                if (!enemy.isAggroed || enemy.target == null)
                    enemy.SetAggro(target);
            }
        }
    }

    protected virtual void CheckLoseAggro()
    {
        if (!isAggroed || target == null) return;

        if (aggroedByTurret)
        {
            if (turretAggroTimer <= 0f)
            {
                aggroedByTurret = false;
                float distanceToPlayer = Vector2.Distance(transform.position, target.transform.position);
                if (distanceToPlayer > stats.currentDetectionRange * loseAggroMultiplier)
                    ClearAggro();
            }
            return;
        }

        float loseDistance = stats.currentDetectionRange * loseAggroMultiplier;
        float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);

        if (distanceToTarget > loseDistance)
            ClearAggro();
    }
    #endregion

    #region Protected Methods - Movement
    protected virtual void HandleMovementTarget(GameObject target)
    {
        float distance = Vector2.Distance(transform.position, target.transform.position);

        if (distance <= stats.currentAttackRange - stopBuffer)
            movement.Stop();
        else
            SetMovementTarget(target);
    }

    protected virtual void SetMovementTarget(GameObject newTarget)
    {
        if (movement.target == newTarget) return;
        movement.target = newTarget;
    }
    #endregion

    #region Protected Methods - Combat
    protected virtual void TryAttack(GameObject target)
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

    protected virtual void TryUseAbilities(GameObject target)
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

    #region Protected Methods - Event Handlers
    protected virtual void HandleDamaged(DamageData damageData, KnockbackData knockbackData)
    {
        if (isPaused) return;
        enemyAnimator?.PlayHit();

        if (cachedPlayer == null)
            cachedPlayer = GameObject.FindGameObjectWithTag("Player");

        if (cachedPlayer == null)
        {
            Debug.LogWarning($"{name} took damage but no player found!");
            return;
        }

        if (IsTurretDamage(damageData))
        {
            HandleTurretDamage();
        }
        else
        {
            SetAggro(cachedPlayer);
            AlertNearbyEnemies();
        }
    }

    protected virtual bool IsTurretDamage(DamageData damageData)
    {
        return damageData.source != null && damageData.source.TryGetComponent<TurretLevelBehaviour>(out _);
    }

    protected virtual void HandleTurretDamage()
    {
        aggroedByTurret = true;
        turretAggroTimer = turretAggroDuration;

        SetAggro(cachedPlayer);
        movement.isAggroed = true;
        movement.ForceAggroOnPlayer(cachedPlayer);
        AlertNearbyEnemies();
    }

    protected virtual void HandleDeath(EnemyHealth enemyHealth, DamageData damageData)
    {
        enemyAnimator?.PlayDeath();
        DropResources();
        AddXPToTurret(damageData);
        EnemySpawnManager.Instance?.UnregisterEnemy(gameObject);
        Destroy(gameObject);
    }

    protected virtual void AddXPToTurret(DamageData damageData)
    {
        if (damageData.source == null) return;

        if (damageData.source.TryGetComponent<TurretBehaviour>(out var turretBehaviour))
        {
            TurretBlueprint blueprint = turretBehaviour.TurretBlueprint;
            if (blueprint != null)
            {
                TurretLevelManager.Instance?.AddXP(blueprint.turretType, stats.currentExperienceYield);
                Debug.Log($"XP added to {blueprint.turretType}: {stats.currentExperienceYield}");
            }
        }
    }
    #endregion

    #region Protected Methods - Resources
    protected virtual void DropResources()
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
}
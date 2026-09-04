using System.Collections.Generic;
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

    [Header("Targeting Settings")]
    [SerializeField] private EnemyTargetingSettings targetingSettings = new();
    #endregion

    #region Protected Fields (changed back from private to protected)
    protected EnemyStats stats;
    protected EnemyHealth health;
    protected EnemyMovement movement;
    protected EnemyKnockback knockback;
    protected EnemyAttack attack;
    protected EnemyAbilityBehaviour abilityBehaviour;
    protected EnemyAnimator enemyAnimator;

    protected static GameObject cachedPlayer;
    protected bool isAggroed;
    protected bool isPaused;

    protected bool aggroedByTurret;
    protected float turretAggroTimer;

    protected float nextAttackTime;
    protected float nextAbilityTime;
    protected float nextTargetSwitchTime;
    protected float turretSearchCooldown;

    protected GameObject currentTurretTarget;
    protected readonly List<GameObject> cachedTurrets = new();
    #endregion

    #region Public Properties
    public GameObject Target { get; protected set; }
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

        targetingSettings ??= new EnemyTargetingSettings();
    }

    protected virtual void Start()  // Changed from private to protected virtual
    {
        GameManager.Instance?.RegisterPausable(this);
        EnemySpawnManager.Instance?.RegisterEnemy(gameObject);

        if (GameManager.Instance == null)
            Debug.LogWarning("GameManager not ready yet, EnemyBehaviour won't receive pause events");

        if (targetingSettings.targetTurrets)
            CacheTurrets();
    }

    protected virtual void OnDisable()  // Changed from private to protected virtual
    {
        GameManager.Instance?.UnregisterPausable(this);
    }

    private void Update()
    {
        if (ShouldSkipUpdate()) return;

        UpdateTurretAggroTimer();
        CheckProximityAggro();
        FindBestTarget();

        if (Target == null) return;

        HandleMovementTarget(Target);
        TryAttack(Target);
        TryUseAbilities(Target);
        CheckLoseAggro();
    }
    #endregion

    #region Public Methods
    public GameObject AcquirePlayerTarget() => GetPlayerTarget();

    public bool IsAggroedByTurret() => aggroedByTurret;

    public virtual void SetAggro(GameObject newTarget)
    {
        if (newTarget == null) return;

        isAggroed = true;
        movement.isAggroed = true;
        Target = newTarget;
        movement.target = newTarget;
        abilityBehaviour?.SetTarget(newTarget);
    }

    public virtual void ClearAggro()
    {
        isAggroed = false;
        movement.Stop();
        movement.isAggroed = false;
        abilityBehaviour?.SetTarget(null);
        currentTurretTarget = null;
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

    #region Initialization
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

    #region Update Helpers
    private bool ShouldSkipUpdate() => isPaused || knockback != null && knockback.IsKnockedBack;

    private void UpdateTurretAggroTimer()
    {
        if (!aggroedByTurret) return;

        turretAggroTimer -= Time.deltaTime;
        if (turretAggroTimer <= 0f)
            aggroedByTurret = false;
    }
    #endregion

    #region Targeting & Aggro
    private void CheckProximityAggro()
    {
        if (targetingSettings == null || !targetingSettings.targetPlayer) return;

        var player = GetPlayerTarget();
        if (player == null) return;

        var distance = Vector2.Distance(transform.position, player.transform.position);
        var isInRange = distance <= stats.currentDetectionRange;

        if (!isAggroed && (isInRange || aggroedByTurret))
            SetAggro(player);

        if (isAggroed)
            AlertNearbyEnemies();
    }

    private void AlertNearbyEnemies()
    {
        if (Target == null) return;

        var hits = Physics2D.OverlapCircleAll(transform.position, groupAggroRadius);
        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent(out EnemyBehaviour enemy) || enemy == this) continue;
            if (!enemy.isAggroed || enemy.Target == null)
                enemy.SetAggro(Target);
        }
    }

    private void CheckLoseAggro()
    {
        if (!isAggroed || Target == null) return;

        if (aggroedByTurret)
        {
            if (turretAggroTimer <= 0f)
            {
                aggroedByTurret = false;
                if (IsTargetOutOfRange(Target))
                    ClearAggro();
            }
            return;
        }

        if (IsTargetOutOfRange(Target))
            ClearAggro();
    }

    private bool IsTargetOutOfRange(GameObject target)
    {
        var loseDistance = stats.currentDetectionRange * loseAggroMultiplier;
        var distance = Vector2.Distance(transform.position, target.transform.position);
        return distance > loseDistance;
    }
    #endregion

    #region Turret Management
    private void CacheTurrets()
    {
        cachedTurrets.Clear();
        var turrets = FindObjectsOfType<TurretBehaviour>();

        foreach (var turret in turrets)
        {
            if (turret != null && turret.gameObject.activeInHierarchy)
                cachedTurrets.Add(turret.gameObject);
        }

        Debug.Log($"Cached {cachedTurrets.Count} turrets for targeting");
    }

    private bool IsValidTurretTarget(GameObject turretObj)
    {
        if (turretObj == null) return false;

        if (((1 << turretObj.layer) & targetingSettings.turretLayerMask) == 0)
            return false;

        if (targetingSettings.targetOnlyActiveTurrets && !turretObj.activeInHierarchy)
            return false;

        var turretHealth = turretObj.GetComponent<EnemyHealth>();
        if (turretHealth != null && turretHealth.CurrentHealth <= 0)
            return false;

        return true;
    }
    #endregion

    #region Target Selection
    private void FindBestTarget()
    {
        if (targetingSettings == null) return;

        if (TrySetTurretAggroTarget()) return;

        if (targetingSettings.switchTargets && Time.time < nextTargetSwitchTime)
            return;

        var targetScores = BuildTargetScores();
        if (targetScores.Count == 0)
        {
            ClearAggroIfNoPlayerTarget();
            return;
        }

        var bestTarget = targetScores
            .OrderByDescending(kvp => kvp.Value)
            .First()
            .Key;

        if (bestTarget != Target)
        {
            SetAggro(bestTarget);
            nextTargetSwitchTime = Time.time + targetingSettings.targetSwitchCooldown;
        }
    }

    private bool TrySetTurretAggroTarget()
    {
        if (!aggroedByTurret) return false;
        if (currentTurretTarget == null || !currentTurretTarget.activeInHierarchy) return false;

        SetAggro(currentTurretTarget);
        return true;
    }

    private Dictionary<GameObject, float> BuildTargetScores()
    {
        var scores = new Dictionary<GameObject, float>();

        if (targetingSettings.targetPlayer)
            AddPlayerTargetScore(scores);

        if (targetingSettings.targetTurrets)
            AddTurretTargetScores(scores);

        return scores;
    }

    private void AddPlayerTargetScore(Dictionary<GameObject, float> scores)
    {
        var player = GetPlayerTarget();
        if (player == null || !player.activeInHierarchy) return;

        var distance = Vector2.Distance(transform.position, player.transform.position);
        if (distance <= stats.currentDetectionRange)
        {
            scores[player] = CalculateTargetScore(player, targetingSettings.playerPriority);
        }
    }

    private void AddTurretTargetScores(Dictionary<GameObject, float> scores)
    {
        turretSearchCooldown -= Time.deltaTime;
        if (turretSearchCooldown <= 0)
        {
            CacheTurrets();
            turretSearchCooldown = 5f;
        }

        foreach (var turretObj in cachedTurrets)
        {
            if (turretObj == null || !turretObj.activeInHierarchy) continue;

            var distance = Vector2.Distance(transform.position, turretObj.transform.position);
            if (distance > targetingSettings.turretTargetingRange) continue;
            if (!IsValidTurretTarget(turretObj)) continue;

            scores[turretObj] = CalculateTargetScore(turretObj, targetingSettings.turretPriority);
        }
    }

    private void ClearAggroIfNoPlayerTarget()
    {
        if (Target != null && Target.CompareTag("Player"))
            return;

        ClearAggro();
    }

    private float CalculateTargetScore(GameObject target, int basePriority)
    {
        var score = basePriority * 100f;

        if (targetingSettings.prioritizeClosestTarget)
        {
            var distance = Vector2.Distance(transform.position, target.transform.position);
            var maxDistance = Mathf.Max(stats.currentDetectionRange, targetingSettings.turretTargetingRange);
            var distanceBonus = 1f - distance / maxDistance;
            score += distanceBonus * 50f;
        }

        if (IsTargetAttackingMe(target))
            score += 30f;

        if (target == cachedPlayer && isAggroed)
            score += 20f;

        return score;
    }

    private bool IsTargetAttackingMe(GameObject target)
    {
        // TODO: Implement logic to check if target is attacking this enemy
        return false;
    }

    private GameObject GetPlayerTarget()
    {
        if (cachedPlayer == null || !cachedPlayer.activeInHierarchy)
            cachedPlayer = GameObject.FindGameObjectWithTag("Player");

        return cachedPlayer;
    }
    #endregion

    #region Movement
    private void HandleMovementTarget(GameObject target)
    {
        var distance = Vector2.Distance(transform.position, target.transform.position);

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
    #endregion

    #region Combat
    private void TryAttack(GameObject target)
    {
        if (attack == null || target == null) return;

        var attackCooldown = 1f / Mathf.Max(0.01f, stats.currentAttackSpeed);
        if (Time.time < nextAttackTime) return;

        var distance = Vector2.Distance(transform.position, target.transform.position);
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

        var distanceToTarget = Vector2.Distance(transform.position, target.transform.position);

        var usableAbility = abilities
            .Select((ability, index) => new { ability, index })
            .Where(x => x.ability != null &&
                        x.ability.CanUse(gameObject, target) &&
                        distanceToTarget <= x.ability.ability.range)
            .OrderByDescending(x => x.ability.ability.priority)
            .FirstOrDefault();

        if (usableAbility == null) return;

        if (AbilityManager.Instance.TryUseAbility(gameObject, usableAbility.index, target))
            nextAbilityTime = Time.time + abilityCheckInterval;
    }
    #endregion

    #region Event Handlers
    protected virtual void HandleDamaged(DamageData damageData, KnockbackData knockbackData)
    {
        if (isPaused) return;
        enemyAnimator?.PlayHit();

        cachedPlayer ??= GameObject.FindGameObjectWithTag("Player");

        if (cachedPlayer == null)
        {
            Debug.LogWarning($"{name} took damage but no player found!");
            return;
        }

        if (IsTurretDamage(damageData))
            HandleTurretDamage();
        else
            HandlePlayerDamage();
    }

    private bool IsTurretDamage(DamageData damageData) =>
        damageData.source != null && damageData.source.TryGetComponent<TurretLevelBehaviour>(out _);

    private void HandlePlayerDamage()
    {
        SetAggro(cachedPlayer);
        AlertNearbyEnemies();
    }

    private void HandleTurretDamage()
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

    private void AddXPToTurret(DamageData damageData)
    {
        if (damageData.source == null) return;

        if (!damageData.source.TryGetComponent<TurretBehaviour>(out var turretBehaviour)) return;

        var blueprint = turretBehaviour.TurretBlueprint;
        if (blueprint != null)
        {
            TurretLevelManager.Instance?.AddXP(blueprint.turretType, stats.currentExperienceYield);
            Debug.Log($"XP added to {blueprint.turretType}: {stats.currentExperienceYield}");
        }
    }
    #endregion

    #region Resources
    private void DropResources()
    {
        var drops = stats.baseStats.resourceDrops;
        if (drops == null || drops.Length == 0) return;

        foreach (var drop in drops)
        {
            var roll = UnityEngine.Random.Range(0f, 1f);  // Fixed: Fully qualified Random
            if (roll > drop.dropChance) continue;

            var amount = UnityEngine.Random.Range(drop.minAmount, drop.maxAmount + 1);  // Fixed: Fully qualified Random
            if (amount > 0)
            {
                GameManager.Instance.gameDataSO.AddResource(drop.resourceType, amount);
                Debug.Log($"Dropped {amount} {drop.resourceType} (roll: {roll})");
            }
        }
    }
    #endregion

    #region Nested Types
    [System.Serializable]
    public class EnemyTargetingSettings
    {
        [Header("Target Priority")]
        public bool targetPlayer = true;
        public bool targetTurrets = false;
        public float turretTargetingRange = 30f;
        public int turretPriority = 1;
        public int playerPriority = 2;

        [Header("Target Switching")]
        public bool switchTargets = true;
        public float targetSwitchCooldown = 2f;
        public bool prioritizeClosestTarget = true;

        [Header("Turret Filters")]
        public LayerMask turretLayerMask = -1;
        public bool targetOnlyActiveTurrets = true;
        public bool targetOnlyTurretsWithAmmo = false;
    }
    #endregion
}
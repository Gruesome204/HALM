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
    [SerializeField] private float groupAggroRadius = 10f;
    [SerializeField] private float loseAggroMultiplier = 1.3f;

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


    #region Unity Callbacks
    private void Awake()
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

        AcquirePlayerTarget();
    }

    private void OnDisable() => GameManager.Instance?.UnregisterPausable(this);

    private void Update()
    {
        if (isPaused || (knockback != null && knockback.IsKnockedBack))
            return;

        if (!isAggroed)
        {
            CheckProximityAggro();
            return;
        }

        if (target == null) return;

        HandleMovementTarget(target);
        TryAttack(target);
        TryUseAbilities(target);
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

    public void AcquirePlayerTarget()
    {
        if (target != null) return;
        cachedPlayer ??= GameObject.FindGameObjectWithTag("Player");
        target = cachedPlayer;
    }

    private void CheckProximityAggro()
    {
        AcquirePlayerTarget();
        if (target == null) return;

        if (Vector2.Distance(transform.position, target.transform.position) <= stats.currentDetectionRange)
            SetAggro(target);
    }

    private void AlertNearbyEnemies()
    {
        AcquirePlayerTarget();
        if (target == null) return;

        var hits = Physics2D.OverlapCircleAll(transform.position, groupAggroRadius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out EnemyBehaviour enemy) && enemy != this)
                enemy.SetAggro(target);
        }
    }

    private void SetAggro(GameObject newTarget)
    {
        if (isAggroed) return;

        isAggroed = true;
        AcquirePlayerTarget();
        SetMovementTarget(newTarget);
        movement.isAggroed = true; 
        abilityBehaviour?.SetTarget(newTarget);
    }

    private void ClearAggro()
    {
        isAggroed = false;
        movement.Stop();
        movement.target = null;
        abilityBehaviour?.SetTarget(null);
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

        AcquirePlayerTarget();
        if (target == null)
        {
            Debug.LogWarning("Enemy damaged but no player found to chase!");
            return;
        }
        Debug.Log($"{name} is now aggroed on {target.name}");
        SetAggro(target);
        AlertNearbyEnemies();
    }

    private void HandleDeath(EnemyHealth enemyHealth, DamageData damageData)
    {
        DropResources();

        if (damageData.source != null && damageData.source.TryGetComponent<TurretLevelBehaviour>(out var turret))
        {
            TurretLevelManager.Instance.AddXP(turret.blueprint.turretType, stats.currentExperienceYield);
        }

        EnemySpawnManager.Instance.UnregisterEnemy(gameObject);
        Destroy(gameObject);
    }
    #endregion

    #region Resource Handling
    private void AddResourceToGameData(ResourceTypeData.ResourceType type, int amount)
    {
        var gameDataSO = GameManager.Instance.gameDataSO;
        switch (type)
        {
            case ResourceTypeData.ResourceType.WoodResource: gameDataSO.woodResource += amount; break;
            case ResourceTypeData.ResourceType.StoneResource: gameDataSO.steinResource += amount; break;
            case ResourceTypeData.ResourceType.MetalResource: gameDataSO.metallResource += amount; break;
            case ResourceTypeData.ResourceType.PulverResource: gameDataSO.pulverResource += amount; break;
            default: Debug.LogWarning($"Unhandled resource type: {type}"); break;
        }
    }

    private void DropResources()
    {
        var drops = stats.baseStats.resourceDrops;
        if (drops == null || drops.Length == 0) return;

        ResourceTypeData.OnResourceDropped += AddResourceToGameData;

        foreach (var drop in drops)
            ResourceTypeData.TryDrop(drop);

        ResourceTypeData.OnResourceDropped -= AddResourceToGameData;
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
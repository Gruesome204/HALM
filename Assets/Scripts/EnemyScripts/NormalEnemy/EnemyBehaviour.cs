using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour, IPausable
{
    [Header("References")]
    private EnemyStats stats;
    private EnemyHealth health;
    private EnemyMovement movement;
    private EnemyKnockback knockback;
    private EnemyAttack attack;
    private EnemyAbilityBehaviour abilityBehaviour;


    [Header("Targeting")]
    [SerializeField] private float groupAggroRadius = 30f;
    private static GameObject cachedPlayer;
    public GameObject target;
    private bool isAggroed;

    [Header("Ability Settings")]
    [Tooltip("Time between ability usage attempts (seconds).")]
    [SerializeField] private float abilityCheckInterval = 1.0f;

    private float nextAttackTime = 0.5f;   
    private float nextAbilityTime = 0f;
    private bool isPaused;

    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
        health = GetComponent<EnemyHealth>();
        movement = GetComponent<EnemyMovement>();
        knockback = GetComponent<EnemyKnockback>();
        attack = GetComponent<EnemyAttack>();
        abilityBehaviour = GetComponent<EnemyAbilityBehaviour>();

        stats.Initialize();

        health.OnDeath += HandleDeath;
        health.OnDamaged += HandleDamaged;
    }


    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterPausable(this);
        else
            Debug.LogWarning("GameManager not ready yet, EnemyBehaviour won't receive pause events");
        AcquirePlayerTarget();
    }


    private void OnDisable() => GameManager.Instance?.UnregisterPausable(this);
    private void FixedUpdate()
    {
        if (isPaused || (knockback != null && knockback.IsKnockedBack))
            return;

        if (!isAggroed)
        {
            CheckProximityAggro();
            return;
        }


        if (target == null)
            return;

        HandleMovementTarget(target);
        TryAttack(target);
    }

    private void AcquirePlayerTarget()
    {
        if (target != null) return;

        if (cachedPlayer == null)
            cachedPlayer = GameObject.FindGameObjectWithTag("Player");

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
            {
                enemy.SetAggro(target);
            }
        }
    }

    private void SetAggro(GameObject newTarget)
    {
        if (isAggroed) return;

        AcquirePlayerTarget();
        if (target == null) return;

        isAggroed = true;
        movement.target = target;
    }


    private void HandleMovementTarget(GameObject target)
    {
        float distance = Vector2.Distance(transform.position, target.transform.position);

        if (distance <= stats.currentAttackRange)
            movement.Stop();
        else
            movement.target = target;
    }


    private void HandleDamaged(DamageData damageData, KnockbackData knockbackData)
    {
        if (isPaused) return;

        AcquirePlayerTarget();
        if (target == null) return;

        SetAggro(target);
        AlertNearbyEnemies();

    }

    private void TryAttack(GameObject target)
    {
        if (attack == null || target == null) return;

        float attackCooldown = 1f / Mathf.Max(0.01f, stats.currentAttackSpeed);

        if (Time.time >= nextAttackTime)
        {
            float distance = Vector2.Distance(transform.position, target.transform.position);
            if (distance <= stats.currentAttackRange)
            {
                attack.PerformAttack(target, isPaused);
                nextAttackTime = Time.time + attackCooldown;

               // Debug.Log($"{name} attacked {target.name} (Cooldown: {attackCooldown:F2}s)");
            }
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

        bool used = AbilityManager.Instance.TryUseAbility(gameObject, usable.index, target);
        if (used)
        {
            nextAbilityTime = Time.time + abilityCheckInterval;
            Debug.Log($"{name} used ability: {usable.ability.ability.name}");
        }
    }

    private void HandleDeath(EnemyHealth enemyHealth, DamageData damageData)
    {
        Debug.Log($"Enemy {enemyHealth.gameObject.name} died from {damageData.type} damage.");

        DropResources();

        // Check if the source still exists before accessing
        if (damageData.source != null)
        {
            var turret = damageData.source.GetComponent<TurretLevelBehaviour>();
            if (turret != null)
            {
                TurretLevelManager.Instance.AddXP(
                    turret.blueprint.turretType,
                    stats.currentExperienceYield
                );
                Debug.Log($"{stats.currentExperienceYield} EXP Awarded to {turret.blueprint.turretType} ");
            }
            else
            {
                Debug.Log("Source exists but is not a turret");
            }
        }
        else
        {
            Debug.Log("No valid turret source (probably demolished or destroyed).");
        }
        EnemySpawnManager.Instance.UnregisterEnemy(gameObject);
        Destroy(gameObject);
    }

    private void AddResourceToGameData(ResourceTypeData.ResourceType type, int amount)
    {
       GameDataSO gameDataSO = GameManager.Instance.gameDataSO;

        switch (type)
        {
            case ResourceTypeData.ResourceType.WoodResource:
                gameDataSO.woodResource += amount;
                break;

            case ResourceTypeData.ResourceType.StoneResource:
                gameDataSO.steinResource += amount;
                break;

            case ResourceTypeData.ResourceType.MetalResource:
                gameDataSO.metallResource += amount;
                break;

            case ResourceTypeData.ResourceType.PulverResource:
                gameDataSO.pulverResource += amount;
                break;

            default:
                Debug.LogWarning($"Unhandled resource type: {type}");
                break;
        }

        Debug.Log($"Added {amount}x {type} to GameDataSO.");
    }
    private void DropResources()
    {
        var drops = stats.baseStats.resourceDrops;
        if (drops == null || drops.Length == 0) return;

        foreach (var drop in drops)
        {
            if (Random.value > drop.dropChance)
                continue;

            int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);
            if (amount <= 0)
                continue;

            AddResourceToGameData(drop.resourceType, amount);
        }
    }

    public void OnPause()
    {
        isPaused = true;
        movement.SetPaused(true);
        Animator animator = GetComponent<Animator>();
        animator.enabled = false;
        // Stop moving, stop attacking, etc.
    }

    public void OnResume()
    {
        isPaused = false;
        movement.SetPaused(false);
        // Reset attack & ability timers
        nextAttackTime = Time.time;
        nextAbilityTime = Time.time;
        if (movement != null)
        {
            movement.target = null;
            movement.MoveTowardTarget();
        }
        Animator animator = GetComponent<Animator>();
        animator.enabled = true;
    }

}
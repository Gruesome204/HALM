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
    public GameObject target;

    [Header("Ability Settings")]
    [Tooltip("Time between ability usage attempts (seconds).")]
    [SerializeField] private float abilityCheckInterval = 1.0f;

    private float nextAttackTime = 0f;   
    private float nextAbilityTime = 0f;
    private bool isPaused;

    private void OnDisable() => GameManager.Instance?.UnregisterPausable(this);

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
    }


    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterPausable(this);
        else
            Debug.LogWarning("GameManager not ready yet, EnemyBehaviour won't receive pause events");
        target = GameObject.FindGameObjectWithTag("Player");
        if (target == null)
            Debug.LogWarning($"{name} could not find a GameObject with tag 'Player'");
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

    private void FixedUpdate()
    {
        if (isPaused || (knockback != null && knockback.IsKnockedBack)) return;

        DropTargetIfTooFar();

        if (target == null) return;

        HandleMovement(target);
        TryAttack(target);
        TryUseAbilities(target);

    }

    private void HandleMovement(GameObject target)
    {
        if (movement.target == null || movement.target.gameObject != target)
            movement.target = target;

        float distance = Vector2.Distance(transform.position, target.transform.position);
        bool inAttackRange = distance <= stats.currentAttackRange;

        if (!inAttackRange)
            movement.MoveTowardTarget();
        else
            movement.Stop();
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

                Debug.Log($"{name} attacked {target.name} (Cooldown: {attackCooldown:F2}s)");
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
    private void DropTargetIfTooFar()
    {
        if (target == null) return;

        float distance = Vector2.Distance(transform.position, target.transform.position);

        if (distance > stats.currentDetectionRange)
            movement.target = null;

        if (distance > stats.currentAttackRange && abilityBehaviour != null)
            abilityBehaviour.target = null;
    }

    private void HandleDeath(EnemyHealth enemyHealth, DamageData damageData)
    {
        Debug.Log($"Enemy {enemyHealth.gameObject.name} died from {damageData.type} damage.");

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
}
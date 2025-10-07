using System.Linq;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour, IPausable
{
    private EnemyStats stats;
    private EnemyHealth health;
    private EnemyMovement movement;
    private EnemyKnockback knockback;
    private EnemyAttack attack;
    private EnemyAbilityBehaviour abilityBehaviour;

    private bool isPaused;

    [Header("Ability Settings")]
    [Tooltip("Time between ability usage attempts (seconds).")]
    [SerializeField] private float abilityCheckInterval = 1.0f;
    private float abilityCheckTimer;
    private float nextAttackTime = 0f;
    private float nextAbilityTime = 0f;

    private void OnEnable() => GameManager.Instance?.RegisterPausable(this);
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

    public void OnPause()
    {
        isPaused = true;
        // Stop moving, stop attacking, etc.
    }

    public void OnResume()
    {
        isPaused = false;
        // Resume AI
    }
    private void Update()
    {
        if (isPaused || (knockback != null && knockback.IsKnockedBack)) return;

        var target = SelectTarget();
        if (target == null) return;

        HandleMovement(target);
        //TryAttack(target);
        TryUseAbilities(target);
    }
    private void HandleMovement(GameObject target)
    {
        float distance = Vector2.Distance(transform.position, target.transform.position);
        bool inAttackRange = distance <= stats.currentAttackRange;

        if (!inAttackRange)
        {
            movement.MoveTowardTarget();
        }
        else
        {
            movement.Stop();
        }
    }
    private void TryAttack(GameObject target)
    {
        if (attack == null || target == null) return;

        float distance = Vector2.Distance(transform.position, target.transform.position);
        if (distance > stats.currentAttackRange) return;

        float attackCooldown = 1f / Mathf.Max(0.01f, stats.currentAttackSpeed);

        if (Time.time >= nextAttackTime)
        {
            attack.TryAttack(target);
            nextAttackTime = Time.time + attackCooldown;
            Debug.Log($"{name} attacked {target.name}. Next attack at {nextAttackTime:F2}s");
        }
    }

    private void TryUseAbilities(GameObject target)
    {
        if (abilityBehaviour == null || target == null || AbilityManager.Instance == null) return;

        if (Time.time < nextAbilityTime) return;

        var abilities = AbilityManager.Instance.GetAbilities(gameObject);
        if (abilities == null || abilities.Count == 0) return;

        // Pick highest-priority usable ability
        var usable = abilities
            .Select((a, i) => new { ability = a, index = i })
            .Where(x => x.ability != null && x.ability.CanUse(gameObject, target))
            .OrderByDescending(x => x.ability.ability.priority)
            .FirstOrDefault();

        if (usable == null) return;

        bool used = AbilityManager.Instance.TryUseAbility(gameObject, usable.index, target);
        if (used)
        {
            nextAbilityTime = Time.time + abilityCheckInterval;
            Debug.Log($"{name} used ability: {usable.ability.ability.name}. Next ability at {nextAbilityTime:F2}s");
        }
    }

    private GameObject SelectTarget()
    {
        // First try movement's assigned target
        if (movement?.target != null)
            return movement.target.gameObject;

        // Otherwise fallback to closest enemy (players/towers)
        var potentialTargets = GameObject.FindGameObjectsWithTag("Player"); // or other target tag
        if (potentialTargets.Length == 0) return null;

        return potentialTargets
            .OrderBy(t => Vector2.Distance(transform.position, t.transform.position))
            .First();
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

        Destroy(gameObject);
    }
}
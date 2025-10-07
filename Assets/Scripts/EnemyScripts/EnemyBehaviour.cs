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

    private void FixedUpdate()
    {
        //Checks if Game is Paused
        if (isPaused) return;

        if (knockback != null && knockback.IsKnockedBack)
        {
            movement.Stop();
            return;
        }

        if (movement.target != null)
        {
            float distance = Vector2.Distance(transform.position, movement.target.transform.position);

            TryUseAbilities(movement.target);

            if (attack != null && distance <= stats.currentAttackRange)
            {
                // Stop moving and attack
                movement.Stop();    
                attack.TryAttack(movement.target);
                Debug.Log("Try Call Attack");
            }
            else
            {
                // Move toward target
                movement.MoveTowardTarget();
            }
        }
        else
        {
            // No target, look for one
            movement.MoveTowardTarget();
        }
    }
    private void TryUseAbilities(GameObject target)
    {
        // Ability check timer
        abilityCheckTimer -= Time.deltaTime;
        if (abilityCheckTimer > 0f) return;
        abilityCheckTimer = abilityCheckInterval;

        // Make sure the enemy has abilities
        if (abilityBehaviour == null || abilityBehaviour.abilities == null || abilityBehaviour.abilities.Length == 0)
            return;

        if (AbilityManager.Instance == null)
            return;

        //Get all abilities for this enemy
        var abilities = AbilityManager.Instance.GetAbilities(gameObject);
        if (abilities == null || abilities.Count == 0)
            return;

        // Pick usable ability by highest priority
        var usable = abilities
            .Select((a, i) => new { ability = a, index = i })
            .Where(x => x.ability != null && x.ability.CanUse(gameObject, target))
            .OrderByDescending(x => x.ability.ability.priority)
            .FirstOrDefault();

        if (usable == null)
            return;

        //Use ability through the centralized AbilityManager method
        bool used = AbilityManager.Instance.TryUseAbility(gameObject, usable.index, target);
        if (used)
        {
            Debug.Log($"{name} used ability: {usable.ability.ability.name}");
        }
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
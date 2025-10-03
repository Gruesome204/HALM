using UnityEngine;

public class EnemyBehaviour : MonoBehaviour, IPausable
{
    private EnemyStats stats;
    private EnemyHealth health;
    private EnemyMovement movement;
    private EnemyKnockback knockback;
    private EnemyAttack attack;

    private bool isPaused;


    private void OnEnable() => GameManager.Instance?.RegisterPausable(this);
    private void OnDisable() => GameManager.Instance?.UnregisterPausable(this);

    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
        health = GetComponent<EnemyHealth>();
        movement = GetComponent<EnemyMovement>();
        knockback = GetComponent<EnemyKnockback>();
        attack = GetComponent<EnemyAttack>();

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
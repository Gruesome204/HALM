using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    private EnemyStats stats;
    private EnemyHealth health;
    private EnemyMovement movement;
    private EnemyKnockback knockback;

    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
        health = GetComponent<EnemyHealth>();
        movement = GetComponent<EnemyMovement>();
        knockback = GetComponent<EnemyKnockback>();

        stats.Initialize();
        health.OnDeath += HandleDeath;
    }

    private void FixedUpdate()
    {
        if (knockback != null && knockback.IsKnockedBack) return;

        movement.MoveTowardTarget();
    }

    private void HandleDeath(EnemyHealth enemyHealth, DamageData damageData)
    {
        Debug.Log($"Enemy {enemyHealth.gameObject.name} died from {damageData.type} damage.");
        // Give XP to the turret that killed it
        var turret = damageData.source?.GetComponent<TurretLevelBehaviour>();
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
            Debug.Log("No turret source");
        }
        Destroy(gameObject);
    }
}
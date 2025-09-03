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
        health.OnDeath += Die;
    }

    private void FixedUpdate()
    {
        if (knockback != null && knockback.IsKnockedBack) return;

        movement.MoveTowardTarget();
    }

    private void Die(EnemyHealth enemy, DamageData damageData)
    {
        // Give XP to the turret that killed it
        TurretLevelBehaviour turret = damageData.source.GetComponent<TurretLevelBehaviour>();
        if (turret != null)
        {
            turret.AddXP(stats.currentExperienceYield);
        }
        Destroy(gameObject);
    }
}
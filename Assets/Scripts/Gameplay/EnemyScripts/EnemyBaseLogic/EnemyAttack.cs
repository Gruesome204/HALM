using UnityEngine;
using static DamageData;

public class EnemyAttack : MonoBehaviour
{
    private EnemyStats stats;
    private EnemyAnimator enemyAnimator;
    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
        if (stats == null)
            Debug.LogWarning("EnemyStats component missing on enemy!");

        enemyAnimator = GetComponent<EnemyAnimator>();
    }

    public void PerformAttack(GameObject target, bool isPaused)
    {
        if (isPaused) return;

        enemyAnimator?.PlayAttack();

        if (target == null || stats == null) return;

        if (target.TryGetComponent<IInvulnerable>(out var inv) && inv.IsInvulnerable)
            return;

        IDamagable damagable = target.GetComponentInChildren<IDamagable>();
        if (damagable == null || damagable.IsInvulnerable)
        {
        Debug.Log("Target could not be hit");
         return;
        }

        float distance = Vector2.Distance(transform.position, target.transform.position);

        if (distance > stats.currentAttackRange) return;

        // Create damage data
        DamageData damageData = new DamageData
        {
            amount = stats.currentDamage,
            source = gameObject,
            type = DamageType.Physical // Change if needed
        };

        // Knockback data
        KnockbackData knockbackData = new KnockbackData
        {
            knockbackStrength = stats.currentKnockbackForce,
            direction = (target.transform.position - transform.position).normalized
        };

        damagable.TakeDamage(damageData, knockbackData);
        Debug.Log($"{gameObject.name} attacked {damagable.GetTargetType()} for {damageData.amount} damage with {knockbackData.knockbackStrength} knockback.");
    }
}

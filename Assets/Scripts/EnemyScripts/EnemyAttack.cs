using UnityEngine;
using static DamageData;

public class EnemyAttack : MonoBehaviour
{
    private EnemyStats stats;

    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
        if (stats == null)
            Debug.LogWarning("EnemyStats component missing on enemy!");
    }

    public void PerformAttack(GameObject target, bool isPaused)
    {
        if (target == null || stats == null) return;

        IDamagable damagable = target.GetComponent<IDamagable>();
        if (damagable == null || damagable.IsInvulnerable) return;

        float distance = Vector2.Distance(transform.position, target.transform.position);
        if (distance > stats.currentAttackRange + 0.1f) return;

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

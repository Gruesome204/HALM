using UnityEngine;
using static DamageData;

public class EnemyAttack : MonoBehaviour
{
    private float lastAttackTime;
    private EnemyStats stats;

    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
        if (stats == null)
            Debug.LogWarning("EnemyStats component missing on enemy!");
    }

    public void TryAttack(GameObject target)
    {
        Debug.Log("Attack Target");
        if (target == null || stats == null) return;

        IDamagable damagable = target.GetComponent<IDamagable>();
        if (damagable == null || damagable.IsInvulnerable) return;

        float distance = Vector2.Distance(transform.position, target.transform.position);
        float attackRangeWithTolerance = stats.currentAttackRange + 0.1f; // small tolerance
        if (distance > attackRangeWithTolerance) return;

        if (Time.time >= lastAttackTime + stats.currentAttackSpeed)
        {
            // Create damage data
            DamageData damageData = new DamageData
            {
                amount = stats.currentDamage,
                source = gameObject,
                type = DamageType.Physical // Change if needed
            };

            // Knockback
            KnockbackData knockbackData = new KnockbackData
            {
                knockbackStrength = stats.currentKnockbackForce,
                direction = (target.transform.position - transform.position).normalized
            };

            damagable.TakeDamage(damageData, knockbackData);
            Debug.Log($"{gameObject.name} attacked {damagable.GetTargetType()} for {damageData.amount} damage with {knockbackData.knockbackStrength} knockback.");

            lastAttackTime = Time.time;
        }
    }
}

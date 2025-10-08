using UnityEngine;
using static DamageData;

public class EnemyAttack : MonoBehaviour
{
    private EnemyStats stats;
    private float attackTimer = 0f;

    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
        if (stats == null)
            Debug.LogWarning("EnemyStats component missing on enemy!");
    }

    public void TryAttack(GameObject target, bool isPaused)
    {
        Debug.Log("Attack Target");
        if (target == null || stats == null) return;
        if (isPaused) return;
        attackTimer += Time.deltaTime;
        IDamagable damagable = target.GetComponent<IDamagable>();
        if (damagable == null || damagable.IsInvulnerable) return;

        float distance = Vector2.Distance(transform.position, target.transform.position);
        float attackRangeWithTolerance = stats.currentAttackRange + 0.1f; // small tolerance
        if (distance > attackRangeWithTolerance) return;

        float attackCooldown = 1f / stats.currentAttackSpeed;
        Debug.Log($"AttackSpeed: {stats.currentAttackSpeed} attacks/sec, Cooldown: {attackCooldown}s, Time since last attack: {attackCooldown}s");

        if (attackTimer >= attackCooldown)
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

        }
    }
}

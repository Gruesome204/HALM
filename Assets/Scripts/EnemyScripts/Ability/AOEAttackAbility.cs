using UnityEngine;
using static EnemyAbility;

public class AOEAttackAbility : EnemyAbility
{
    public int damage;
    public float radius;

    private void OnEnable() => targetType = AbilityTargetType.AreaOfEffect;

    protected override void Activate(GameObject user, GameObject target)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(user.transform.position, radius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Debug.Log($"{user.name} AoE hits {hit.name} for {damage} damage!");
                // hit.GetComponent<Health>()?.TakeDamage(damage);
            }
        }
    }
}

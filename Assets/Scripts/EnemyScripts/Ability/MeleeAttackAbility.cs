using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy/EnemyAbilities/New MeeleAttack")]
public class MeleeAttackAbility : EnemyAbility
{
    public int damage;

    private void OnEnable() => targetType = AbilityTargetType.SingleTarget;

    protected override void Activate(GameObject user, GameObject target)
    {
        Debug.Log($"{user.name} slashes {target.name} for {damage} damage!");
        // target.GetComponent<Health>()?.TakeDamage(damage);
    }
}

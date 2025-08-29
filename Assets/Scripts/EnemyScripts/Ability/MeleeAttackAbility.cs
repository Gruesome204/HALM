using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy/EnemyAbilities/New MeeleAttack")]
public class MeleeAttackAbility : EnemyAbility
{
    public int damage;

    protected override void Activate(GameObject user, GameObject target)
    {
        Debug.Log($"{user.name} attacks {target.name} for {damage} damage!");
        // Add animation, hit detection, apply damage logic here
    }
}

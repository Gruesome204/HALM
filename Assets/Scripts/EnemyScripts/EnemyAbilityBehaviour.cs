using UnityEngine;
using static EnemyAbility;

public class EnemyAbilityBehaviour : MonoBehaviour
{
    public EnemyAbility[] abilities;
    public Transform target;

    private void Update()
    {
        foreach (var ability in abilities)
        {
            if (target == null) return;

            if (ability.CanUse())
            {
                switch (ability.targetType)
                {
                    case AbilityTargetType.Self:
                        ability.Use(gameObject, gameObject);
                        break;

                    case AbilityTargetType.SingleTarget:
                        if (Vector2.Distance(transform.position, target.position) <= ability.range)
                            ability.Use(gameObject, target.gameObject);
                        break;

                    case AbilityTargetType.AreaOfEffect:
                        ability.Use(gameObject, target.gameObject); // AoE doesn't need specific target
                        break;
                }
                break;
            }
        }
    }
}

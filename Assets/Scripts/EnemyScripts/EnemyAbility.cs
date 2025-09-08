using UnityEngine;

public class EnemyAbilityBehaviour : MonoBehaviour
{
    public EnemyAbility[] abilities;
    public Transform target;

    private void Update()
    {
        foreach (var ability in abilities)
        {
            if (ability.CanUse() && Vector2.Distance(transform.position, target.position) <= ability.range)
            {
                ability.Use(gameObject, target.gameObject);
                break; // Use one ability at a time
            }
        }
    }
}

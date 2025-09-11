using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy/EnemyAbilities/New Heal Ability")]
public class HealAbility : EnemyAbility
{
    public int healAmount;

    private void OnEnable() => targetType = AbilityTargetType.Self;

    protected override void Activate(GameObject user, GameObject target)
    {
        Debug.Log($"{user.name} heals for {healAmount} HP!");
        // user.GetComponent<Health>()?.Heal(healAmount);
    }
}
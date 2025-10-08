using UnityEngine;

public class AbilityRuntime
{
    public EnemyAbilityBlueprint ability;
    private float nextUseTime = 0f;

    public AbilityRuntime(EnemyAbilityBlueprint ability)
    {
        this.ability = ability;
    }

    public bool CanUse(GameObject user, GameObject target)
    {
        if (ability == null) return false;
        if (Time.time < nextUseTime) return false;

        // Add more conditions as needed (range, status, etc.)
        return true;
    }

    public void Use(GameObject user, GameObject target)
    {
        if (ability == null) return;

        // Apply all effects
        foreach (var effect in ability.effects)
        {
            effect?.Apply(user, target);
        }
        // Here you trigger the actual ability effect
        // For example: spawn projectiles, apply damage, buffs, etc.
        Debug.Log($"{user.name} used ability {ability.abilityName} on {target?.name}");


        // Set cooldown
        nextUseTime = Time.time + ability.cooldown;
    }

}

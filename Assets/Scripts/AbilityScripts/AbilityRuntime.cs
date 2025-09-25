using UnityEngine;

public class AbilityRuntime
{
    public EnemyAbilityBlueprint ability;
    public float lastUsedTime;

    public AbilityRuntime(EnemyAbilityBlueprint ability)
    {
        this.ability = ability;
        lastUsedTime = -999f;
    }

    public bool CanUse(GameObject user, GameObject target)
    {
        return Time.time >= lastUsedTime + ability.cooldown;
    }

    public void Use(GameObject user, GameObject target)
    {
        lastUsedTime = Time.time;
        foreach (var effect in ability.effects)
        {
            effect.Apply(user, target);
        }
    }

    public float GetCooldownRemaining()
    {
        return Mathf.Max(0, (lastUsedTime + ability.cooldown) - Time.time);
    }
}

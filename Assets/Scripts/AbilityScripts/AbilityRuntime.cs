using UnityEngine;

public class AbilityRuntime
{
    public EnemyAbilityBlueprint ability;
    public float lastUseTime = -Mathf.Infinity;

    public AbilityRuntime(EnemyAbilityBlueprint ability)
    {
        this.ability = ability;
        lastUseTime = -999f;
    }

    public bool CanUse(GameObject user, GameObject target)
    {
        if (ability == null)
            return false;
        // Cooldown check
        if (Time.time - lastUseTime < ability.cooldown)
            return false;
        if (target != null)
        {
            float distance = Vector2.Distance(user.transform.position, target.transform.position);
            if (distance > ability.range)
                return false;
        }
        return true;
    }

    public void Use(GameObject user, GameObject target)
    {
        if (ability == null)
            return;

        lastUseTime = Time.time;

        Debug.Log($"{user.name} used {ability.abilityName}");

        // Apply all defined effects
        if (ability.effects != null)
        {
            foreach (var effect in ability.effects)
            {
                if (effect == null) continue;
                effect.Apply(user, target);
            }
        }
    }

    public float GetCooldownRemaining()
    {
        return Mathf.Max(0, (lastUseTime + ability.cooldown) - Time.time);
    }
}

using UnityEngine;

public class AbilityRuntime
{
    public EnemyAbilityBlueprint ability;
    private float cooldownTimer = 0f;
    private bool isPaused = false;

    public AbilityRuntime(EnemyAbilityBlueprint ability)
    {
        this.ability = ability;
    }

    public bool CanUse(GameObject user, GameObject target)
    {
        if (ability == null) return false;
        if (cooldownTimer > 0f) return false;

        if (ability.targetType != AbilityTargetType.Self && target == null)
            return false;

        // Range check
        if (target != null)
        {
            float distance = Vector3.Distance(user.transform.position, target.transform.position);
            if (distance > ability.range)
                return false;
        }

        // Health check
        if (target != null)
        {
            var targetHealth = target.GetComponent<EnemyStats>();
            if (targetHealth != null)
            {
                float hpPercent = targetHealth.currentHealth / targetHealth.currentMaxHealth;
                if (hpPercent > ability.useBelowHealthPercent)
                    return false;
            }
        }

        return true;
    }

    public void Use(GameObject user, GameObject target)
    {
        if (ability == null) return;

        // Apply effects
        foreach (var effect in ability.effects)
        {
            effect?.Apply(user, target);
        }

        // Start cooldown
        cooldownTimer = ability.cooldown;
    }

    // Call this every frame from AbilityManager
    public void Tick(float deltaTime)
    {
        if (isPaused) return;

        if (cooldownTimer > 0f)
            cooldownTimer -= deltaTime;
    }

    public void Pause() => isPaused = true;
    public void Resume() => isPaused = false;
}

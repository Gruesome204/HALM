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

        // Add additional conditions as needed (range, status effects, etc.)
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

using UnityEngine;
using System.Collections.Generic;

public class AbilityRuntime
{
    public AbilityBlueprint ability;
    private float cooldownTimer = 0f;
    private bool isPaused = false;

    public AbilityRuntime(AbilityBlueprint ability)
    {
        this.ability = ability;
    }

    /// <summary>
    /// Checks if the ability can currently be used on the target
    /// </summary>
    public bool CanUse(GameObject user, GameObject target)
    {
        if (ability == null || cooldownTimer > 0f)
            return false;

        // Self-targeted abilities always allowed
        if (ability.targetType != AbilityTargetType.Self && target == null)
            return false;

        // Range check
        if (target != null && ability.range > 0f)
        {
            float distance = Vector3.Distance(user.transform.position, target.transform.position);
            if (distance > ability.range)
                return false;
        }

        // Health constraints
        if (target != null)
        {
            var targetHealth = target.GetComponent<IDamagable>();
            if (targetHealth != null)
            {
                float hpPercent = 1f;
                if (targetHealth is PlayerHealth ph)
                    hpPercent = ph.stats.currentHealth / ph.stats.currentMaxHealth;
                else if (targetHealth is EnemyHealth eh)
                    hpPercent = eh.stats.currentHealth / eh.stats.currentMaxHealth;

                if (hpPercent > ability.maxTargetHealthPercent || hpPercent < ability.minUserHealthPercent)
                    return false;
            }
        }

        // TODO: Add resource checks if needed (mana, energy, etc.)

        return true;
    }

    /// <summary>
    /// Use the ability on the target
    /// </summary>
    public void Use(GameObject user, GameObject target)
    {
        if (ability == null)
            return;

        // Apply visual prefab immediately if assigned
        if (ability.visualEffectPrefab != null)
        {
            GameObject.Instantiate(ability.visualEffectPrefab, user.transform.position, Quaternion.identity);
        }

        // Apply all effects
        foreach (var effect in ability.effects)
        {
            if (effect != null)
            {
                if (ability.effectDelay > 0f)
                    user.GetComponent<MonoBehaviour>().StartCoroutine(DelayedEffect(effect, user, target, ability.effectDelay));
                else
                    effect.Apply(user, target);
            }
        }

        // Start cooldown
        cooldownTimer = ability.cooldown;

        // Optional: play sound
        if (ability.soundEffect != null)
            AudioSource.PlayClipAtPoint(ability.soundEffect, user.transform.position);
    }

    private System.Collections.IEnumerator DelayedEffect(AbilityEffect effect, GameObject user, GameObject target, float delay)
    {
        yield return new WaitForSeconds(delay);
        effect.Apply(user, target);
    }

    public void Tick(float deltaTime)
    {
        if (isPaused) return;
        if (cooldownTimer > 0f)
            cooldownTimer -= deltaTime;
    }

    public void Pause() => isPaused = true;
    public void Resume() => isPaused = false;
}

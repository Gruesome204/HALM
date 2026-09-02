using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AbilityRuntime
{
    public AbilityBlueprint ability;
    private float cooldownTimer = 0f;
    private bool isPaused = false;

    // Layer masks for enemy detection
    private const int ENEMY_LAYER = 1 << 8; // Assuming Layer 8 is "Enemy"
    private const int PLAYER_LAYER = 1 << 9; // Assuming Layer 9 is "Player"

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
        if (ability.targetType == AbilityTargetType.Self && target == null)
            return true;

        // Check if target is required
        if (ability.targetType != AbilityTargetType.Self &&
            ability.targetType != AbilityTargetType.GroundPlacement &&
            ability.targetType != AbilityTargetType.Direction &&
            target == null)
            return false;

        // Range check for single target abilities (not AoE or ground placement)
        if (target != null && ability.range > 0f &&
            ability.targetType != AbilityTargetType.AreaOfEffect &&
            ability.targetType != AbilityTargetType.Cone &&
            ability.targetType != AbilityTargetType.GroundPlacement)
        {
            float distance = Vector3.Distance(user.transform.position, target.transform.position);
            if (distance > ability.range)
                return false;
        }

        // Health constraints for single target
        if (target != null &&
            ability.targetType != AbilityTargetType.AreaOfEffect &&
            ability.targetType != AbilityTargetType.Cone)
        {
            if (!CheckHealthConstraints(target))
                return false;
        }

        return true;
    }

    private bool CheckHealthConstraints(GameObject target)
    {
        var targetHealth = target.GetComponent<IDamagable>();
        if (targetHealth != null)
        {
            float hpPercent = 1f;
            if (targetHealth is PlayerHealth ph)
                hpPercent = ph.stats.currentHealth / ph.stats.currentMaxHealth;
            else if (targetHealth is EnemyHealth eh)
                hpPercent = eh.Stats.currentHealth / eh.Stats.maxHealth;

            if (hpPercent > ability.maxTargetHealthPercent || hpPercent < ability.minUserHealthPercent)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Check if a GameObject is on the enemy layer
    /// </summary>
    private bool IsEnemy(GameObject obj)
    {
        return obj.layer == LayerMask.NameToLayer("Enemy");
    }

    /// <summary>
    /// Check if a GameObject is on the player layer
    /// </summary>
    private bool IsPlayer(GameObject obj)
    {
        return obj.layer == LayerMask.NameToLayer("Player");
    }

    /// <summary>
    /// Get all valid targets within AoE range
    /// </summary>
    public List<GameObject> GetTargetsInAoE(GameObject user, GameObject primaryTarget)
    {
        List<GameObject> targets = new List<GameObject>();

        if (ability.aoeShape == AoEShape.None)
        {
            if (primaryTarget != null)
                targets.Add(primaryTarget);
            return targets;
        }

        // Determine which layers to target based on the user
        int targetLayerMask = IsEnemy(user) ? PLAYER_LAYER : ENEMY_LAYER;

        // Get all targets in range
        Collider[] colliders = Physics.OverlapSphere(user.transform.position, ability.aoeRadius, targetLayerMask);

        foreach (var collider in colliders)
        {
            GameObject potentialTarget = collider.gameObject;

            // Skip self
            if (potentialTarget == user)
                continue;

            // Check if within cone angle if applicable
            if (ability.aoeShape == AoEShape.Cone)
            {
                Vector3 directionToTarget = (potentialTarget.transform.position - user.transform.position).normalized;
                float angle = Vector3.Angle(user.transform.forward, directionToTarget);
                if (angle > ability.aoeAngle / 2f)
                    continue;
            }

            // Check health constraints
            if (CheckHealthConstraints(potentialTarget))
            {
                targets.Add(potentialTarget);
            }
        }

        return targets;
    }

    /// <summary>
    /// Select the best target based on targetSelection strategy
    /// </summary>
    public GameObject SelectTarget(GameObject user, List<GameObject> availableTargets)
    {
        if (availableTargets == null || availableTargets.Count == 0)
            return null;

        // If only one target, return it
        if (availableTargets.Count == 1)
            return availableTargets[0];

        // For Self targeting, return the user
        if (ability.targetSelection == TargetSelection.Self)
            return user;

        // Filter to only enemies (for offensive abilities)
        List<GameObject> enemies = availableTargets.Where(t => IsEnemy(t)).ToList();

        // If no enemies, use all available targets
        List<GameObject> validTargets = enemies.Count > 0 ? enemies : availableTargets;

        switch (ability.targetSelection)
        {
            case TargetSelection.ClosestEnemy:
                return validTargets.OrderBy(t => Vector3.Distance(user.transform.position, t.transform.position)).FirstOrDefault();

            case TargetSelection.LowestHPEnemy:
                return validTargets.OrderBy(t => GetHealthPercentage(t)).FirstOrDefault();

            case TargetSelection.RandomEnemy:
                return validTargets[Random.Range(0, validTargets.Count)];

            case TargetSelection.Self:
                return user;

            default:
                return validTargets[0];
        }
    }

    private float GetHealthPercentage(GameObject target)
    {
        var health = target.GetComponent<IDamagable>();
        if (health == null) return 1f;

        if (health is PlayerHealth ph)
            return ph.stats.currentHealth / ph.stats.currentMaxHealth;
        else if (health is EnemyHealth eh)
            return eh.Stats.currentHealth / eh.Stats.maxHealth;

        return 1f;
    }

    /// <summary>
    /// Use the ability on the target
    /// </summary>
    public void Use(GameObject user, GameObject target)
    {
        if (ability == null)
            return;

        // For Self targeting, set target to user
        if (ability.targetSelection == TargetSelection.Self)
            target = user;

        // Get all valid targets based on AoE
        List<GameObject> targets = GetTargetsInAoE(user, target);

        // Select primary target if none selected
        if (target == null && targets.Count > 0)
        {
            target = SelectTarget(user, targets);
        }

        // Apply visual prefab
        if (ability.visualEffectPrefab != null)
        {
            Vector3 effectPosition = target != null ? target.transform.position : user.transform.position;
            GameObject effect = GameObject.Instantiate(ability.visualEffectPrefab, effectPosition, Quaternion.identity);

            // Handle visual duration
            if (ability.visualDuration > 0)
                GameObject.Destroy(effect, ability.visualDuration);
        }

        // Apply effects to all targets in AoE
        if (ability.aoeShape != AoEShape.None)
        {
            // AoE: Apply to all targets in range
            foreach (var aoeTarget in targets)
            {
                ApplyEffects(user, aoeTarget);
            }

            // Also apply to self if it's a healing or buff ability
            if (ability.category == AbilityCategory.Healing ||
                ability.category == AbilityCategory.Buff ||
                ability.targetType == AbilityTargetType.Self)
            {
                ApplyEffects(user, user);
            }
        }
        else
        {
            // Single target
            if (target != null)
                ApplyEffects(user, target);
            else if (ability.targetType == AbilityTargetType.Self || ability.targetSelection == TargetSelection.Self)
                ApplyEffects(user, user);

            // For Direction, GroundPlacement, Projectile - they don't need a target
            else if (ability.targetType == AbilityTargetType.Direction ||
                     ability.targetType == AbilityTargetType.GroundPlacement ||
                     ability.targetType == AbilityTargetType.Projectile)
            {
                // Apply effect at user position or forward direction
                ApplyEffects(user, user);

                // For projectile abilities, you might want to instantiate a projectile
                if (ability.targetType == AbilityTargetType.Projectile && ability.visualEffectPrefab != null)
                {
                    // Instantiate projectile that will travel forward
                    GameObject projectile = GameObject.Instantiate(ability.visualEffectPrefab,
                        user.transform.position + user.transform.forward * 2f,
                        user.transform.rotation);
                    // Add projectile movement logic here if needed
                }
            }
        }

        // Start cooldown
        cooldownTimer = ability.cooldown;

        // Play sound
        if (ability.soundEffect != null)
            AudioSource.PlayClipAtPoint(ability.soundEffect, user.transform.position);
    }

    private void ApplyEffects(GameObject user, GameObject target)
    {
        foreach (var effect in ability.effects)
        {
            if (effect != null)
            {
                if (ability.effectDelay > 0f && user.GetComponent<MonoBehaviour>() != null)
                    user.GetComponent<MonoBehaviour>().StartCoroutine(DelayedEffect(effect, user, target, ability.effectDelay));
                else
                    effect.Apply(user, target);
            }
        }
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
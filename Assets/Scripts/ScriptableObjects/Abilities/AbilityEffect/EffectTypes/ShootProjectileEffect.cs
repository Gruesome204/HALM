using UnityEngine;

[CreateAssetMenu(menuName = "Game/Abilities/Effects/Shoot Projectile")]
public class ShootProjectileEffect : AbilityEffect
{
    [Header("Projectile Settings")]
    [Tooltip("Prefab of the projectile to shoot. Must have ProjectileBehaviour component.")]
    public GameObject projectilePrefab;

    [Tooltip("Damage dealt by the projectile.")]
    public float damage = 10f;

    [Tooltip("Speed of the projectile.")]
    public float projectileSpeed = 10f;

    [Tooltip("Offset from the user's position to spawn the projectile.")]
    public Vector3 spawnOffset = Vector3.forward;

    [Header("Homing Settings")]
    [Tooltip("Should the projectile home in on the target?")]
    public bool isHoming = false;

    [Tooltip("Homing strength (how aggressively it turns toward target).")]
    [Range(0f, 10f)]
    public float homingStrength = 2f;

    [Header("Piercing Settings")]
    [Tooltip("How many enemies the projectile can pierce through.")]
    public int pierceCount = 0;

    [Header("Knockback Settings")]
    [Tooltip("Knockback strength applied to hit enemies.")]
    public float knockbackStrength = 5f;

    [Tooltip("Knockback duration in seconds.")]
    public float knockbackDuration = 0.5f;

    [Header("Chain Lightning Settings")]
    [Tooltip("How many enemies the chain can bounce to.")]
    public int chainBounceCount = 0;

    [Tooltip("Range to find next chain target.")]
    public float chainBounceRange = 5f;

    [Header("AOE Settings")]
    [Tooltip("Is this an AOE projectile?")]
    public bool isAOE = false;

    [Tooltip("Radius of the AOE effect.")]
    public float aoeRadius = 3f;

    [Tooltip("Visual effect for AOE explosion.")]
    public GameObject aoeEffectPrefab;

    [Header("Layer Settings")]
    [Tooltip("Layer mask for enemies.")]
    public LayerMask targetLayer;

    [Tooltip("Layer mask for walls/obstacles.")]
    public LayerMask wallLayer;

    [Tooltip("Layer mask for obstacles that block chain lightning.")]
    public LayerMask obstacleLayer;

    public override void Apply(GameObject user, GameObject target)
    {
        if (projectilePrefab == null || user == null)
            return;

        // Calculate spawn position
        Vector3 spawnPos = user.transform.position +
                          user.transform.TransformDirection(spawnOffset);

        // Spawn the projectile
        GameObject projectileObj = Instantiate(
            projectilePrefab,
            spawnPos,
            user.transform.rotation
        );

        // Get the ProjectileBehaviour component
        ProjectileBehaviour projectile = projectileObj.GetComponent<ProjectileBehaviour>();
        if (projectile == null)
        {
            Debug.LogError("Projectile prefab is missing ProjectileBehaviour component!");
            Destroy(projectileObj);
            return;
        }

        // Get the Rigidbody2D component
        Rigidbody2D rb = projectileObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Set initial velocity
            Vector2 direction = Vector2.right;
            if (target != null)
            {
                direction = (target.transform.position - spawnPos).normalized;
            }
            else
            {
                direction = user.transform.right;
            }

            rb.linearVelocity = direction * projectileSpeed;
        }

        // Initialize the projectile with all settings
        projectile.SetOwner(user, damage);

        // Set knockback
        projectile.SetKnockback(knockbackStrength, knockbackDuration);

        // Set layer masks
        projectile.SetLayerMasks(targetLayer, wallLayer, obstacleLayer);

        // Set piercing
        projectile.SetPiercing(pierceCount);

        // Set homing
        if (isHoming && target != null)
        {
            projectile.SetHoming(homingStrength, target.transform);
        }

        // Set chain lightning
        if (chainBounceCount > 0 && target != null)
        {
            projectile.SetChainLightning(chainBounceCount, chainBounceRange);
            projectile.SetChainTarget(target.transform);
        }

        // Set AOE
        if (isAOE)
        {
            projectile.SetAOE(null, aoeRadius);
        }

        Debug.Log($"Projectile fired from {user.name} targeting {(target != null ? target.name : "none")}");
    }
}
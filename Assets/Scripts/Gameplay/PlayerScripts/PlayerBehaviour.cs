using UnityEngine;
using UnityEngine.Localization.Settings;

public class PlayerBehaviour : MonoBehaviour
{
    // Cache all components in Awake
    private PlayerStats stats;
    private PlayerHealth health;
    private PlayerMovement movement;
    private PlayerAnimator animator;
    private PlayerInteraction interaction;

    private void Awake()
    {
        // Get all components once
        stats = GetComponent<PlayerStats>();
        health = GetComponent<PlayerHealth>();
        movement = GetComponent<PlayerMovement>();
        animator = GetComponentInChildren<PlayerAnimator>();
        interaction = GetComponent<PlayerInteraction>();

        // Validate all required components
        if (stats == null || health == null || movement == null)
        {
            Debug.LogError("Missing required Player components!");
            enabled = false;
            return;
        }

        Initialize();
    }

    private void Initialize()
    {
        stats.Initialize();
        health.OnDeath += HandleDeath;
    }

    private void HandleDeath(PlayerHealth playerHealth, DamageData damageData)
    {
        Debug.Log($"Player died from {damageData.type} damage.");

    }

    private void OnDestroy()
    {
        // Prevent memory leaks / orphaned event handlers
        health.OnDeath -= HandleDeath;
    }
}

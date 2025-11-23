using UnityEngine;
using UnityEngine.Localization.Settings;

public class PlayerBehaviour : MonoBehaviour
{
    private PlayerStats stats;
    private PlayerHealth health;
    private PlayerMovement movement;


    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        health = GetComponent<PlayerHealth>();
        movement = GetComponent<PlayerMovement>();

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

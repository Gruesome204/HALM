using UnityEngine;

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

        // Destroy or disable player object
        Destroy(gameObject);
    }
}

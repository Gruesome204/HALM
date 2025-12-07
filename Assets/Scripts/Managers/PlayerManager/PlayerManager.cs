using UnityEngine;
using System.Collections.Generic;
using System;
public class PlayerManager : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    private PlayerBehaviour playerBehaviour;
    public PlayerStats playerStats;
    public event Action OnPlayerDeath;
    public static PlayerManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // Auto-find PlayerHealth if not assigned in inspector
        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealth>();

        if(playerBehaviour == null)
        {
            playerBehaviour = FindAnyObjectByType<PlayerBehaviour>();
        }

        if (playerStats == null)
        {
            playerStats = FindAnyObjectByType<PlayerStats>();
        }
    }
    private void Start()
    {
        if (playerHealth != null)
        {
            // Subscribe to PlayerHealth death event
            playerHealth.OnDeath += HandlePlayerDeath;
        }
        else
        {
            Debug.LogWarning("[PlayerManager] No PlayerHealth found!");
        }
    }


    private void OnDestroy()
    {
        // Always unsubscribe to prevent memory leaks
        if (playerHealth != null)
            playerHealth.OnDeath -= HandlePlayerDeath;
    }
    private void HandlePlayerDeath(PlayerHealth player, DamageData damageData)
    {
        Debug.Log("[PlayerManager] Player died! Checking Game Over condition...");

        // Double-check that the player is truly dead
        if (!player.IsAlive())
        {
            Debug.Log("[PlayerManager] Player confirmed dead. Triggering Game Over event.");
            OnPlayerDeath?.Invoke(); // Fire event for UI or GameManager
        }
    }
}

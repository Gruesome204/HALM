using UnityEngine;
using System.Collections.Generic;
using System;
using System.ComponentModel;

public class PlayerManager : MonoBehaviour, IGameSystem
{
    // ========================
    // SINGLETON
    // ========================
    public static PlayerManager Instance { get; private set; }

    // ========================
    // REFERENCES
    // ========================
    [Header("Player References - Auto Loaded")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerBehaviour playerBehaviour;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private GameObject playerGameObject;

    // ========================
    // EVENTS
    // ========================
    public event Action OnPlayerDeath;
    public event Action<PlayerHealth, DamageData> OnPlayerDamaged;
    public event Action<float> OnHealthChanged;
    public event Action<DamageData, KnockbackData> OnDamageTaken;
    public event Action OnParrySuccess;
    public event Action OnPlayerSpawned;

    // ========================
    // STATE
    // ========================
    private bool isInitialized = false;
    private bool isPlayerAlive = true;
    private Vector3 spawnPosition = Vector3.zero;

    // ========================
    // PROPERTIES
    // ========================
    public PlayerHealth Health => playerHealth;
    public PlayerBehaviour Behaviour => playerBehaviour;
    public PlayerStats Stats => playerStats;
    public PlayerMovement Movement => playerMovement;
    public GameObject PlayerObject => playerGameObject;
    public bool IsAlive => isPlayerAlive;
    public bool IsInitialized => isInitialized;
    public Vector3 SpawnPosition => spawnPosition;
    public float CurrentHealth => playerStats != null ? playerStats.currentHealth : 0f;
    public float MaxHealth => playerStats != null ? playerStats.currentMaxHealth : 0f;

    // ========================
    // IGameSystem Implementation
    // ========================
    public int InitializePriority => 3;

    public void Initialize()
    {
        if (isInitialized)
        {
            Debug.Log("[PlayerManager] Already initialized.");
            return;
        }

        Debug.Log("[PlayerManager] Initializing...");

        // Find references if not assigned
        FindPlayerReferences();

        // Validate dependencies
        ValidateDependencies();

        // Reset state
        isPlayerAlive = true;
        spawnPosition = Vector3.zero;

        isInitialized = true;
        Debug.Log("[PlayerManager] Initialized successfully.");
    }

    public void PostInitialize()
    {
        Debug.Log("[PlayerManager] Post-Initializing...");

        // Apply modifiers from GameData
        ApplyModifiersFromGameData();

        // Subscribe to events
        SubscribeToEvents();

        // Initialize spawn position
        if (playerGameObject != null)
        {
            spawnPosition = playerGameObject.transform.position;
        }

        // Spawn the player if not already present
        if (playerGameObject == null)
        {
            SpawnPlayer();
        }

        OnPlayerSpawned?.Invoke();
        Debug.Log("[PlayerManager] Post-Initialization complete.");
    }

    // ========================
    // UNITY LIFECYCLE
    // ========================
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Auto-initialize if not already done
        if (!isInitialized)
        {
            Debug.LogWarning("[PlayerManager] Auto-initializing in Start()");
            Initialize();
            PostInitialize();
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
        isInitialized = false;
    }

    // ========================
    // REFERENCE FINDING
    // ========================
    private void FindPlayerReferences()
    {
        // Find Player GameObject
        if (playerGameObject == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerGameObject = player;
                Debug.Log("[PlayerManager] Found player GameObject by tag.");
            }
        }

        // Find PlayerHealth
        if (playerHealth == null)
        {
            if (playerGameObject != null)
            {
                playerHealth = playerGameObject.GetComponentInChildren<PlayerHealth>();
            }
            else
            {
                playerHealth = FindObjectOfType<PlayerHealth>();
            }

            if (playerHealth != null)
            {
                Debug.Log("[PlayerManager] Found PlayerHealth component.");
            }
        }

        // Find PlayerBehaviour
        if (playerBehaviour == null)
        {
            if (playerGameObject != null)
            {
                playerBehaviour = playerGameObject.GetComponentInChildren<PlayerBehaviour>();
            }
            else
            {
                playerBehaviour = FindObjectOfType<PlayerBehaviour>();
            }

            if (playerBehaviour != null)
            {
                Debug.Log("[PlayerManager] Found PlayerBehaviour component.");
            }
        }

        // Find PlayerStats
        if (playerStats == null)
        {
            if (playerGameObject != null)
            {
                playerStats = playerGameObject.GetComponentInChildren<PlayerStats>();
            }
            else
            {
                playerStats = FindObjectOfType<PlayerStats>();
            }

            if (playerStats != null)
            {
                Debug.Log("[PlayerManager] Found PlayerStats component.");
            }
        }

        // Find PlayerMovement
        if (playerMovement == null)
        {
            if (playerGameObject != null)
            {
                playerMovement = playerGameObject.GetComponentInChildren<PlayerMovement>();
            }
            else
            {
                playerMovement = FindObjectOfType<PlayerMovement>();
            }

            if (playerMovement != null)
            {
                Debug.Log("[PlayerManager] Found PlayerMovement component.");
            }
        }
    }

    // ========================
    // VALIDATION
    // ========================
    private void ValidateDependencies()
    {
        if (playerHealth == null)
        {
            Debug.LogError("[PlayerManager] PlayerHealth is missing! Player will not have health functionality.");
        }

        if (playerBehaviour == null)
        {
            Debug.LogWarning("[PlayerManager] PlayerBehaviour is missing! Player may not function correctly.");
        }

        if (playerStats == null)
        {
            Debug.LogWarning("[PlayerManager] PlayerStats is missing! Player may not have stats.");
        }

        if (playerMovement == null)
        {
            Debug.LogWarning("[PlayerManager] PlayerMovement is missing! Player may not move.");
        }
    }

    // ========================
    // EVENT SUBSCRIPTION
    // ========================
    private void SubscribeToEvents()
    {
        if (playerHealth != null)
        {
            // Subscribe to death event
            playerHealth.OnDeath += HandlePlayerDeath;

            // Subscribe to health changed event
            playerHealth.OnHealthChanged += HandleHealthChanged;

            // Subscribe to damage taken event
            playerHealth.OnDamageTakenEvent += HandleDamageTaken;

            // Subscribe to parry success event
            playerHealth.OnParrySuccess += HandleParrySuccess;

            Debug.Log("[PlayerManager] Subscribed to PlayerHealth events.");
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath -= HandlePlayerDeath;
            playerHealth.OnHealthChanged -= HandleHealthChanged;
            playerHealth.OnDamageTakenEvent -= HandleDamageTaken;
            playerHealth.OnParrySuccess -= HandleParrySuccess;
            Debug.Log("[PlayerManager] Unsubscribed from PlayerHealth events.");
        }
    }

    // ========================
    // EVENT HANDLERS
    // ========================
    private void HandlePlayerDeath(PlayerHealth player, DamageData damageData)
    {
        isPlayerAlive = false;
        Debug.Log($"[PlayerManager] Player died!");

        // Log death details - DamageData is a struct, so we check if it's default
        if (damageData.source != null || damageData.amount > 0)
        {
            Debug.Log($"[PlayerManager] Death details - Damage: {damageData.amount}, " +
                      $"Type: {damageData.type}, " +
                      $"Source: {damageData.source?.name ?? "Unknown"}");
        }

        OnPlayerDeath?.Invoke();
    }

    private void HandleHealthChanged(float currentHealth)
    {
        OnHealthChanged?.Invoke(currentHealth);
    }

    private void HandleDamageTaken(DamageData damageData, KnockbackData knockbackData)
    {
        Debug.Log($"[PlayerManager] Player took {damageData.amount} {damageData.type} damage.");
        OnDamageTaken?.Invoke(damageData, knockbackData);
    }

    private void HandleParrySuccess()
    {
        Debug.Log("[PlayerManager] Player parried successfully!");
        OnParrySuccess?.Invoke();
    }

    // ========================
    // MODIFIER APPLICATION
    // ========================
    private void ApplyModifiersFromGameData()
    {
        if (GameManager.Instance?.gameDataSO == null)
        {
            Debug.LogWarning("[PlayerManager] GameDataSO not found. Cannot apply modifiers.");
            return;
        }

        if (playerStats == null)
        {
            Debug.LogWarning("[PlayerManager] PlayerStats not found. Cannot apply modifiers.");
            return;
        }

        var modifiers = GameManager.Instance.gameDataSO.buildMasterModifiers;
        if (modifiers == null || modifiers.Count == 0)
        {
            Debug.Log("[PlayerManager] No modifiers to apply to player.");
            return;
        }

        int appliedCount = 0;
        foreach (BuildMasterModifier modifier in modifiers)
        {
            if (modifier?.options != null)
            {
                playerStats.AddModifier(modifier.options);
                appliedCount++;
                Debug.Log($"[PlayerManager] Applied modifier to player: {modifier.options.name}");
            }
        }

        Debug.Log($"[PlayerManager] Applied {appliedCount} modifiers to player.");
    }

    // ========================
    // PLAYER MANAGEMENT
    // ========================
    public void SpawnPlayer()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[PlayerManager] Cannot spawn player - not initialized!");
            return;
        }

        if (playerGameObject != null)
        {
            Debug.Log("[PlayerManager] Player already exists. Reviving instead.");
            RevivePlayer();
            return;
        }

        // Find the player in scene if it exists
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerGameObject = player;
            FindPlayerReferences();
            SubscribeToEvents();
            isPlayerAlive = true;
            Debug.Log("[PlayerManager] Found existing player in scene.");
        }
        else
        {
            Debug.LogError("[PlayerManager] No player found in scene!");
            return;
        }

        if (spawnPosition != Vector3.zero && playerGameObject != null)
        {
            playerGameObject.transform.position = spawnPosition;
        }

        OnPlayerSpawned?.Invoke();
        Debug.Log("[PlayerManager] Player spawned successfully.");
    }

    public void RevivePlayer()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[PlayerManager] Cannot revive player - not initialized!");
            return;
        }

        if (playerHealth != null && playerStats != null)
        {
            // Heal the player to full
            float maxHealth = playerStats.currentMaxHealth;
            playerHealth.Heal(maxHealth);
            isPlayerAlive = true;
            Debug.Log($"[PlayerManager] Player revived with {maxHealth} health.");
        }
        else
        {
            Debug.LogWarning("[PlayerManager] Cannot revive player - PlayerHealth or PlayerStats missing!");
        }
    }

    public void RespawnPlayer()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[PlayerManager] Cannot respawn player - not initialized!");
            return;
        }

        if (playerHealth != null && !playerHealth.IsAlive())
        {
            RevivePlayer();
        }
        else
        {
            SpawnPlayer();
        }

        // Reset player position to spawn point
        if (playerGameObject != null && spawnPosition != Vector3.zero)
        {
            playerGameObject.transform.position = spawnPosition;
        }
    }

    public void SetSpawnPosition(Vector3 position)
    {
        spawnPosition = position;
        Debug.Log($"[PlayerManager] Spawn position set to {position}");
    }

    public void ResetSpawnPosition()
    {
        spawnPosition = Vector3.zero;
        Debug.Log("[PlayerManager] Spawn position reset.");
    }

    // ========================
    // HEALTH MANAGEMENT
    // ========================
    public void HealPlayer(float amount)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[PlayerManager] Cannot heal - not initialized!");
            return;
        }

        if (playerHealth != null)
        {
            playerHealth.Heal(amount);
            Debug.Log($"[PlayerManager] Healed player for {amount} health.");
        }
        else
        {
            Debug.LogWarning("[PlayerManager] Cannot heal - PlayerHealth missing!");
        }
    }

    public void DamagePlayer(float amount, GameObject source = null, DamageData.DamageType damageType = DamageData.DamageType.Physical)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[PlayerManager] Cannot damage - not initialized!");
            return;
        }

        if (playerHealth != null)
        {
            DamageData damageData = new DamageData
            {
                amount = amount,
                source = source,
                type = damageType
            };

            KnockbackData knockbackData = new KnockbackData(); // Use default knockback

            playerHealth.TakeDamage(damageData, knockbackData);
            Debug.Log($"[PlayerManager] Damaged player for {amount} {damageType} damage.");
        }
        else
        {
            Debug.LogWarning("[PlayerManager] Cannot damage - PlayerHealth missing!");
        }
    }

    public void SetInvulnerable(float duration)
    {
        if (playerHealth != null)
        {
            playerHealth.SetInvulnerable(duration);
            Debug.Log($"[PlayerManager] Player invulnerable for {duration} seconds.");
        }
    }

    // ========================
    // STATS MANAGEMENT
    // ========================
    public void ApplyModifierToPlayer(BuildMasterModifier.Modifier modifier)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[PlayerManager] Cannot apply modifier - not initialized!");
            return;
        }

        if (playerStats == null)
        {
            Debug.LogWarning("[PlayerManager] PlayerStats missing. Cannot apply modifier.");
            return;
        }

        playerStats.AddModifier(modifier);
        Debug.Log($"[PlayerManager] Applied modifier to player: {modifier.name}");
    }

    public void RemoveModifierFromPlayer(BuildMasterModifier.Modifier modifier)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[PlayerManager] Cannot remove modifier - not initialized!");
            return;
        }

        if (playerStats == null)
        {
            Debug.LogWarning("[PlayerManager] PlayerStats missing. Cannot remove modifier.");
            return;
        }

        playerStats.RemoveModifier(modifier);
        Debug.Log($"[PlayerManager] Removed modifier from player: {modifier.name}");
    }

    public void RefreshPlayerStats()
    {
        if (playerStats != null)
        {
            playerStats.RecalculateStats(true);
            Debug.Log("[PlayerManager] Refreshed player stats.");
        }
        else
        {
            Debug.LogWarning("[PlayerManager] Cannot refresh stats - PlayerStats missing!");
        }
    }

    // ========================
    // UTILITY METHODS
    // ========================
    public bool IsPlayerInRange(Vector3 position, float range)
    {
        if (playerGameObject == null) return false;
        return Vector3.Distance(playerGameObject.transform.position, position) <= range;
    }

    public Vector3 GetPlayerPosition()
    {
        if (playerGameObject != null)
        {
            return playerGameObject.transform.position;
        }
        return Vector3.zero;
    }

    public Quaternion GetPlayerRotation()
    {
        if (playerGameObject != null)
        {
            return playerGameObject.transform.rotation;
        }
        return Quaternion.identity;
    }

    public float GetDistanceToPlayer(Vector3 position)
    {
        if (playerGameObject == null) return float.MaxValue;
        return Vector3.Distance(playerGameObject.transform.position, position);
    }

    public void CallParrySuccess()
    {
        if (playerHealth != null)
        {
            playerHealth.CallParrySuccess();
        }
    }

    // ========================
    // DEBUG HELPERS
    // ========================
    public void DebugPlayerState()
    {
        if (!isInitialized)
        {
            Debug.Log("[PlayerManager] Not initialized!");
            return;
        }

        Debug.Log("[PlayerManager] Player State:");
        Debug.Log($"  - Is Alive: {isPlayerAlive}");
        Debug.Log($"  - Position: {GetPlayerPosition()}");
        Debug.Log($"  - GameObject: {(playerGameObject != null ? playerGameObject.name : "Null")}");
        Debug.Log($"  - Health Component: {(playerHealth != null ? "Present" : "Missing")}");
        Debug.Log($"  - Behaviour Component: {(playerBehaviour != null ? "Present" : "Missing")}");
        Debug.Log($"  - Stats Component: {(playerStats != null ? "Present" : "Missing")}");
        Debug.Log($"  - Movement Component: {(playerMovement != null ? "Present" : "Missing")}");

        if (playerStats != null)
        {
            Debug.Log($"  - Current Health: {playerStats.currentHealth}");
            Debug.Log($"  - Max Health: {playerStats.currentMaxHealth}");
            Debug.Log($"  - Current Armor: {playerStats.currentArmor}");
            Debug.Log($"  - Magic Resistance: {playerStats.currentMagicResistance}");
        }
    }

    // ========================
    // GAME STATE HANDLING
    // ========================
    public void OnGamePaused(bool paused)
    {
        // Handle game pause if needed
        if (paused)
        {
            // Optionally pause player updates
        }
    }

    public void OnGameReset()
    {
        if (!isInitialized) return;

        // Reset player state
        isPlayerAlive = true;
        spawnPosition = Vector3.zero;

        // Respawn player
        RespawnPlayer();

        // Reapply modifiers
        ApplyModifiersFromGameData();

        Debug.Log("[PlayerManager] Game reset complete.");
    }

    // ========================
    // SCENE MANAGEMENT
    // ========================
    public void OnSceneLoaded()
    {
        // Find player references again after scene load
        FindPlayerReferences();
        SubscribeToEvents();

        // Reset spawn position to current scene's spawn point
        spawnPosition = Vector3.zero;

        Debug.Log("[PlayerManager] Scene loaded - references refreshed.");
    }
}
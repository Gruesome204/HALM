using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HubManager : MonoBehaviour
{
    public static HubManager Instance;

    [Header("Data References")]
    public GameDataSO gameDataSO;          // Reference to your ScriptableObject

    public int maxSelectedTurrets;

    public event Action<TurretBlueprint> OnTurretUnlocked;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // keep between scenes if needed
    }

    private void Start()
    {
        maxSelectedTurrets = gameDataSO.limitOfSelectableTurrets;
    }

    // --- Refactored to use TurretBlueprint ---
    public bool UnlockTurret(TurretBlueprint blueprint)
    {
        if (blueprint == null) return false;

        if (!gameDataSO.GetUnlockedBlueprints().Contains(blueprint))
        {
            gameDataSO.GetUnlockedBlueprints().Add(blueprint);
            OnTurretUnlocked?.Invoke(blueprint);
            Debug.Log($"Unlocked turret: {blueprint.name}");
            return true;
        }
        return false;
    }

    public bool IsUnlocked(TurretBlueprint blueprint) => blueprint != null && gameDataSO.GetUnlockedBlueprints().Contains(blueprint);

    public List<TurretBlueprint> GetUnlockedTurrets()
    {
        return new List<TurretBlueprint>(gameDataSO.GetUnlockedBlueprints());
    }
}

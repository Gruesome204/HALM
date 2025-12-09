using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HubManager : MonoBehaviour
{
    public static HubManager Instance;

    [Header("Data References")]
    public GameDataSO gameData;          // Reference to your ScriptableObject

    public int maxSelectedTurrets;

    public event Action<TurretBlueprint> OnTurretUnlocked;
    public event Action<TurretBlueprint> OnTurretSelected;
    public event Action<TurretBlueprint> OnTurretDeselected;

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
        maxSelectedTurrets = gameData.limitOfSelectableTurrets;
    }

    // --- Refactored to use TurretBlueprint ---
    public bool UnlockTurret(TurretBlueprint blueprint)
    {
        if (blueprint == null) return false;

        if (!gameData.unlockedBlueprints.Contains(blueprint))
        {
            gameData.unlockedBlueprints.Add(blueprint);
            OnTurretUnlocked?.Invoke(blueprint);
            Debug.Log($"Unlocked turret: {blueprint.name}");
            return true;
        }
        return false;
    }

    public bool SelectTurret(TurretBlueprint blueprint)
    {
        if (blueprint == null) return false;

        if (!gameData.unlockedBlueprints.Contains(blueprint))
        {
            Debug.LogWarning($"Turret {blueprint.name} is not unlocked!");
            return false;
        }
        if (gameData.selectedBlueprints.Contains(blueprint))
        {
            Debug.LogWarning($"{blueprint.name} is already selected!");
            return false;
        }
        if (gameData.selectedBlueprints.Count >= maxSelectedTurrets)
        {
            Debug.LogWarning("Max selected turrets reached!");
            return false;
        }

        gameData.selectedBlueprints.Add(blueprint);
        OnTurretSelected?.Invoke(blueprint);
        return true;
    }

    public void DeselectTurret(TurretBlueprint blueprint)
    {
        if (blueprint == null) return;

        if (gameData.selectedBlueprints.Contains(blueprint))
        {
            gameData.selectedBlueprints.Remove(blueprint);
            OnTurretDeselected?.Invoke(blueprint);
        }
    }

    public bool IsUnlocked(TurretBlueprint blueprint) => blueprint != null && gameData.unlockedBlueprints.Contains(blueprint);
    public bool IsSelected(TurretBlueprint blueprint) => blueprint != null && gameData.selectedBlueprints.Contains(blueprint);

    public List<TurretBlueprint> GetAvailableTurrets()
    {
        return gameData.unlockedBlueprints
            .Where(bp => !gameData.selectedBlueprints.Contains(bp))
            .ToList();
    }

    public List<TurretBlueprint> GetSelectedTurrets()
    {
        return new List<TurretBlueprint>(gameData.selectedBlueprints);
    }

    public List<TurretBlueprint> GetUnlockedTurrets()
    {
        return new List<TurretBlueprint>(gameData.unlockedBlueprints);
    }
}

using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;

public class HubManager : MonoBehaviour
{
    public static HubManager Instance;

    [Header("Data References")]
    public GameDataSO gameData;          // Reference to your ScriptableObject

    public int maxSelectedTurrets = 4;

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

    public bool UnlockTurret(TurretBlueprint turret)
    {
        if (!gameData.IsUnlocked(turret))
        {
            gameData.unlockedTurrets.Add(turret);
            Debug.Log($"Unlocked turret: {turret.turretName}");
            OnTurretUnlocked?.Invoke(turret); // Notify listeners
            return true;
        }
        return false;
    }
    public bool SelectTurret(TurretBlueprint turret)
    {
        if (!gameData.IsUnlocked(turret))
        {
            Debug.LogWarning($"{turret.turretName} is not unlocked!");
            return false;
        }

        if (gameData.selectedTurrets.Contains(turret))
        {
            Debug.LogWarning($"{turret.turretName} is already selected!");
            return false;
        }

        if (gameData.selectedTurrets.Count >= maxSelectedTurrets)
        {
            Debug.LogWarning("Max selected turrets reached!");
            return false;
        }

        gameData.selectedTurrets.Add(turret);
        Debug.Log($"Selected turret: {turret.turretName}");
        OnTurretSelected?.Invoke(turret);
        return true;
    }

    public void DeselectTurret(TurretBlueprint turret)
    {
        if (gameData.selectedTurrets.Contains(turret))
        {
            gameData.selectedTurrets.Remove(turret);
            Debug.Log($"Deselected turret: {turret.turretName}");
            OnTurretDeselected?.Invoke(turret);
        }
    }    
    
    
    // Check if a turret is unlocked
    public bool IsUnlocked(TurretBlueprint turret) => gameData.IsUnlocked(turret);

    // Check if a turret is currently selected
    public bool IsSelected(TurretBlueprint turret) => gameData.IsSelected(turret);
    // Returns turrets that are unlocked but not selected
    public List<TurretBlueprint> GetAvailableTurrets()
    {
        List<TurretBlueprint> available = new List<TurretBlueprint>();
        foreach (var turret in gameData.unlockedTurrets)
        {
            if (!gameData.selectedTurrets.Contains(turret))
                available.Add(turret);
        }
        return available;
    }

    // Deselect all selected turrets
    public void ResetSelection()
    {
        for (int i = gameData.selectedTurrets.Count - 1; i >= 0; i--)
        {
            DeselectTurret(gameData.selectedTurrets[i]);
        }
    }

    // Get all selected turrets
    public List<TurretBlueprint> GetSelectedTurrets() => new List<TurretBlueprint>(gameData.selectedTurrets);

    // Get all unlocked turrets
    public List<TurretBlueprint> GetUnlockedTurrets() => new List<TurretBlueprint>(gameData.unlockedTurrets);

    //Debug / Test Methods
    [ContextMenu("Unlock All Turrets")]
    public void UnlockAllTurrets()
    {
        foreach (var turret in gameData.allTurrets)
            UnlockTurret(turret);
    }
}

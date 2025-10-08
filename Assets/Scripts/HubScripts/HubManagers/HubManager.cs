using NUnit.Framework.Internal;
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

    public bool UnlockTurret(TurretType type)
    {
        if (!gameData.unlockedTurrets.Contains(type))
        {
            gameData.unlockedTurrets.Add(type);
            TurretBlueprint bp = gameData.GetBlueprint(type);
            if (bp != null)
                OnTurretUnlocked?.Invoke(bp);
            Debug.Log($"Unlocked turret type: {type}");
            return true;
        }
        return false;
    }

    public bool SelectTurret(TurretType type)
    {
        if (!gameData.unlockedTurrets.Contains(type))
        {
            Debug.LogWarning($"Turret type {type} is not unlocked!");
            return false;
        }
        if (gameData.selectedTurrets.Contains(type))
        {
            Debug.LogWarning($"{type} is already selected!");
            return false;
        }
        if (gameData.selectedTurrets.Count >= maxSelectedTurrets)
        {
            Debug.LogWarning("Max selected turrets reached!");
            return false;
        }

        gameData.selectedTurrets.Add(type);
        TurretBlueprint bp = gameData.GetBlueprint(type);
        if (bp != null)
            OnTurretSelected?.Invoke(bp);
        return true;
    }

    public void DeselectTurret(TurretType type)
    {
        if (gameData.selectedTurrets.Contains(type))
        {
            gameData.selectedTurrets.Remove(type);
            TurretBlueprint bp = gameData.GetBlueprint(type);
            if (bp != null)
                OnTurretDeselected?.Invoke(bp);
        }
    }

    public bool IsUnlocked(TurretType type) => gameData.unlockedTurrets.Contains(type);
    public bool IsSelected(TurretType type) => gameData.selectedTurrets.Contains(type);

    public List<TurretBlueprint> GetAvailableTurrets()
    {
        return gameData.unlockedTurrets
            .Where(t => !gameData.selectedTurrets.Contains(t))
            .Select(t => gameData.GetBlueprint(t))
            .Where(bp => bp != null)
            .ToList();
    }

    public List<TurretBlueprint> GetSelectedTurrets()
    {
        return gameData.selectedTurrets
            .Select(t => gameData.GetBlueprint(t))
            .Where(bp => bp != null)
            .ToList();
    }

    public List<TurretBlueprint> GetUnlockedTurrets()
    {
        return gameData.unlockedTurrets
            .Select(t => gameData.GetBlueprint(t))
            .Where(bp => bp != null)
            .ToList();
    }
}

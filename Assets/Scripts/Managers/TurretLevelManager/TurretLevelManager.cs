using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class TurretLevelManager : MonoBehaviour
{
    public static TurretLevelManager Instance { get; private set; }

    [Serializable]
    public class TurretProgress
    {
     [SerializeField] public int currentLevel = 1;
     [SerializeField] public float currentXP = 0;
     [SerializeField] public float xpToNextLevel = 100f;
    }

    private Dictionary<TurretType, TurretProgress> turretProgressDict = new();

    [Header("Level Settings")]
    public int maxLevel = 10;
    public float xpGrowthMultiplier = 1.2f;

    // Level-up event
    public delegate void LevelUpEvent(TurretType type, int newLevel);
    public event LevelUpEvent OnLevelUp;

    // Milestone event
    public delegate void MilestoneEvent(TurretType type, int level);
    public event MilestoneEvent OnMilestoneReached;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (TurretUpgradeChoiceManager.Instance == null)
        {
            Debug.LogError("[LevelManager] TurretUpgradeChoiceManager not found in scene!");
        }

        // Initialize turretProgressDict
        foreach (TurretType type in Enum.GetValues(typeof(TurretType)))
        {
            turretProgressDict[type] = new TurretProgress();
        }
    }

    /// <summary>Adds XP to a turret type and handles leveling</summary>
    public void AddXP(TurretType type, float amount)
    {
        var progress = turretProgressDict[type];
        if (progress.currentLevel >= maxLevel) return;

        progress.currentXP += amount;

        while (progress.currentXP >= progress.xpToNextLevel && progress.currentLevel < maxLevel)
        {
            progress.currentXP -= progress.xpToNextLevel;
            progress.currentLevel++;
            progress.xpToNextLevel *= xpGrowthMultiplier;

            OnLevelUp?.Invoke(type, progress.currentLevel);

            // Apply upgrades automatically
            ApplyUpgradesForLevel(type, progress.currentLevel);
        }
    }

    private void ApplyUpgradesForLevel(TurretType type, int level)
    {
        var options = TurretUpgradeChoiceManager.Instance.GetAvailableOptionsForLevel(type, level);

        foreach (var option in options)
        {
            var mod = option.modifier;
            Debug.Log($"[LevelManager] Upgrade Option: {option.name} | " +
                      $"Damage x{mod.damageMultiplier} | FireRate x{mod.fireRateMultiplier} | " +
                      $"Range +{mod.rangeBonus} | Projectiles +{mod.projectilesPerSalve} | Speed x{mod.projectileSpeed}");
        }

        if (options.Any())
        {
            OnMilestoneReached?.Invoke(type, level);
        }

        // Automatically force all turrets of this type to reapply upgrades
        ForceReapplyUpgrades(type);
    }
    public int GetLevel(TurretType type)
    {
        return turretProgressDict[type].currentLevel;
    }

    // Forces all active turrets of the specified type to reapply their upgrades.
    public void ForceReapplyUpgrades(TurretType type)
    {
        foreach (var turret in TurretPlacementController.Instance.GetActiveTurrets())
        {
            if (turret == null) continue;

            var levelBehaviour = turret.GetComponentInChildren<TurretLevelBehaviour>();
            if (levelBehaviour != null && levelBehaviour.blueprint != null &&
                levelBehaviour.blueprint.turretType == type)
            {
                levelBehaviour.SyncWithCurrentLevel();
            }
        }
    }
    // Convenience method to trigger upgrade reapplication.
    public void OnTurretLevelChanged(TurretType type)
    {
        ForceReapplyUpgrades(type);
    }

    public float GetProgressPercentage(TurretType type)
    {
        var progress = turretProgressDict[type];
        return Mathf.Clamp01(progress.currentXP / progress.xpToNextLevel);
    }   
    public void DebugAllTurretLevels()
    {
        foreach (var kvp in turretProgressDict)
        {
            Debug.Log($"{kvp.Key}: Level {kvp.Value.currentLevel}, XP {kvp.Value.currentXP}/{kvp.Value.xpToNextLevel}");
        }
    }


}

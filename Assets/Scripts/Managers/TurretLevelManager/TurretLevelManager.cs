using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TurretLevelManager : MonoBehaviour
{
    public static TurretLevelManager Instance { get; private set; }

    [System.Serializable]
    public class TurretProgress
    {
     [SerializeField] public int currentLevel = 1;
     [SerializeField] public float currentXP = 0;
     [SerializeField] public float xpToNextLevel = 50f;
    }

    private Dictionary<TurretType, TurretProgress> turretProgressDict = new();

    [Header("Level Settings")]
    public int maxLevel = 10;
    public float xpGrowthMultiplier = 1.5f;

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
    private void Start()
    {
        OnMilestoneReached += (type, level) => Debug.Log($"[Test] Milestone triggered for {type} at level {level}");
    }

    public void AddXP(TurretType type, float amount)
    {
        var progress = turretProgressDict[type];
        if (progress.currentLevel >= maxLevel) return;

        progress.currentXP += amount;

        while (progress.currentXP >= progress.xpToNextLevel && progress.currentLevel < maxLevel)
        {
            progress.currentXP -= progress.xpToNextLevel;
            LevelUp(type);
        }
    }

    private void LevelUp(TurretType type)
    {
        var progress = turretProgressDict[type];
        progress.currentLevel++;
        progress.currentXP = 0;
        progress.xpToNextLevel *= xpGrowthMultiplier;

        Debug.Log($"[LevelUpManager] {type} Turrets leveled up to {progress.currentLevel}");

        OnLevelUp?.Invoke(type, progress.currentLevel);

        foreach (var option in TurretUpgradeChoiceManager.Instance.GetAllOptionsForLevel(type, progress.currentLevel))
        {
            Debug.Log($"Option: {option.name} | Damage x{option.damageMultiplier} | " +
                      $"FireRate x{option.fireRateMultiplier} | Range +{option.rangeBonus}");
        }
    }
    public int GetLevel(TurretType type)
    {
        return turretProgressDict[type].currentLevel;
    }

}

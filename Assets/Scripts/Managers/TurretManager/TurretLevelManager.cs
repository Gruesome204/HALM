using System;
using System.Collections.Generic;
using UnityEngine;

public class TurretLevelManager : MonoBehaviour
{
    public static TurretLevelManager Instance { get; private set; }

    public event System.Action<TurretType, int> OnMilestoneReached;

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

    public delegate void LevelUpEvent(TurretType type, int newLevel);
    public event LevelUpEvent OnLevelUp;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Init dictionary
        foreach (TurretType type in System.Enum.GetValues(typeof(TurretType)))
        {
            turretProgressDict[type] = new TurretProgress();
        }
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

        Debug.Log($"[Manager] {type} Turrets leveled up to {progress.currentLevel}");

        OnLevelUp?.Invoke(type, progress.currentLevel);

        foreach (var choice in TurretUpgradeChoiceManager.Instance.GetUpgradeChoices(type))
        {
            if (choice.triggerLevel == progress.currentLevel)
            {
                OnMilestoneReached?.Invoke(type, progress.currentLevel);
                break;
            }
        }
    }

    public int GetLevel(TurretType type)
    {
        return turretProgressDict[type].currentLevel;
    }



}

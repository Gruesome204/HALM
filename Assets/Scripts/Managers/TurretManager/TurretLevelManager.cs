using System;
using System.Collections.Generic;
using UnityEngine;

public class TurretLevelManager : MonoBehaviour
{
    public static TurretLevelManager Instance { get; private set; }

    [System.Serializable]
    public class TurretProgress
    {
        public int currentLevel = 1;
        public float currentXP = 0;
        public float xpToNextLevel = 50f;
    }

    private Dictionary<TurretTypeData.TurretType, TurretProgress> turretProgressDict = new();

    [Header("Level Settings")]
    public int maxLevel = 10;
    public float xpGrowthMultiplier = 1.5f;

    public delegate void LevelUpEvent(TurretTypeData type, int newLevel);
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

    public void AddXP(TurretTypeData.TurretType type, float amount)
    {
        var progress = turretProgressDict[type];
        if (progress.currentLevel >= maxLevel) return;

        progress.currentXP += amount;

        if (progress.currentXP >= progress.xpToNextLevel)
        {
            LevelUp(type);
        }
    }

    private void LevelUp(TurretTypeData.TurretType type)
    {
        var progress = turretProgressDict[type];
        progress.currentLevel++;
        progress.currentXP = 0;
        progress.xpToNextLevel *= xpGrowthMultiplier;

        Debug.Log($"[Manager] {type} Turrets leveled up to {progress.currentLevel}");

        OnLevelUp?.Invoke(type, progress.currentLevel);
    }

    public int GetLevel(TurretTypeData.TurretType type)
    {
        return turretProgressDict[type].currentLevel;
    }

    internal void AddXP()
    {
        throw new NotImplementedException();
    }
}

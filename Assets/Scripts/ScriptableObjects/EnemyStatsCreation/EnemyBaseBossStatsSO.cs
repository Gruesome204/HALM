using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New BossStats", menuName = "Game/Enemy/New BossStats")]
public class EnemyBaseBossStatsSO : EnemyBaseStats
{
    [Header("Boss Properties")]
    public string bossBarName;
    public Color bossBarColor = Color.red;
    public Sprite bossPortrait;
    public float bossBarHeight = 20f; // UI height in pixels

    [Header("Boss Phases")]
    public bool isMultiStageBoss = false;
    public BossPhaseConfig[] phaseConfigs = new BossPhaseConfig[0];

    [Header("Boss Specific")]
    [Tooltip("Time before boss enrages and becomes more powerful")]
    public float enrageTimer = 0f; // 0 = no enrage

    [Tooltip("Additional damage multiplier when enraged")]
    public float enrageDamageMultiplier = 1.5f;

    [Header("Boss Loot")]
    public GameObject[] exclusiveLootTable;
    public float guaranteedDropRate = 1f;

    private void OnEnable()
    {
        enemyType = EnemyType.Boss;
    }

    // Override for boss-specific scaling
    public override float GetScaledHealth(int level)
    {
        // Bosses might have more aggressive health scaling
        return baseMaxHealth + (level - 1) * baseHealthScaleFactor * 1.5f;
    }

    public BossPhaseConfig GetPhaseConfig(int phaseIndex)
    {
        if (phaseIndex >= 0 && phaseIndex < phaseConfigs.Length)
            return phaseConfigs[phaseIndex];
        return null;
    }

    public float GetPhaseHealthThreshold(int phaseIndex)
    {
        var config = GetPhaseConfig(phaseIndex);
        return config != null ? config.healthThreshold : 0f;
    }

    public bool TryGetPhaseAtHealthPercent(float healthPercent, out int phaseIndex)
    {
        phaseIndex = -1;

        if (!isMultiStageBoss || phaseConfigs.Length == 0)
            return false;

        // Find the highest phase threshold that the health is below
        for (int i = phaseConfigs.Length - 1; i >= 0; i--)
        {
            if (healthPercent <= phaseConfigs[i].healthThreshold)
            {
                phaseIndex = i;
                return true;
            }
        }

        // If health is above all thresholds, we're in phase 0
        phaseIndex = 0;
        return true;
    }
}

[Serializable]
public class BossPhaseConfig
{
    [Header("Phase Identification")]
    [Tooltip("Display name for this phase")]
    public string phaseName = "Phase 1";

    [Tooltip("Health threshold to trigger this phase (0-1)")]
    [Range(0f, 1f)]
    public float healthThreshold = 0.75f;

    public string phaseBossName;

    [Header("Phase Effects")]
    [Tooltip("Amount of health to heal when entering this phase")]
    public float healAmount = 0f;

    [Tooltip("Aggression multiplier for this phase (1 = normal)")]
    [Range(0.5f, 3f)]
    public float aggressionMultiplier = 1f;

    [Tooltip("Damage multiplier for this phase (1 = normal)")]
    [Range(0.5f, 3f)]
    public float damageMultiplier = 1f;

    [Tooltip("Movement speed multiplier for this phase (1 = normal)")]
    [Range(0.5f, 2f)]
    public float speedMultiplier = 1f;

    [Header("Phase Events")]
    [Tooltip("Delay before applying phase effects")]
    public float effectDelay = 0.5f;

}
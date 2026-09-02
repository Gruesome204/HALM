using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New BossStats", menuName = "Game/Enemy/New BossStats")]
public class EnemyBaseBossStatsSO : EnemyBaseStats
{
    [Header("Boss Properties")]
    public string bossBarName;
    public Color bossBarColor = Color.red;
    public Sprite bossPortrait;
    public float bossBarHeight = 20f;

    [Header("Boss Phases")]
    public bool isMultiStageBoss = false;

    [Tooltip("Phase configurations - add as many phases as needed")]
    public PhaseConfig[] phases = new PhaseConfig[0];

    [Header("Boss Specific")]
    [Tooltip("Time before boss enrages and becomes more powerful")]
    public float enrageTimer = 0f;

    [Tooltip("Additional damage multiplier when enraged")]
    public float enrageDamageMultiplier = 1.5f;

    [Header("Boss Loot")]
    public GameObject[] exclusiveLootTable;
    public float guaranteedDropRate = 1f;

    private void OnEnable()
    {
        enemyType = EnemyType.Boss;
    }

    public override float GetScaledHealth(int level)
    {
        return baseMaxHealth + (level - 1) * baseHealthScaleFactor * 1.5f;
    }

}

[System.Serializable]
public class PhaseConfig
{
    [Header("Phase Settings")]
    [Tooltip("Health threshold to trigger this phase (0-1)")]
    [Range(0f, 1f)]
    public float healthThreshold = 0.5f;

    [Tooltip("Name of this phase (displayed in UI)")]
    public string phaseName = "Phase 1";

    [Tooltip("Description of this phase (for UI)")]
    public string phaseDescription = "";

    [Tooltip("Color for this phase in the UI")]
    public Color phaseColor = Color.white;

    [Tooltip("Icon/Sprite for this phase")]
    public Sprite phaseIcon;

    [Header("Phase Behavior")]
    [Tooltip("Aggression multiplier for this phase")]
    [Range(0.5f, 5f)]
    public float aggressionMultiplier = 1f;

    [Tooltip("Damage multiplier for this phase")]
    [Range(0.5f, 5f)]
    public float damageMultiplier = 1f;

    [Tooltip("Speed multiplier for this phase")]
    [Range(0.5f, 3f)]
    public float speedMultiplier = 1f;

    [Tooltip("Heal amount when entering this phase")]
    public float healOnEnter = 200f;

    [Tooltip("New abilities to unlock in this phase")]
    public AbilityConfig[] unlockedAbilities = new AbilityConfig[0];

    [Tooltip("Effects to play when entering phase")]
    public GameObject[] phaseEntryEffects;

    [Tooltip("Message to show to player")]
    public string phaseAnnouncement = "";
}

[System.Serializable]
public class AbilityConfig
{
    public string abilityName;
    public float cooldownReduction = 0f;
    public float damageBoost = 0f;
    public bool isUnlocked = true;
}
using UnityEngine;

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
    public int numberOfPhases = 1;

    [Header("Boss Specific")]
    [Tooltip("Time before boss enrages and becomes more powerful")]
    public float enrageTimer = 0f; // 0 = no enrage

    [Tooltip("Additional damage multiplier when enraged")]
    public float enrageDamageMultiplier = 1.5f;

    [Tooltip("Health threshold for phase transitions (0-1)")]
    public float[] phaseThresholds = new float[0]; // e.g., 0.75f, 0.5f, 0.25f

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

    public float GetPhaseHealthThreshold(int phaseIndex)
    {
        if (phaseIndex < phaseThresholds.Length)
            return phaseThresholds[phaseIndex];
        return 0f;
    }
}
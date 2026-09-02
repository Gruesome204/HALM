
using UnityEngine;

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
    public PhaseConfig[] phases = new PhaseConfig[0];

    [Header("Boss Specific")]
    public float enrageTimer = 0f;
    public float enrageDamageMultiplier = 1.5f;

    [Header("Boss Loot")]
    public GameObject[] exclusiveLootTable;
    public float guaranteedDropRate = 1f;

    // Override scaling with boss-specific multipliers
    public override float GetScaledHealth(int level)
    {
        // Bosses get 50% more health scaling
        return baseMaxHealth + (level - 1) * baseHealthScaleFactor * 1.5f;
    }

    public override float GetScaledDamage(int level)
    {
        // Bosses get 20% more damage scaling (optional)
        return baseDamage + (level - 1) * baseDamageScaleFactor * 1.2f;
    }

    private void OnEnable()
    {
        enemyType = EnemyType.Boss;
    }


    [System.Serializable]
    public class PhaseConfig
    {
        [Header("Phase Settings")]
        [Range(0f, 1f)]
        public float healthThreshold = 0.5f;
        public string phaseName = "Phase 1";
        public string phaseDescription = "";
        public Color phaseColor = Color.white;
        public Sprite phaseIcon;

        [Header("Phase Behavior")]
        [Range(0.5f, 5f)]
        public float aggressionMultiplier = 1f;
        [Range(0.5f, 5f)]
        public float damageMultiplier = 1f;
        [Range(0.5f, 3f)]
        public float speedMultiplier = 1f;
        public float healOnEnter = 200f;
        public AbilityConfig[] unlockedAbilities = new AbilityConfig[0];
        public GameObject[] phaseEntryEffects;
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
}
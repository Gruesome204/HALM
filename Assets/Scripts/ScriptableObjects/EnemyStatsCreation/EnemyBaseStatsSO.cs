using UnityEngine;

[CreateAssetMenu(fileName = "New EnemyBaseStats", menuName = "Game/Enemy/New EnemyBaseStats")]
public class EnemyBaseStats : ScriptableObject
{
    [Header("Name")]
    public string baseName;
    public string baseDescription;

    [Header("Type")]
    public EnemyType enemyType = EnemyType.Mob;

    [Header("Base Level")]
    public int baseLevel = 1;

    [Header("Movement")]
    public float baseMovementSpeed = 2f;

    [Header("Defensive Stats")]
    public float baseMaxHealth = 100f;
    public float baseArmor;
    public float baseMagicResistance;
    public float baseKnockbackReduction;

    [Header("Offensive Stats")]
    public float baseDamage;
    public float baseAttackSpeed;
    public float baseCritChance;
    public float baseCritHitMultiplier;
    public float baseAttackRange;

    [Header("Knockback Stats")]
    public float baseKnockbackForce;
    public float baseKnockbackDuration;

    [Header("Detection")]
    public float baseDetectionRange = 2f;
    public float pursueRange = 2f;

    [Header("Progression")]
    public float experienceYield = 2f;
    public int goldYield = 0;

    [Header("Resource Drops")]
    public ResourceDropper.ResourceDrop[] resourceDrops;

    [Header("Scaling Factors")]
    public float baseHealthScaleFactor;
    public float baseDamageScaleFactor;
    public float baseSpeedScaleFactor;
    public float baseArmorScaleFactor;

    [Header("Boss Properties")]
    [Tooltip("Only relevant if this enemy is a boss")]
    public string bossBarName;
    public Color bossBarColor = Color.red;
    public Sprite bossPortrait;
    public float bossBarHeight = 20f; // UI height in pixels
    public bool isMultiStageBoss = false; // For bosses with phases
    public int numberOfPhases = 1;

}

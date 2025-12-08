using UnityEngine;
using static ResourceTypeData;

[CreateAssetMenu(fileName = "New EnemyBaseStats", menuName = "Game/Enemy/New EnemyBaseStats")]
public class EnemyBaseStats : ScriptableObject
{
    [Header("Name")]
    public string baseName;
    public string baseDescription;

    [Header("Base Level")]
    public int baseLevel = 1;

    [Header("Movement")]
    public float baseMovementSpeed = 2f;

    [Header("Defensive Stats")]
    public float baseMaxHealth = 100f;
    public float baseHealth = 100f;
    public float baseArmor;
    public float baseMagicResistance;
    public float baseKnockbackReduction;
    public float baseknockbackForce;
    public float baseknockbackDuration;


    [Header("Offensive Stats")]
    public float baseDamage;
    public float baseAttackSpeed;
    public float baseCritChance;
    public float baseCritHitMultiplier;
    public float baseAttackRange;

    [Header("Detection")]
    public float baseDetectionRange = 2f;
    public float baseVisionRange = 2f;
    public float baseHearingRange = 2f;
    public float pursueRange = 2f;

    [Header("Progression")]
    public float experienceYield = 2f;
    public int goldYield = 0;

    [Header("Resource Drops")]
    public ResourceDrop[] resourceDrops;

    [Header("Scaling Factors")]
    public float baseHealthScaleFactor;
    public float baseDamageScaleFactor;
    public float baseSpeedScaleFactor;
    public float baseArmorScaleFactor;

}

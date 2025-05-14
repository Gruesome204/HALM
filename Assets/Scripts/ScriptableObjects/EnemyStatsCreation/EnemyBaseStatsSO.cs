using UnityEngine;

[CreateAssetMenu(fileName = "New EnemyBaseStats", menuName = "Game/Enemy/New EnemyBaseStats")]
public class EnemyBaseStats : ScriptableObject
{
    [Header("Level")]
    public int baseLevel = 1;

    [Header("Base Stats")]
    public float baseMaxHealth = 300f;
    public float baseHealth = 300f;
    public float baseDamage;
    public float baseArmor;
    public float baseAttackSpeed;

    [Header("Crit")]
    public float baseCritChance;
    public float baseCritHitMultiplier;

    [Header("Resistances")]
    public float baseMagicResistance;

    [Header("Ranges")]
    public float baseAttackRange;

    [Header("Movement")]
    public float baseMovementSpeed = 2f;

    [Header("Detection")]
    public float baseDetectionRange = 2f;
    public float baseVisionRange = 2f;
    public float baseHearingRange = 2f;
    public float pursueRange = 2f;

    [Header("Progression")]
    public float experienceYield = 2f;

}

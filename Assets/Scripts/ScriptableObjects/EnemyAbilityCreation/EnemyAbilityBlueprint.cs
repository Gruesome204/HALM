using UnityEngine;

public enum AbilityTargetType { Self, SingleTarget, AreaOfEffect }
public enum TargetSelection { Self, ClosestEnemy, LowestHPEnemy, RandomEnemy }

[CreateAssetMenu(menuName = "Game/Enemy/Ability")]
public class EnemyAbilityBlueprint : ScriptableObject
{
    [Header("General Settings")]
    public string abilityName;
    public Sprite icon;
    [TextArea] public string description;

    public float cooldown = 1f;
    public float range = 1f;
    public int priority = 0; // higher = preferred by AI

    [Header("Targeting")]
    public AbilityTargetType targetType = AbilityTargetType.SingleTarget;
    public TargetSelection targetSelection = TargetSelection.ClosestEnemy;

    [Header("Effects")]
    public AbilityEffect[] effects;
}

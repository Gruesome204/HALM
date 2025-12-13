using UnityEngine;

public enum AbilityTargetType { Self, SingleTarget, AreaOfEffect }
public enum TargetSelection { Self, ClosestEnemy, LowestHPEnemy, RandomEnemy }

[CreateAssetMenu(menuName = "Game/Enemy/New AbilityBlueprint")]
public class EnemyAbilityBlueprint : ScriptableObject
{
    [Header("General Settings")]
    public string abilityName;
    public Sprite icon;
    [TextArea(2, 6)]
    public string description;   

    [Range(0.1f, 60f)]
    [Tooltip("Cooldown time in seconds before the ability can be used again.")]
    public float cooldown = 1f;
    [Tooltip("Range within which the ability can be used.")]
    [Range(0.5f, 50f)]
    public float range = 1f;
    [Tooltip("Higher priority abilities are preferred by AI when multiple are available.")]
    [Range(0, 10)]
    public int priority = 0; // higher = preferred by AI

    [Header("Targeting")]
    [Tooltip("Defines how this ability selects its target.")]
    public AbilityTargetType targetType = AbilityTargetType.SingleTarget;
    [Tooltip("Defines which target is selected when multiple are in range.")]
    public TargetSelection targetSelection = TargetSelection.ClosestEnemy;

    [Header("Effects")]
    [Tooltip("List of effects applied when this ability is used.")]
    public AbilityEffect[] effects;

    [Tooltip("Minimum health percentage required to consider using this ability (0–1).")]
    [Range(0f, 1f)]
    public float useBelowHealthPercent = 1f;
}

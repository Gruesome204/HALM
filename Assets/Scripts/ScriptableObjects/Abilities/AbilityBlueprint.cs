using UnityEngine;
public enum TargetSelection { Self, ClosestEnemy, LowestHPEnemy, RandomEnemy }

[CreateAssetMenu(menuName = "Game/Abilities/New AbilityBlueprint")]
public class AbilityBlueprint : ScriptableObject
{
    [Header("General Settings")]
    public string abilityName;
    public Sprite icon;
    [TextArea(2, 6)]
    public string description;
    public AbilityCategory category = AbilityCategory.Offensive;
        [Tooltip("Cooldown time in seconds before the ability can be used again.")]
    [Range(0.1f, 60f)]
    public float cooldown = 1f;

    [Header("Targeting")]
    [Tooltip("Defines how this ability selects its target.")]
    public AbilityTargetType targetType = AbilityTargetType.SingleTarget;
    [Tooltip("Defines which target is selected when multiple are in range.")]
    public TargetSelection targetSelection = TargetSelection.ClosestEnemy;
    [Tooltip("Range within which the ability can be used.")]
    [Range(0.5f, 50f)]
    public float range = 1f;

    [Tooltip("Higher priority abilities are preferred by AI when multiple are available.")]
    [Range(0, 10)]
    public int priority = 0; // higher = preferred by AI

    [Header("Area of Effect (Optional)")]
    public AoEShape aoeShape = AoEShape.None;
    [Tooltip("Radius of AoE effect if applicable.")]
    public float aoeRadius = 1f;
    [Tooltip("Angle for cone AoE.")]
    [Range(0, 180)]
    public float aoeAngle = 90f;



    [Header("Effects")]
    public AbilityEffect[] effects;
    public float effectDelay = 0f;
    public float effectDuration = 1f;

    [Header("Activation Conditions")]
    [Tooltip("Minimum health % for the user to consider using this ability.")]
    [Range(0f, 1f)]
    public float minUserHealthPercent = 0f;

    [Tooltip("Maximum health % for the target (if applicable).")]
    [Range(0f, 1f)]
    public float maxTargetHealthPercent = 1f;

    [Header("Resource Cost")]
    public float energyCost = 0f;
    public float manaCost = 0f;

    [Header("Visuals & Audio (Optional)")]
    public GameObject visualEffectPrefab;
    public AudioClip soundEffect;
    public Color highlightColor = Color.white;
    [Tooltip("Duration which the effect is shown.")]
    public float visualDuration = 1f;

}

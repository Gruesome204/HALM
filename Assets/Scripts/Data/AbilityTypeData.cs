using UnityEngine;

public class AbilityTypeData : MonoBehaviour
{
    public AbilityTargetType abilityTargetType;
    public AbilityCategory abilityCategory;
    public AoEShape aoeShapes;
}
public enum AbilityTargetType
{
        Self,
        SingleTarget,
        AreaOfEffect,
        Direction,
        Target
}

public enum AbilityCategory
{
    Offensive,      // Damage abilities
    Defensive,      // Shields, parry, invuln
    Healing,        // Heal self/allies
    Buff,           // Increase stats
    Debuff,         // Decrease enemy stats
    Movement,       // Dash, teleport
    Summon,         // Spawn objects or allies
    Utility         // Non-combat (open doors, detect, etc.)
}

public enum AoEShape { 
    None, 
    Circle, 
    Cone, 
    Line }



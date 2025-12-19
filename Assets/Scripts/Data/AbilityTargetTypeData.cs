using UnityEngine;

public class AbilityTargetTypeData : MonoBehaviour
{
    public AbilityTargetType abilityTargetType;

}
public enum AbilityTargetType
{
        Self,
        SingleTarget,
        AreaOfEffect,
        Direction,
        Target
    }
   

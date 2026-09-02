using UnityEngine;

[CreateAssetMenu(menuName = "Game/Abilities/Effects/Damage Over Time")]
public class DamageOverTimeEffect : AbilityEffect
{
    [Header("DOT Settings")]
    public float damagePerTick = 5f;
    public float tickInterval = 1f;
    public int tickCount = 5;
    public DamageData.DamageType damageType = DamageData.DamageType.Physical;
    public GameObject tickEffectPrefab; // Visual effect on each tick

    public override void Apply(GameObject user, GameObject target)
    {
        if (target == null) return;

        DamageOverTime dot = target.GetComponent<DamageOverTime>();
        if (dot == null)
            dot = target.AddComponent<DamageOverTime>();

        dot.Initialize(user, damagePerTick, tickInterval, tickCount, damageType, tickEffectPrefab);
    }
}
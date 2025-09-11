using UnityEngine;

public abstract class EnemyAbility : ScriptableObject
{
    public string abilityName;
    public float cooldown;
    public float range;
    public AbilityTargetType targetType;
    protected float lastUsedTime;

    public bool CanUse()
    {
        return Time.time >= lastUsedTime + cooldown;
    }

    public void Use(GameObject user, GameObject target)
    {
        if (CanUse())
        {
            lastUsedTime = Time.time;

            switch (targetType)
            {
                case AbilityTargetType.Self:
                    Activate(user, user);
                    break;

                case AbilityTargetType.SingleTarget:
                    if (target != null)
                        Activate(user, target);
                    break;

                case AbilityTargetType.AreaOfEffect:
                    // Pass null or the original target — depends on ability design
                    Activate(user, target);
                    break;
            }
        }
    }

    public enum AbilityTargetType
    {
        Self,
        SingleTarget,
        AreaOfEffect
    }


    protected abstract void Activate(GameObject user, GameObject target);
}

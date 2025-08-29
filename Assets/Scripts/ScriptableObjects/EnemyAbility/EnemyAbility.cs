using UnityEngine;

public abstract class EnemyAbility : ScriptableObject
{
    public string abilityName;
    public float cooldown;
    public float range;

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
            Activate(user, target);
        }
    }

    protected abstract void Activate(GameObject user, GameObject target);
}

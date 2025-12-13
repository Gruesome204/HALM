using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy/AbilityEffects/Damage")]
public class DamageEffect : AbilityEffect
{
    public float amount = 10f;
    public DamageData.DamageType damageType = DamageData.DamageType.Physical;

    public override void Apply(GameObject user, GameObject target)
    {
        if (target == null) return;

        DamageData dmgData = new DamageData(amount, user, damageType);

        var health = target.GetComponent<IDamagable>();
        if (health != null) 
        {
            health.TakeDamage(dmgData, new KnockbackData());
        }

        Debug.Log($"{user.name} dealt {dmgData.amount} {dmgData.type} damage to {target.name}");
    }
}

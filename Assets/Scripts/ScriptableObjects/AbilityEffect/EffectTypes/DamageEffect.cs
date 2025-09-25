using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy/Effects/Damage")]
public class DamageEffect : AbilityEffect
{
    public float amount = 10f;
    public DamageData.DamageType damageType = DamageData.DamageType.Physical;

    public override void Apply(GameObject user, GameObject target)
    {
        //if (target == null) return;

        //DamageData dmg = new DamageData(amount, user, damageType);

        //var health = target.GetComponent<IDamagable>();
        //if (health != null)
        //{
        //    health.TakeDamage(dmg);
        //}

        //Debug.Log($"{user.name} dealt {dmg.amount} {dmg.type} damage to {target.name}");
    }
}

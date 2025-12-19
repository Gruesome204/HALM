using UnityEngine;

[CreateAssetMenu(menuName = "Game/Abilities/Effects/Parry")]
public class ParryAbilityEffect : AbilityEffect
{
    public float parryDuration = 0.25f;  // i-frame
    public float counterDamage = 20f;

    public override void Apply(GameObject user, GameObject target)
    {
        if (!user.TryGetComponent<IInvulnerable>(out var inv)) return;

        inv.SetInvulnerable(parryDuration);

        if (user.TryGetComponent<PlayerHealth>(out var health))
        {

            // Trigger timed parry block
            health.StartParry(parryDuration);
        
        }
    }
}

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
            void OnDamageTaken(DamageData damageData, KnockbackData knock)
            {
                if (damageData.source != null && damageData.source.TryGetComponent<IParryable>(out var enemy))
                {
                    enemy.OnParried(user, counterDamage);
                    health.CallParrySuccess();

                    // Unsubscribe after first parry hit
                    health.OnDamageTakenEvent -= OnDamageTaken;
                }
            }

            health.OnDamageTakenEvent += OnDamageTaken;
        }
    }
}

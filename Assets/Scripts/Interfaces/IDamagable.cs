using UnityEngine;

public interface IDamagable
{
    bool IsInvulnerable { get; set; }
    void OnDamageTaken(float amount); //Visuals and audio Feedback when dmg received
    void TakeDamage(DamageData damageData, KnockbackData knockbackData);
    void Die(DamageData damageData);

}

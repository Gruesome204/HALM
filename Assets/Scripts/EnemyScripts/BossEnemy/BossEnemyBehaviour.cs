using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class BossEnemyBehaviour : EnemyBehaviour
{
    [Header("Boss Specific Settings")]
    [SerializeField] private float enrageHealthThreshold = 0.3f;

    private bool isEnraged = false;

    protected override void HandleDamaged(DamageData damageData, KnockbackData knockbackData)
    {
        base.HandleDamaged(damageData, knockbackData); // call default behaviour

        // Boss-specific logic: Enrage at low health
        if (!isEnraged && stats.currentHealth / stats.maxHealth <= enrageHealthThreshold)  
        {
            Enrage();
        }
    }

    private void Enrage()
    {
        isEnraged = true;
        Debug.Log($"{name} is enraged!");
    }

}

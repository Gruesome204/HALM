using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    public EnemyStats stats;

    public void SpecialAbility()
    {
        // Example: deal AoE damage, summon minions, etc.
        Debug.Log($"{stats.enemyName} uses a special attack!");
    }
}

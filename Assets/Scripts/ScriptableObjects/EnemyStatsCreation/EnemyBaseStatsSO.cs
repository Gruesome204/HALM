using UnityEngine;

[CreateAssetMenu(fileName = "New EnemyStats", menuName = "Game/Enemy/New EnemyStats")]
public class EnemyBaseStatsSO : EnemyBaseStats
{
    [Header("Normal Enemy Specific")]
    public bool canFlee = false;
    public float fleeThreshold = 0.2f;

    private void OnEnable()
    {
        enemyType = EnemyType.Mob;
    }
}
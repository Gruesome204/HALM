using UnityEngine;

[CreateAssetMenu(fileName = "New EnemyStats", menuName = "Game/Enemy/New EnemyStats")]
public class EnemyBaseStatsSO : EnemyBaseStats
{
    // All properties inherited from EnemyBaseStats
    // No additional properties needed for normal enemies

    [Header("Normal Enemy Specific")]
    [Tooltip("Additional properties for normal enemies can go here")]
    public bool canFlee = false;
    public float fleeThreshold = 0.2f; // Health percentage to flee

    private void OnEnable()
    {
        enemyType = EnemyType.Mob;
    }
}
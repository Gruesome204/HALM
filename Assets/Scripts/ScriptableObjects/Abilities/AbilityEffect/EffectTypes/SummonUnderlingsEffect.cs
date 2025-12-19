using UnityEngine;

[CreateAssetMenu(menuName = "Game/Abilities/Effects/Summon Underlings")]
public class SummonUnderlingsEffect : AbilityEffect
{
    [Header("Summon Settings")]
    [Tooltip("Prefab of the underling spider.")]
    public GameObject underlingPrefab;

    [Tooltip("Number of underlings to summon.")]
    public int count = 3;

    [Tooltip("Spawn radius around the summoner.")]
    public float spawnRadius = 2f;

    public override void Apply(GameObject user, GameObject target)
    {
        if (underlingPrefab == null || user == null)
            return;

        for (int i = 0; i < count; i++)
        {
            Vector2 spawnPos = (Vector2)user.transform.position +
                               Random.insideUnitCircle * spawnRadius;

            GameObject underling = Instantiate(
                underlingPrefab,
                spawnPos,
                Quaternion.identity
            );

            // Tell underling who to attack
            if (target != null &&
                underling.TryGetComponent(out EnemyBehaviour targeting))
            {
                targeting.AcquirePlayerTarget();
            }
        }
    }   
}

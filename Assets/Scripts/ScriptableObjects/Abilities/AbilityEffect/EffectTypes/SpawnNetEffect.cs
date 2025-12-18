using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy/Effects/SpawnNet")]
public class SpawnNetEffect : AbilityEffect
{
    [Tooltip("Prefab of the net/trap to spawn.")]
    public GameObject netPrefab;

    [Tooltip("How long the net lasts in seconds.")]
    public float duration = 5f;

    [Tooltip("Radius of the net effect area.")]
    public float radius = 1f;

    [Header("Slow Settings")]
    [Range(0f, 1f)]
    [Tooltip("Multiplier applied to player's movement speed (0 = stop, 1 = normal).")]
    public float slowMultiplier = 0.5f;
    [Tooltip("Duration of the slow effect in seconds.")]
    public float slowDuration = 3f;

    public override void Apply(GameObject user, GameObject target)
    {
        if (netPrefab == null) return;

        // Spawn the net at the target's position (or under the user if self-targeted)
        Vector3 spawnPosition = target != null ? target.transform.position : user.transform.position;
        GameObject net = GameObject.Instantiate(netPrefab, spawnPosition, Quaternion.identity);

        // Scale net to match radius
        net.transform.localScale = new Vector3(radius, 1f, radius);

        // Add NetTrap component if not already present
        NetTrap trap = net.GetComponent<NetTrap>();
        if (trap == null)
        {
            trap = net.AddComponent<NetTrap>();
        }

        trap.slowMultiplier = slowMultiplier;
        trap.slowDuration = slowDuration;

        // Destroy the net after its duration
        GameObject.Destroy(net, duration);
    }
}
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Abilities/Effects/SpawnNet")]
public class SpawnNetEffect : AbilityEffect
{
    [Header("Net Configuration")]
    [Tooltip("Prefab of the net/trap to spawn.")]
    [SerializeField] private GameObject netPrefab;

    [Tooltip("How long the net lasts in seconds before despawning.")]
    [SerializeField][Min(0.1f)] private float duration = 5f;

    [Tooltip("Radius of the net effect area.")]
    [SerializeField][Min(0.1f)] private float radius = 1f;

    [Header("Slow Settings")]
    [Range(0f, 1f)]
    [Tooltip("Multiplier applied to movement speed (0 = stop, 1 = normal).")]
    [SerializeField] private float slowMultiplier = 0.5f;

    [Tooltip("Duration of the slow effect in seconds.")]
    [SerializeField][Min(0.1f)] private float slowDuration = 3f;

    [Header("Spawn Behavior")]
    [Tooltip("Y offset for spawning the net (useful for ground placement).")]
    [SerializeField] private float spawnYOffset = 0.1f;

    [Tooltip("Should the net face the user?")]
    [SerializeField] private bool faceUser = false;

    [Tooltip("Layer mask for ground detection when placing nets.")]
    [SerializeField] private LayerMask groundLayerMask = ~0;

    public override void Apply(GameObject user, GameObject target)
    {
        if (!ValidateInputs(user, target)) return;

        Vector3 spawnPosition = GetSpawnPosition(user, target);
        GameObject net = InstantiateNet(spawnPosition, user);

        if (net == null) return;

        ConfigureNetTrap(net);
        HandleVisualFeedback(net, user, target);
        ScheduleNetDestruction(net);
    }

    private bool ValidateInputs(GameObject user, GameObject target)
    {
        if (user == null)
        {
            Debug.LogError("SpawnNetEffect: User is null!");
            return false;
        }

        if (netPrefab == null)
        {
            Debug.LogError($"SpawnNetEffect: NetPrefab is not assigned on {name}!");
            return false;
        }

        return true;
    }

    private Vector3 GetSpawnPosition(GameObject user, GameObject target)
    {
        // Determine base position
        Vector3 basePosition = target != null
            ? target.transform.position
            : user.transform.position;

        // Try to snap to ground if possible
        if (Physics.Raycast(basePosition + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f, groundLayerMask))
        {
            basePosition.y = hit.point.y + spawnYOffset;
        }
        else
        {
            basePosition.y += spawnYOffset;
        }

        return basePosition;
    }

    private GameObject InstantiateNet(Vector3 position, GameObject user)
    {
        Quaternion rotation = faceUser && user != null
            ? Quaternion.LookRotation(user.transform.forward)
            : Quaternion.identity;

        GameObject net = Instantiate(netPrefab, position, rotation);

        // Scale net to match radius
        net.transform.localScale = new Vector3(radius, 1f, radius);

        // Set parent to null to avoid being destroyed with user
        net.transform.SetParent(null);

        return net;
    }

    private void ConfigureNetTrap(GameObject net)
    {
        // Try to get existing NetTrap or add one
        if (!net.TryGetComponent<NetTrap>(out NetTrap trap))
        {
            trap = net.AddComponent<NetTrap>();
        }

        trap.Initialize(slowMultiplier, slowDuration, radius);
    }

    private void HandleVisualFeedback(GameObject net, GameObject user, GameObject target)
    {
        // Optionally trigger VFX/SFX
        if (net.TryGetComponent<ParticleSystem>(out ParticleSystem ps))
        {
            ps.Play();
        }
    }

    private void ScheduleNetDestruction(GameObject net)
    {
        // Destroy after duration, but also add a safety check
        Destroy(net, duration);
    }

    // Editor validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        // Ensure radius has a minimum value
        if (radius < 0.1f) radius = 0.1f;
        if (duration < 0.1f) duration = 0.1f;
        if (slowDuration < 0.1f) slowDuration = 0.1f;

        // Clamp slow multiplier
        slowMultiplier = Mathf.Clamp01(slowMultiplier);
    }
#endif
}

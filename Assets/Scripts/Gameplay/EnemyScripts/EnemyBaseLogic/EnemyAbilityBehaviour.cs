using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class EnemyAbilityBehaviour : MonoBehaviour, IPausable
{
    public AbilityBlueprint[] abilities;
    public GameObject target;
    private float maxAbilityRange;
    private bool isPaused;
    private float abilityTimer = 0f;
    [SerializeField] private float abilityCheckInterval = 1f;

    private float aggressionMultiplier = 1f;
    private List<GameObject> nearbyEnemies = new List<GameObject>();

    // Layer mask for enemies (players)
    private const int PLAYER_LAYER = 1 << 9; // Assuming Layer 9 is "Player"

    private void Start()
    {
        if (abilities == null || abilities.Length == 0)
        {
            Debug.LogWarning($"{name} has no abilities assigned — skipping registration.");
            return;
        }
        AbilityManager.Instance.Register(gameObject, abilities);
        maxAbilityRange = abilities.Max(a => a.range);
    }

    private void OnDestroy()
    {
        if (AbilityManager.Instance != null)
            AbilityManager.Instance.Unregister(gameObject);
    }

    private void Update()
    {
        if (abilities == null || abilities.Length == 0 || AbilityManager.Instance == null || isPaused)
            return;

        abilityTimer += Time.deltaTime * aggressionMultiplier;
        if (abilityTimer < abilityCheckInterval)
            return;

        // Scan for nearby enemies (players)
        ScanForEnemies();

        if (target == null)
        {
            // Try to find a new target
            target = FindBestTarget();
            if (target == null)
                return;
        }

        float distance = Vector2.Distance(transform.position, target.transform.position);

        // Check if target is out of range for non-AoE abilities
        bool hasNonAOEAbility = abilities.Any(a => a.targetType != AbilityTargetType.AreaOfEffect &&
                                                   a.targetType != AbilityTargetType.Cone &&
                                                   a.targetType != AbilityTargetType.GroundPlacement &&
                                                   a.targetType != AbilityTargetType.Direction);

        if (distance > maxAbilityRange && hasNonAOEAbility)
        {
            target = FindBestTarget();
            if (target == null)
                return;
        }

        var runtimeList = AbilityManager.Instance.GetAbilities(gameObject);
        if (runtimeList == null)
            return;

        AbilityRuntime selectedAbility = null;
        int selectedIndex = -1;
        int maxPriority = int.MinValue;

        for (int i = 0; i < runtimeList.Count; i++)
        {
            var ab = runtimeList[i];
            if (ab != null && ab.CanUse(gameObject, target))
            {
                // For AoE abilities, check if there are enough targets
                if (ab.ability.aoeShape != AoEShape.None)
                {
                    var targets = ab.GetTargetsInAoE(gameObject, target);
                    if (targets.Count < 1)
                        continue;
                }

                if (ab.ability.priority > maxPriority)
                {
                    selectedAbility = ab;
                    selectedIndex = i;
                    maxPriority = ab.ability.priority;
                }
            }
        }

        if (selectedAbility == null)
            return;

        bool used = AbilityManager.Instance.TryUseAbility(gameObject, selectedIndex, target);
        if (used)
        {
            abilityTimer = 0f;
            Debug.Log($"{name} used ability: {selectedAbility.ability.abilityName}");

            // If AoE, log how many targets were hit
            if (selectedAbility.ability.aoeShape != AoEShape.None)
            {
                var targetsHit = selectedAbility.GetTargetsInAoE(gameObject, target);
                Debug.Log($"{name} hit {targetsHit.Count} targets with AoE");
            }
        }
    }

    private void ScanForEnemies()
    {
        nearbyEnemies.Clear();

        // Only detect objects on the Player layer
        Collider[] colliders = Physics.OverlapSphere(transform.position, maxAbilityRange, PLAYER_LAYER);

        foreach (var collider in colliders)
        {
            if (collider.gameObject != gameObject)
            {
                if (collider.GetComponent<IDamagable>() != null)
                {
                    nearbyEnemies.Add(collider.gameObject);
                }
            }
        }
    }

    private GameObject FindBestTarget()
    {
        if (nearbyEnemies.Count == 0)
            return null;

        // Try to find target based on all abilities' target selection preferences
        var bestTarget = nearbyEnemies.OrderByDescending(t =>
        {
            float score = 0f;
            foreach (var ability in abilities)
            {
                if (ability.targetType == AbilityTargetType.Self || ability.targetSelection == TargetSelection.Self)
                    continue;

                // Check if ability can be used
                var runtime = AbilityManager.Instance.GetAbilityRuntime(gameObject, ability);
                if (runtime != null && runtime.CanUse(gameObject, t))
                {
                    score += ability.priority;
                }
            }
            return score;
        }).FirstOrDefault();

        return bestTarget;
    }

    public void SetTarget(GameObject newTarget)
    {
        target = newTarget;
    }

    public void SetAggressionMultiplier(float value)
    {
        aggressionMultiplier = value;
    }

    public void OnPause()
    {
        isPaused = true;
    }

    public void OnResume()
    {
        isPaused = false;
    }
}
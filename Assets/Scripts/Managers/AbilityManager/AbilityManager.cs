using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityManager : MonoBehaviour, IGameSystem
{
    public static AbilityManager Instance;

    private Dictionary<GameObject, List<AbilityRuntime>> runtimeAbilities = new();
    public int InitializePriority => 3;

    public void Initialize()
    {
    }

    public void PostInitialize()
    {
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        foreach (var userAbilities in runtimeAbilities.Values)
        {
            foreach (var ability in userAbilities)
            {
                ability.Tick(Time.deltaTime);
            }
        }
    }

    public void Register(GameObject user, AbilityBlueprint[] abilities)
    {
        if (!runtimeAbilities.ContainsKey(user))
            runtimeAbilities[user] = new List<AbilityRuntime>();
        else
            runtimeAbilities[user].Clear();

        foreach (var ability in abilities)
        {
            // Skip null abilities
            if (ability == null)
            {
                Debug.LogWarning($"Null ability blueprint found when registering for {user.name}. Skipping.");
                continue;
            }
            runtimeAbilities[user].Add(new AbilityRuntime(ability));
        }
    }

    public void Unregister(GameObject user)
    {
        if (runtimeAbilities.ContainsKey(user))
            runtimeAbilities.Remove(user);
    }

    /// <summary>
    /// Get a specific ability runtime by blueprint
    /// </summary>
    public AbilityRuntime GetAbilityRuntime(GameObject user, AbilityBlueprint abilityBlueprint)
    {
        if (!runtimeAbilities.TryGetValue(user, out List<AbilityRuntime> runtimeList))
            return null;

        return runtimeList.FirstOrDefault(r => r.ability == abilityBlueprint);
    }

    /// <summary>
    /// Try to use an ability by index
    /// </summary>
    public bool TryUseAbility(GameObject user, int abilityIndex, GameObject target)
    {
        if (!runtimeAbilities.TryGetValue(user, out List<AbilityRuntime> runtimeList))
            return false;

        if (abilityIndex < 0 || abilityIndex >= runtimeList.Count)
            return false;

        var abilityRuntime = runtimeList[abilityIndex];
        if (abilityRuntime.CanUse(user, target))
        {
            abilityRuntime.Use(user, target);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Get all abilities for a user
    /// </summary>
    public List<AbilityRuntime> GetAbilities(GameObject user)
    {
        return runtimeAbilities.ContainsKey(user) ? runtimeAbilities[user] : null;
    }

    /// <summary>
    /// AI helper: returns first available ability based on priority
    /// </summary>
    public AbilityRuntime GetHighestPriorityAbility(GameObject user, GameObject target)
    {
        if (!runtimeAbilities.ContainsKey(user)) return null;

        AbilityRuntime best = null;
        int bestPriority = int.MinValue;

        foreach (var ability in runtimeAbilities[user])
        {
            if (ability.CanUse(user, target))
            {
                if (best == null || ability.ability.priority > bestPriority)
                {
                    best = ability;
                    bestPriority = ability.ability.priority;
                }
            }
        }
        return best;
    }

    /// <summary>
    /// Get all abilities of a specific category for a user
    /// </summary>
    public List<AbilityRuntime> GetAbilitiesByCategory(GameObject user, AbilityCategory category)
    {
        if (!runtimeAbilities.ContainsKey(user))
            return new List<AbilityRuntime>();

        return runtimeAbilities[user].Where(a => a.ability.category == category).ToList();
    }

    /// <summary>
    /// Check if a user has any ability that can be used
    /// </summary>
    public bool HasAvailableAbility(GameObject user, GameObject target)
    {
        if (!runtimeAbilities.ContainsKey(user))
            return false;

        foreach (var ability in runtimeAbilities[user])
        {
            if (ability.CanUse(user, target))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Get the total count of abilities for a user
    /// </summary>
    public int GetAbilityCount(GameObject user)
    {
        return runtimeAbilities.ContainsKey(user) ? runtimeAbilities[user].Count : 0;
    }
}
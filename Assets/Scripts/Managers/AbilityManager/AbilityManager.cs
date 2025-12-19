using UnityEngine;
using System.Collections.Generic;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;

    private Dictionary<GameObject, List<AbilityRuntime>> runtimeAbilities = new();

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
            runtimeAbilities[user].Add(new AbilityRuntime(ability));
    }

    public void Unregister(GameObject user)
    {
        if (runtimeAbilities.ContainsKey(user))
            runtimeAbilities.Remove(user);
    }

    public bool TryUseAbility(GameObject user, int abilityIndex, GameObject target)
    {
        if (!runtimeAbilities.ContainsKey(user)) return false;
        if (abilityIndex < 0 || abilityIndex >= runtimeAbilities[user].Count) return false;

        var runtime = runtimeAbilities[user][abilityIndex];
        if (runtime.CanUse(user, target))
        {
            runtime.Use(user, target);
            return true;
        }

        return false;
    }

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
        foreach (var ability in runtimeAbilities[user])
        {
            if (ability.CanUse(user, target))
            {
                if (best == null || ability.ability.priority > best.ability.priority)
                    best = ability;
            }
        }
        return best;
    }
}

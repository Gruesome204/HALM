using UnityEngine;
using System.Collections.Generic;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;

    // user → list of ability runtimes
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

    public void Register(GameObject user, EnemyAbilityBlueprint[] abilities)
    {
        if (!runtimeAbilities.ContainsKey(user))
        {
            runtimeAbilities[user] = new List<AbilityRuntime>();
            foreach (var ability in abilities)
            {
                runtimeAbilities[user].Add(new AbilityRuntime(ability));
            }
        }
    }

    public void Unregister(GameObject user)
    {
        if (runtimeAbilities.ContainsKey(user))
        {
            runtimeAbilities.Remove(user);
        }
    }

    public bool TryUseAbility(GameObject user, int abilityIndex, GameObject target)
    {
        if (!runtimeAbilities.ContainsKey(user)) return false;
        if (abilityIndex < 0 || abilityIndex >= runtimeAbilities[user].Count) return false;

        var ability = runtimeAbilities[user][abilityIndex];
        if (ability.CanUse(user, target))
        {
            ability.Use(user, target);
            return true;
        }

        return false;
    }

    public List<AbilityRuntime> GetAbilities(GameObject user)
    {
        return runtimeAbilities.ContainsKey(user) ? runtimeAbilities[user] : null;
    }
}

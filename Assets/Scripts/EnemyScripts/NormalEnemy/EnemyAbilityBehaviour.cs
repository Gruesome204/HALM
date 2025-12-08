using UnityEngine;
using System.Linq;

public class EnemyAbilityBehaviour : MonoBehaviour, IPausable
{
    public EnemyAbilityBlueprint[] abilities;
    public GameObject target;

    private bool isPaused;
    private float abilityTimer = 0f; // deltaTime-based timer

    private void Start()
    {
        if (abilities == null || abilities.Length == 0)
        {
            Debug.LogWarning($"{name} has no abilities assigned — skipping registration.");
            return;
        }
        AbilityManager.Instance.Register(gameObject, abilities);
    }

    private void OnDestroy()
    {
        if (AbilityManager.Instance != null)
            AbilityManager.Instance.Unregister(gameObject);
    }

    private void Update()
    {
        if (abilities == null || abilities.Length == 0)
            return;
        if (AbilityManager.Instance == null)
            return;

        // Increment pause-aware timer
        abilityTimer += Time.deltaTime;
        float abilityCheckInterval = 1f; // default interval between attempts
        if (abilityTimer < abilityCheckInterval)
            return;

        if (target == null)
            return; 

        float distance = Vector2.Distance(transform.position, target.transform.position);
        float maxAbilityRange = abilities.Max(a => a.range); 

        if (distance > maxAbilityRange)
        {
            target = null;
            return;
        }
        
        var runtimeList = AbilityManager.Instance.GetAbilities(gameObject);
        if (runtimeList == null) return;
        if (target == null)
            return;


        // Filter abilities that can currently be used
        var usable = runtimeList
            .Select((a, i) => new { ability = a, index = i })
            .Where(x => x.ability != null && x.ability.CanUse(gameObject, target.gameObject))
            .OrderByDescending(x => x.ability.ability.priority)
            .FirstOrDefault();

        if (usable == null)
            return;

        // Try using it via the manager (respects cooldowns, conditions)
        bool used = AbilityManager.Instance.TryUseAbility(gameObject, usable.index, target.gameObject);

        if (used)
        {
            abilityTimer = 0f;
            Debug.Log($"{name} used ability: {usable.ability.ability.abilityName}");
        }
        else
            Debug.Log($"{name} tried to use ability but failed (cooldown or invalid target).");
    }

    public void OnPause()
    {
        throw new System.NotImplementedException();
    }

    public void OnResume()
    {
        throw new System.NotImplementedException();
    }
}

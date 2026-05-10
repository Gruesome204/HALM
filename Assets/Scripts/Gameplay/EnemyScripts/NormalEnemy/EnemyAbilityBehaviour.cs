using UnityEngine;
using System.Linq;

public class EnemyAbilityBehaviour : MonoBehaviour, IPausable
{
    public AbilityBlueprint[] abilities;
    public GameObject target;
    private float maxAbilityRange;
    private bool isPaused;
    private float abilityTimer = 0f; // deltaTime-based timer
    [SerializeField] private float abilityCheckInterval = 1f;

    private float aggressionMultiplier = 1f;

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

        if (target == null)
            return;

        float distance = Vector2.Distance(transform.position, target.transform.position);
        if (distance > maxAbilityRange)
        {
            target = null;
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
        }
        else
        {
            Debug.Log($"{name} tried to use ability but failed (cooldown or invalid target).");
        }
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

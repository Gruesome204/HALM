using UnityEngine;

public class PlayerAbilityBehaviour : MonoBehaviour, IPausable
{
    [Header("Ability Slots")]
    public AbilityBlueprint[] abilitySlots;

    public GameObject target;
    private bool isPaused;

    private void Start()
    {
        if (abilitySlots == null || abilitySlots.Length == 0)
        {
            Debug.LogWarning($"{name} has no ability slots assigned.");
            return;
        }

        AbilityManager.Instance.Register(gameObject, abilitySlots);
    }

    private void OnDestroy()
    {
        if (AbilityManager.Instance != null)
            AbilityManager.Instance.Unregister(gameObject);
    }

    private void Update()
    {
        if (isPaused || AbilityManager.Instance == null)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
            TryUseSlot(0);

        if (Input.GetKeyDown(KeyCode.Q))
            TryUseSlot(2);

        if (Input.GetKeyDown(KeyCode.E))
            TryUseSlot(3);
    }

    private void TryUseSlot(int slotIndex)
    {
        var runtimes = AbilityManager.Instance.GetAbilities(gameObject);
        if (runtimes == null) return;

        if (slotIndex < 0 || slotIndex >= runtimes.Count)
            return;

        AbilityRuntime ability = runtimes[slotIndex];
        if (ability == null)
            return;

        GameObject resolvedTarget = ResolveTarget(ability);
        if (resolvedTarget == null)
            return;

        AbilityManager.Instance.TryUseAbility(gameObject, slotIndex, resolvedTarget);
    }

    private GameObject ResolveTarget(AbilityRuntime ability)
    {
        switch (ability.ability.targetType)
        {
            case AbilityTargetType.Self:
            case AbilityTargetType.Direction:
                return gameObject;

            case AbilityTargetType.Target:
                return target;

            default:
                return null;
        }
    }

    public void OnPause() => isPaused = true;
    public void OnResume() => isPaused = false;
}

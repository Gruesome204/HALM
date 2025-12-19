using UnityEngine;

public class PlayerAbilityBehaviour : MonoBehaviour, IPausable
{
    [Header("Dash Ability")]
    public AbilityBlueprint dashAbility;
    private AbilityRuntime dashRuntime;
    private bool isPaused;

    private void Start()
    {
        if (dashAbility == null)
        {
            Debug.LogWarning($"{name} has no Dash ability assigned.");
            return;
        }

        // Register only the dash ability with the AbilityManager
        AbilityManager.Instance.Register(gameObject, new AbilityBlueprint[] { dashAbility });

        // Get the runtime reference for quick use
        var runtimes = AbilityManager.Instance.GetAbilities(gameObject);
        if (runtimes != null && runtimes.Count > 0)
            dashRuntime = runtimes[0];
    }

    private void OnDestroy()
    {
        if (AbilityManager.Instance != null)
            AbilityManager.Instance.Unregister(gameObject);
    }

    private void Update()
    {
        if (isPaused || dashRuntime == null)
            return;

        // Use Dash on Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject resolvedTarget = gameObject;
            if (resolvedTarget != null)
                AbilityManager.Instance.TryUseAbility(gameObject, 0, resolvedTarget);
        }
    }

    public void OnPause() => isPaused = true;
    public void OnResume() => isPaused = false;
}

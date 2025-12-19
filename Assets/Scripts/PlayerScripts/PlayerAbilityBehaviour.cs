using UnityEngine;

public class PlayerAbilityBehaviour : MonoBehaviour, IPausable
{
    [Header("Dash Ability")]
    public AbilityBlueprint dashAbility;
    private AbilityRuntime dashRuntime;

    [Header("Parry Ability")]
    public AbilityBlueprint parryAbility;
    private AbilityRuntime parryRuntime;

    private bool isPaused;

    private PlayerHealth playerHealth;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        spriteRenderer = playerHealth.sr;
    }


    private void Start()
    {

        // Ensure at least one ability is assigned
        if (dashAbility == null && parryAbility == null)
        {
            Debug.LogWarning($"{name} has no abilities assigned.");
            return;
        }

        // Register assigned abilities with AbilityManager
        AbilityBlueprint[] abilitiesToRegister = new AbilityBlueprint[] { dashAbility, parryAbility };
        AbilityManager.Instance.Register(gameObject, abilitiesToRegister);


        // Get runtime references for quick access
        var runtimes = AbilityManager.Instance.GetAbilities(gameObject);
        if (runtimes != null)
        {
            foreach (var runtime in runtimes)
            {
                if (runtime.ability == dashAbility)
                    dashRuntime = runtime;
                else if (runtime.ability == parryAbility)
                    parryRuntime = runtime;
            }
        }
    }

    private void OnDestroy()
    {
        if (AbilityManager.Instance != null)
            AbilityManager.Instance.Unregister(gameObject);
    }

    private void Update()
    {
        if (isPaused)
            return;

        // Dash on Space
        if (Input.GetKeyDown(KeyCode.Space) && dashRuntime != null)
            TryUseAbility(dashRuntime);

        // Parry on E
        if (Input.GetKeyDown(KeyCode.E) && parryRuntime != null)
            TryUseAbility(parryRuntime);


        HandleSpriteBlink();
    }

    private void HandleSpriteBlink()
    {
        if (playerHealth.IsInvulnerable && spriteRenderer != null)
            spriteRenderer.enabled = Mathf.FloorToInt(Time.time * 15) % 2 == 0;
        else if (spriteRenderer != null)
            spriteRenderer.enabled = true;
    }
    private void TryUseAbility(AbilityRuntime runtime)
    {
        if (runtime == null || AbilityManager.Instance == null)
            return;

        GameObject resolvedTarget = gameObject;
        if (resolvedTarget == null)
            return;

        var runtimes = AbilityManager.Instance.GetAbilities(gameObject);
        if (runtimes == null)
            return;
        int index = runtimes.IndexOf(runtime);
        if (index >= 0)
            AbilityManager.Instance.TryUseAbility(gameObject, index, resolvedTarget);
    }

    public void OnPause() => isPaused = true;
    public void OnResume() => isPaused = false;
}

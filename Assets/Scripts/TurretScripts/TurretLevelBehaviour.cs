using UnityEngine;

public class TurretLevelBehaviour : MonoBehaviour
{
    [Header("References")]
    public TurretBlueprint blueprint;
    private TurretBehaviour turretBehaviour;
    private void OnEnable()
    {
        if (blueprint != null && TurretLevelManager.Instance != null)
            SyncWithCurrentLevel();


        if (TurretGlobalModifierManager.Instance != null)
            TurretGlobalModifierManager.Instance.OnModifiersChanged += HandleGlobalModifiersChanged;
    }

    private void OnDisable()
    {
        if (TurretLevelManager.Instance != null)
            TurretLevelManager.Instance.OnLevelUp -= HandleLevelUp;

        if (TurretGlobalModifierManager.Instance != null)
            TurretGlobalModifierManager.Instance.OnModifiersChanged -= HandleGlobalModifiersChanged;
    }

    private void HandleGlobalModifiersChanged()
    {
        SyncWithCurrentLevel();
    }

    private void Awake()
    {
        turretBehaviour = GetComponent<TurretBehaviour>();
        if (turretBehaviour == null)
        {
            Debug.LogError($"{name} has no TurretBehaviour attached!");
            return;
        }
    }

    private void Start()
    {

        // Sync upgrades with current level
        SyncWithCurrentLevel();

    }
        
    private void OnDestroy()
    {
        if (TurretLevelManager.Instance != null)
            TurretLevelManager.Instance.OnLevelUp -= HandleLevelUp;
    }

    public void SyncWithCurrentLevel()
    {
        int currentLevel = TurretLevelManager.Instance.GetLevel(blueprint.turretType);
        ApplyUpgrades(currentLevel);
    }

    private void HandleLevelUp(TurretType type, int newLevel)
    {
        if (type == blueprint.turretType)
            ApplyUpgrades(newLevel);
    }

    public void ApplyUpgrades(int level)
    {

        // --- BASE STATS FROM BLUEPRINT WITH LEVEL SCALING ---
        float scaledDamage = blueprint.baseAttackDamage * Mathf.Pow(blueprint.baseDamageGrowthFactor, level - 1);
        float scaledFireRate = blueprint.baseFireRate * Mathf.Pow(blueprint.baseFireRateGrowthFactor, level - 1);
        float scaledRange = blueprint.baseAttackRange + blueprint.baseRangeGrowthFlat * (level - 1);

        // --- TURRET-SPECIFIC UPGRADE MODIFIERS ---
        float turretDamageMult = TurretUpgradeChoiceManager.Instance.GetDamageMultiplier(blueprint.turretType);
        float turretFireRateMult = TurretUpgradeChoiceManager.Instance.GetFireRateMultiplier(blueprint.turretType);
        float turretRangeBonus = TurretUpgradeChoiceManager.Instance.GetRangeBonus(blueprint.turretType);
        int turretProjectiles = TurretUpgradeChoiceManager.Instance.GetProjectilesPerSalve(blueprint.turretType);
        float turretProjectileSpeed = TurretUpgradeChoiceManager.Instance.GetProjectileSpeedMultiplier(blueprint.turretType);

        // --- GLOBAL MODIFIERS ---
        var global = TurretGlobalModifierManager.Instance;
        float globalDamageMult = global?.globalDamageMultiplier ?? 1f;
        float globalFireRateMult = global?.globalFireRateMultiplier ?? 1f;
        float globalRangeBonus = 0f; // Add if needed
        int globalProjectiles = global?.globalProjectilesPerSalve ?? 0;
        float globalProjectileSpeed = global?.globalProjectileSpeed ?? 1f;

        // --- FINAL STATS ---
        turretBehaviour.currentAttackDamage = scaledDamage * turretDamageMult * globalDamageMult;
        turretBehaviour.currentFireRate = scaledFireRate * turretFireRateMult * globalFireRateMult;
        turretBehaviour.currentAttackRange = scaledRange + turretRangeBonus + globalRangeBonus;
        turretBehaviour.projectilesPerSalve = turretProjectiles + globalProjectiles;
        turretBehaviour.currentProjectileSpeed = Mathf.Max(0.01f, blueprint.baseProjectileSpeed
                                                             * turretProjectileSpeed
                                                             * globalProjectileSpeed);

        // --- Change projectile prefab if upgrade provides one ---
        ProjectileTypeSO newProjectile = TurretUpgradeChoiceManager.Instance.GetCombinedModifier(blueprint.turretType).projectileType;
        if (newProjectile != null)
        {
            turretBehaviour.SetProjectile(newProjectile);
        }

        Debug.Log($"{blueprint.turretType} turret upgraded! Level {level} | " +
                  $"Damage={turretBehaviour.currentAttackDamage}, " +
                  $"FireRate={turretBehaviour.currentFireRate}, " +
                  $"Range={turretBehaviour.currentAttackRange}, " +
                  $"Projectiles={turretBehaviour.projectilesPerSalve}, " +
                  $"ProjSpeed={turretBehaviour.currentProjectileSpeed}");
    }


#if UNITY_EDITOR
    [ContextMenu("Give XP Test")]
    private void DebugGiveXP()
    {
        TurretLevelManager.Instance.AddXP(blueprint.turretType, 100f);
    }
#endif
}

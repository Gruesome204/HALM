using UnityEngine;

public class TurretLevelBehaviour : MonoBehaviour
{
    [Header("References")]
    public TurretBlueprint blueprint;
    private TurretBehaviour turretBehaviour;

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
        if (TurretLevelManager.Instance != null)
        {
            TurretLevelManager.Instance.OnLevelUp += HandleLevelUp;
            SyncWithCurrentLevel();
        }

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
        float scaledDamage =
            blueprint.baseAttackDamage *
            Mathf.Pow(blueprint.baseDamageGrowthFactor, level - 1);

        float scaledFireRate =
            blueprint.baseFireRate *
            Mathf.Pow(blueprint.baseFireRateGrowthFactor, level - 1);

        float scaledRange =
            blueprint.baseAttackRange +
            blueprint.baseRangeGrowthFlat * (level - 1);

        // --- TURRET-SPECIFIC UPGRADE MODIFIERS ---
        float turretDamageMult =
            TurretUpgradeChoiceManager.Instance.GetDamageMultiplier(blueprint.turretType);

        float turretFireRateMult =
            TurretUpgradeChoiceManager.Instance.GetFireRateMultiplier(blueprint.turretType);

        float turretRangeBonus =
            TurretUpgradeChoiceManager.Instance.GetRangeBonus(blueprint.turretType);

        int turretProjectiles =
            TurretUpgradeChoiceManager.Instance.GetProjectilesPerSalve(blueprint.turretType);

        float turretProjectileSpeed =
            TurretUpgradeChoiceManager.Instance.GetProjectileSpeedMultiplier(blueprint.turretType);

        // --- BASE STATS (NO GLOBALS HERE) ---
        TurretModifyStats baseStats = new TurretModifyStats
        {
            damage = scaledDamage * turretDamageMult,
            fireRate = scaledFireRate * turretFireRateMult,
            range = scaledRange + turretRangeBonus,
            projectileSpeed = blueprint.baseProjectileSpeed * turretProjectileSpeed,
            projectilesPerSalve = turretProjectiles
        };

        turretBehaviour.ApplyStats(baseStats);

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

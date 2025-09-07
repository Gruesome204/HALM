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
        if (turretBehaviour == null) return; 
        if (blueprint == null)
        {
            Debug.LogError($"{name} is missing its TurretBlueprint!");
            return;
        }
        // Subscribe to global events
        TurretLevelManager.Instance.OnLevelUp += HandleLevelUp;

        // Sync upgrades with current level
        SyncWithCurrentLevel();

    }

    private void OnDestroy()
    {
        if (TurretLevelManager.Instance != null)
            TurretLevelManager.Instance.OnLevelUp -= HandleLevelUp;
    }

    private void SyncWithCurrentLevel()
    {
        int currentLevel = TurretLevelManager.Instance.GetLevel(blueprint.turretType);
        ApplyUpgrades(currentLevel);
    }

    private void HandleLevelUp(TurretType type, int newLevel)
    {
        if (type == blueprint.turretType)
            ApplyUpgrades(newLevel);
    }

    private void ApplyUpgrades(int level)
    {
        if (blueprint == null)
        {
            Debug.LogError($"{name} has no Blueprint assigned!");
            return;
        }
        if (turretBehaviour == null)
        {
            Debug.LogError($"{name} has no TurretBehaviour!");
            return;
        }
        if (TurretUpgradeChoiceManager.Instance == null)
        {
            Debug.LogError("No TurretUpgradeManager found in scene!");
            return;
        }
        // --- base scaling using blueprint growth values ---
        float scaledDamage = blueprint.attackDamage * Mathf.Pow(blueprint.damageGrowthFactor, level - 1);
        float scaledFireRate = blueprint.fireRate * Mathf.Pow(blueprint.fireRateGrowthFactor, level - 1);
        float scaledRange = blueprint.attackRange + blueprint.rangeGrowthFlat * (level - 1);

        // --- apply meta upgrades from UpgradeManager ---
        float dmgMult = TurretUpgradeChoiceManager.Instance.GetDamageMultiplier(blueprint.turretType);
        float rateMult = TurretUpgradeChoiceManager.Instance.GetFireRateMultiplier(blueprint.turretType);
        float rangeBonus = TurretUpgradeChoiceManager.Instance.GetRangeBonus(blueprint.turretType);

        turretBehaviour.currentAttackDamage = scaledDamage * dmgMult;
        turretBehaviour.currentFireRate = scaledFireRate * rateMult;
        turretBehaviour.currentAttackRange = scaledRange + rangeBonus;

        Debug.Log($"{blueprint.turretType} turret upgraded! Level {level} | " +
                  $"Damage={turretBehaviour.currentAttackDamage}, " +
                  $"FireRate={turretBehaviour.currentFireRate}, " +
                  $"Range={turretBehaviour.currentAttackRange}");
    }


#if UNITY_EDITOR
    [ContextMenu("Give XP Test")]
    private void DebugGiveXP()
    {
        TurretLevelManager.Instance.AddXP(blueprint.turretType, 100f);
    }
#endif
}

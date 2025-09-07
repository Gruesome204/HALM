using UnityEngine;

public class TurretLevelBehaviour : MonoBehaviour
{
    [Header("References")]
    public TurretBlueprint blueprint;
    private TurretBehaviour turretBehaviour;

    private void Start()
    {
        turretBehaviour = GetComponent<TurretBehaviour>();
        if (turretBehaviour == null)
        {
            Debug.LogError($"{name} has no TurretShooter attached!");
            return;
        }


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
        // --- base scaling using blueprint growth values ---
        float scaledDamage = blueprint.attackDamage * Mathf.Pow(blueprint.damageGrowthFactor, level - 1);
        float scaledFireRate = blueprint.fireRate * Mathf.Pow(blueprint.fireRateGrowthFactor, level - 1);
        float scaledRange = blueprint.attackRange + blueprint.rangeGrowthFlat * (level - 1);

        // --- apply meta upgrades from UpgradeManager ---
        float dmgMult = UpgradeManager.Instance.GetDamageMultiplier(blueprint.turretType);
        float rateMult = UpgradeManager.Instance.GetFireRateMultiplier(blueprint.turretType);
        float rangeBonus = UpgradeManager.Instance.GetRangeBonus(blueprint.turretType);

        turretBehaviour.currentAttackDamage = scaledDamage * dmgMult;
        turretBehaviour.currentFireRate = scaledFireRate * rateMult;
        turretBehaviour.currentAttackRange = scaledRange + rangeBonus;

        Debug.Log($"{blueprint.turretType} turret upgraded! Level {level} | " +
                  $"Damage={turretBehaviour.currentAttackDamage}, " +
                  $"FireRate={turretBehaviour.currentFireRate}, " +
                  $"Range={turretBehaviour.currentAttackRange}");
    }

    //public void AwardXP(float amount)
    //{
    //    if (blueprint == null || TurretLevelManager.Instance == null) return;
    //    TurretLevelManager.Instance.AddXP(blueprint.turretType, amount);    
    //}
}

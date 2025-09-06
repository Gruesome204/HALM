using UnityEngine;

public class TurretLevelBehaviour : MonoBehaviour
{
    [Header("References")]
    public TurretBlueprint blueprint;
    public TurretBehaviour turretBehaviour;

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
        {
            ApplyUpgrades(newLevel);
        }
    }

    private void ApplyUpgrades(int level)
    {
        // Example upgrade logic (scale values based on level)
        float scaledDamage = blueprint.attackDamage * Mathf.Pow(1.2f, level - 1);
        float scaledFireRate = blueprint.fireRate * Mathf.Pow(0.95f, level - 1);
        float scaledRange = blueprint.attackRange + 0.5f * (level - 1);

        float dmgMult = UpgradeManager.Instance.GetDamageMultiplier(blueprint.turretType);
        float rateMult = UpgradeManager.Instance.GetFireRateMultiplier(blueprint.turretType);
        float rangeBonus = UpgradeManager.Instance.GetRangeBonus(blueprint.turretType);

        float finalDamage = scaledDamage * dmgMult;
        float finalFireRate = scaledFireRate * rateMult;
        float finalRange = scaledRange + rangeBonus;

        turretBehaviour.currentAttackDamage = finalDamage;
        turretBehaviour.currentFireRate = finalFireRate;
        turretBehaviour.currentAttackRange = finalRange;


        Debug.Log($"{blueprint.turretType} turret upgraded! Level {level} | Damage={finalDamage}, FireRate={finalFireRate}, Range={finalRange}");
    }
    public void AwardXP(float amount)
    {
        TurretLevelManager.Instance.AddXP(blueprint.turretType, amount);
    }
}

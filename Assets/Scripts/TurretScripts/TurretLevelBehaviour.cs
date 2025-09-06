using UnityEngine;

public class TurretLevelBehaviour : MonoBehaviour
{
    [Header("References")]
    public TurretBlueprint blueprint;
    public TurretBehaviour turretBehaviour;

    [Header("Per-Level Growth")]
    [SerializeField] private float damagePerLevelPct = 0.20f; // +20% damage per level
    [SerializeField] private float fireRatePerLevelPct = 0.05f; // +5% shots/sec per level
    [SerializeField] private float rangePerLevelFlat = 0.5f;  // +0.5 range per level

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
        float scaledDamage = blueprint.attackDamage * Mathf.Pow(1.2f, level - 1);
        float scaledFireRate = blueprint.fireRate * Mathf.Pow(0.95f, level - 1);
        float scaledRange = blueprint.attackRange + 0.5f * (level - 1);

        float dmgMult = UpgradeManager.Instance.GetDamageMultiplier(blueprint.turretType);
        float rateMult = UpgradeManager.Instance.GetFireRateMultiplier(blueprint.turretType);
        float rangeBonus = UpgradeManager.Instance.GetRangeBonus(blueprint.turretType);

        turretBehaviour.currentAttackDamage = scaledDamage + dmgMult;
        turretBehaviour.currentFireRate = scaledFireRate + rateMult;
        turretBehaviour.currentAttackRange = scaledRange + rangeBonus;

        Debug.Log($"{blueprint.turretType} turret upgraded! Level {level} | Damage={turretBehaviour.currentAttackDamage}, FireRate={turretBehaviour.currentFireRate}, Range={turretBehaviour.currentAttackRange}");
    }
    public void AwardXP(float amount)
    {
        if (blueprint == null || TurretLevelManager.Instance == null) return;
        TurretLevelManager.Instance.AddXP(blueprint.turretType, amount);    
    }
}

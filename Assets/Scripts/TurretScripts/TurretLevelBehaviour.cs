using UnityEngine;
using static TurretLevelManager;

public class TurretLevelBehaviour : MonoBehaviour
{
    public TurretType turretType;
    private void Start()
    {
        // Subscribe to global events
        TurretLevelManager.Instance.OnLevelUp += HandleLevelUp;

        // Apply current level upgrades
        int currentLevel = TurretLevelManager.Instance.GetLevel(turretType);
        ApplyUpgrades(currentLevel);
    }

    private void OnDestroy()
    {
        if (TurretLevelManager.Instance != null)
            TurretLevelManager.Instance.OnLevelUp -= HandleLevelUp;
    }

    private void HandleLevelUp(TurretType type, int newLevel)
    {
        if (type == turretType)
        {
            ApplyUpgrades(newLevel);
        }
    }

    private void ApplyUpgrades(int level)
    {
        // Reset to base and scale
        //damage = baseDamage * Mathf.Pow(1.2f, level - 1);
        //fireRate = baseFireRate * Mathf.Pow(0.95f, level - 1);
        //range = baseRange + 0.5f * (level - 1);

        //Debug.Log($"{turretType} turret upgraded! Level {level} | Damage={damage}, FireRate={fireRate}, Range={range}");
    }

    public void AwardXP(float amount)
    {
        TurretLevelManager.Instance.AddXP(turretType, amount);
    }
}

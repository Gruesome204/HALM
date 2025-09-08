using UnityEngine;

public class TurretUpgradeTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("Load test script");
    }
    private void OnEnable()
    {
        if (TurretLevelManager.Instance != null)
            TurretLevelManager.Instance.OnMilestoneReached += ShowUpgradeOptions;
    }

    private void OnDisable()
    {
        if (TurretLevelManager.Instance != null)
            TurretLevelManager.Instance.OnMilestoneReached -= ShowUpgradeOptions;
    }

    private void ShowUpgradeOptions(TurretType type, int level)
    {
        Debug.Log($"=== Upgrade Options for {type} Turret at Level {level} ===");

        foreach (var choice in TurretUpgradeChoiceManager.Instance.GetUpgradeChoices(type))
        {
            if (choice.triggerLevel != level) continue;

            foreach (var option in choice.options)
            {
                Debug.Log($"Option: {option.name} | " +
                          $"Damage x{option.damageMultiplier} | " +
                          $"FireRate x{option.fireRateMultiplier} | " +
                          $"Range +{option.rangeBonus} | " +
                          $"Description: {option.description}");
            }
        }
    
}
}

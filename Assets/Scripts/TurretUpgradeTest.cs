using System;
using UnityEditor;
using UnityEngine;

public class TurretUpgradeTest : MonoBehaviour
{

    private void Start()
    {
        Debug.Log("Load test script");
        if (TurretLevelManager.Instance != null)
        {
            TurretLevelManager.Instance.OnMilestoneReached += ShowUpgradeOptions;
        }
    }
    private void OnEnable()
    {
        if (TurretLevelManager.Instance == null)
        {
            Debug.Log("[Test] Waiting for TurretLevelManager...");
            return;
        }
    }


    private void OnDisable()
    {
        if (TurretLevelManager.Instance != null)
            TurretLevelManager.Instance.OnMilestoneReached -= ShowUpgradeOptions;
    }

    private void ShowUpgradeOptions(TurretType type, int level)
    {
        Debug.Log($"=== Test Upgrade Options for {type} Turret at Level {level} ===");


        var options = TurretUpgradeChoiceManager.Instance.GetAllOptionsForLevel(type, level);


        foreach (var option in options)
        {
            Debug.Log($"Option: {option.name} | Damage x{option.damageMultiplier} | " +
                      $"FireRate x{option.fireRateMultiplier} | Range +{option.rangeBonus}");
        }

    }
}

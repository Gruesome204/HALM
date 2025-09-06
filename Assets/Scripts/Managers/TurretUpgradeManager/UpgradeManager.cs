using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [SerializeField] private List<TurretUpgradeChoice> upgradeChoices;
    private Dictionary<(TurretType, int), TurretUpgradeChoice.UpgradeOption> chosenUpgrades;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        chosenUpgrades = new Dictionary<(TurretType, int), TurretUpgradeChoice.UpgradeOption>();
    }

    public void ChooseUpgrade(TurretType type, int level, TurretUpgradeChoice.UpgradeOption option)
    {
        chosenUpgrades[(type, level)] = option;
        Debug.Log($"{type} turret: Player chose {option.name} at level {level}");
    }

    public TurretUpgradeChoice.UpgradeOption GetChosenUpgrade(TurretType type, int level)
    {
        chosenUpgrades.TryGetValue((type, level), out var option);
        return option;
    }

    public float GetDamageMultiplier(TurretType type)
    {
        float multiplier = 1f;
        foreach (var kvp in chosenUpgrades)
        {
            if (kvp.Key.Item1 == type)
            {
                multiplier *= kvp.Value.damageMultiplier;
            }
        }
        return multiplier;
    }

    public float GetFireRateMultiplier(TurretType type)
    {
        float multiplier = 1f;
        foreach (var kvp in chosenUpgrades)
        {
            if (kvp.Key.Item1 == type)
            {
                multiplier *= kvp.Value.fireRateMultiplier;
            }
        }
        return multiplier;
    }

    public float GetRangeBonus(TurretType type)
    {
        float bonus = 0f;
        foreach (var kvp in chosenUpgrades)
        {
            if (kvp.Key.Item1 == type)
            {
                bonus += kvp.Value.rangeBonus;
            }
        }
        return bonus;
    }
}

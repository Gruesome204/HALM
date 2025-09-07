using System.Collections.Generic;
using UnityEngine;

public class TurretUpgradeManager : MonoBehaviour
{
    public static TurretUpgradeManager Instance { get; private set; }

    [SerializeField] private List<TurretUpgradeChoiceSO> upgradeChoices;
    private Dictionary<(TurretType, int), TurretUpgradeChoiceSO.UpgradeOption> chosenUpgrades;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        chosenUpgrades = new Dictionary<(TurretType, int), TurretUpgradeChoiceSO.UpgradeOption>();
    }

    public void ChooseUpgrade(TurretType type, int level, TurretUpgradeChoiceSO.UpgradeOption option)
    {
      
        if (chosenUpgrades.ContainsKey((type, level)))
        {
            Debug.LogWarning($"{type} turret already has an upgrade for level {level}. Overwriting.");
        }

        chosenUpgrades[(type, level)] = option;
    }

    public TurretUpgradeChoiceSO.UpgradeOption GetChosenUpgrade(TurretType type, int level)
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

    public IEnumerable<TurretUpgradeChoiceSO> GetUpgradeChoices(TurretType type)
    {
        foreach (var choice in upgradeChoices)
        {
            if (choice.turretType == type)
                yield return choice;
        }
    }
}

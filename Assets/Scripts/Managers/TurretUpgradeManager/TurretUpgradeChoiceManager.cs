using System.Collections.Generic;
using UnityEngine;

public class TurretUpgradeChoiceManager : MonoBehaviour
{
    public static TurretUpgradeChoiceManager Instance { get; private set; }

    //List of possible Upgrades(Added in Inspector as Scriptable Objects)
    [SerializeField] private List<TurretUpgradeChoiceSO> upgradeChoices;
    //Dictionary that saves all picked upgrades
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

    //Loops over all Choices and gives all of the given turret Type
    public IEnumerable<TurretUpgradeChoiceSO> GetUpgradeChoices(TurretType type)
    {
        foreach (var choice in upgradeChoices)
        {
            //Debug.Log($"Checking choice: {choice.name} for turret type {choice.turretType}");
            if (choice != null && choice.turretType == type)
                yield return choice;
        }
    }

    public IEnumerable<TurretUpgradeChoiceSO.UpgradeOption> GetAllOptionsForLevel(TurretType type, int level)
    {
        foreach (var choice in upgradeChoices)
        {
            if (choice != null && choice.turretType == type && choice.triggerLevel == level)
            {
                foreach (var option in choice.options)
                    yield return option;
            }
        }
    }

    //Player Selects and upgrade and this method saves the choice
    public void ChooseUpgrade(TurretType type, int level, TurretUpgradeChoiceSO.UpgradeOption option)
    {
      
        if (chosenUpgrades.ContainsKey((type, level)))
        {
            Debug.LogWarning($"{type} turret already has an upgrade for level {level}. Overwriting.");
        }

        chosenUpgrades[(type, level)] = option;

        if (TurretLevelManager.Instance != null)
        {
            TurretLevelManager.Instance.ForceReapplyUpgrades(type);
        }

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

    [System.Serializable]
    public struct UpgradeSaveData
    {
        public TurretType type;
        public int level;
        public string optionId; // unique ID from UpgradeOption
    }

    //Method to show all choosen upgrades
    public IEnumerable<TurretUpgradeChoiceSO.UpgradeOption> GetAllChosenUpgrades(TurretType type)
    {
        foreach (var kvp in chosenUpgrades)
        {
            if (kvp.Key.Item1 == type)
                yield return kvp.Value;
        }
    }

}

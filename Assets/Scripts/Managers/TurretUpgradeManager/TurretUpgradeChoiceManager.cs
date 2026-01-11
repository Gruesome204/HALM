using System.Collections.Generic;
using UnityEngine;

public class TurretUpgradeChoiceManager : MonoBehaviour
{
    public static TurretUpgradeChoiceManager Instance { get; private set; }

    //List of possible Upgrades(Added in Inspector as Scriptable Objects)
    [SerializeField] private List<TurretUpgradeChoiceSO> upgradeChoices;

    //Dictionary that saves all picked upgrades
    private Dictionary<(TurretType, int), TurretUpgradeChoiceSO.UpgradeOption> chosenUpgrades;
    // Tracks which UpgradeChoiceSO was already used
    private HashSet<(TurretType, int, TurretUpgradeChoiceSO)> usedChoices;

    private HashSet<string> usedOptionIds = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        chosenUpgrades = new Dictionary<(TurretType, int), TurretUpgradeChoiceSO.UpgradeOption>();
        usedChoices = new HashSet<(TurretType, int, TurretUpgradeChoiceSO)>();


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
            if (choice != null && choice.turretType == type && choice.triggerLevels.Contains(level))
            {
                foreach (var option in choice.options)
                    yield return option;
            }
        }
    }
    public IEnumerable<TurretUpgradeChoiceSO> GetAllSOOptionsForLevel(TurretType type, int level)
    {
        foreach (var choice in upgradeChoices)
        {
            if (choice != null && choice.turretType == type && choice.triggerLevels.Contains(level))
            {
                yield return choice;
            }
        }
    }

    //Player Selects and upgrade and this method saves the choice
    public void ChooseUpgrade(TurretType type, int level, TurretUpgradeChoiceSO choice, TurretUpgradeChoiceSO.UpgradeOption option)
    {
      
        if (chosenUpgrades.ContainsKey((type, level)))
        {
            Debug.LogWarning($"{type} turret already has an upgrade for level {level}. Overwriting.");
        }

        chosenUpgrades[(type, level)] = option;
        usedChoices.Add((type, level, choice));
        MarkOptionUsed(option.optionId);
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

    // Aggregate modifiers from all chosen upgrades
    public TurretModifier GetCombinedModifier(TurretType type)
    {
        TurretModifier combined = new TurretModifier();

        foreach (var kvp in chosenUpgrades)
        {
            if (kvp.Key.Item1 == type)
            {

                var mod = kvp.Value.modifier;

                // Additive approach: 0 = no change, positive = bonus, negative = penalty
                combined.damageMultiplier += mod.damageMultiplier;
                combined.fireRateMultiplier += mod.fireRateMultiplier;
                combined.projectileSpeed += mod.projectileSpeed;
                combined.healthMultiplier += mod.healthMultiplier;
                combined.turretPlacementCooldownMultiplier += mod.turretPlacementCooldownMultiplier;

                combined.projectilesPerSalve += mod.projectilesPerSalve;
                combined.piercingHits += mod.piercingHits;
                combined.rangeBonus += mod.rangeBonus;
            }
        }

        return combined;
    }

    public IEnumerable<TurretUpgradeChoiceSO> GetAvailableChoicesForLevel(
    TurretType type,
    int level)
    {
        foreach (var choice in upgradeChoices)
        {
            if (choice == null)
                continue;

             // Check turret type AND that this choice appears at this level
            if (choice.turretType != type || !choice.triggerLevels.Contains(level))
                continue;

            if (usedChoices.Contains((type, level, choice)))
                continue;

            yield return choice;
        }
    }

    // Convenience getters (percentage-based multipliers)
    public float GetDamageMultiplier(TurretType type)
    {
        // 0 = no change → returns 1 for multiplication
        return 1f + GetCombinedModifier(type).damageMultiplier;
    }

    public float GetFireRateMultiplier(TurretType type)
    {
        return 1f + GetCombinedModifier(type).fireRateMultiplier;
    }

    public float GetProjectileSpeedMultiplier(TurretType type)
    {
        return 1f + GetCombinedModifier(type).projectileSpeed;
    }

    public float GetHealthMultiplier(TurretType type)
    {
        return 1f + GetCombinedModifier(type).healthMultiplier;
    }

    public float GetTurretPlacementCooldownMultiplier(TurretType type)
    {
        return 1f + GetCombinedModifier(type).turretPlacementCooldownMultiplier;
    }

    // Additive fields remain additive (they are already flat bonuses)
    public int GetProjectilesPerSalve(TurretType type) => GetCombinedModifier(type).projectilesPerSalve;

    public float GetRangeBonus(TurretType type)
    {
        float bonus = 0f;
        foreach (var kvp in chosenUpgrades)
        {
            if (kvp.Key.Item1 == type)
                bonus += kvp.Value.modifier.rangeBonus;
        }
        return bonus;
    }

    public int GetPiercingHits(TurretType type)
    {
        int total = 0;
        foreach (var kvp in chosenUpgrades)
        {
            if (kvp.Key.Item1 == type)
                total += kvp.Value.modifier.piercingHits;
        }
        return total;
    }

    public bool IsOptionUsed(string optionId)
    {
        return usedOptionIds.Contains(optionId);
    }

    public void MarkOptionUsed(string optionId)
    {
        usedOptionIds.Add(optionId);
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

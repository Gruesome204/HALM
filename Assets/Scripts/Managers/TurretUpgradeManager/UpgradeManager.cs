using UnityEngine;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [SerializeField] private List<TurretUpgradeTree> allUpgradeTrees;
    private Dictionary<TurretType, TurretUpgradeTree> upgradeDict;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        upgradeDict = new Dictionary<TurretType, TurretUpgradeTree>();
        foreach (var tree in allUpgradeTrees)
        {
            upgradeDict[tree.turretType] = tree;
        }
    }

    public TurretUpgradeTree GetTree(TurretType type)
    {
        return upgradeDict[type];
    }

    public bool UnlockUpgrade(TurretType type, int index, int playerCurrency)
    {
        var tree = upgradeDict[type];
        var node = tree.upgrades[index];

        if (node.unlocked) return false;
        if (playerCurrency < node.cost) return false;

        node.unlocked = true;
        return true;
    }

    public float GetDamageMultiplier(TurretType type)
    {
        float total = 1f;
        foreach (var node in upgradeDict[type].upgrades)
        {
            if (node.unlocked) total *= node.damageMultiplier;
        }
        return total;
    }

    public float GetFireRateMultiplier(TurretType type)
    {
        float total = 1f;
        foreach (var node in upgradeDict[type].upgrades)
        {
            if (node.unlocked) total *= node.fireRateMultiplier;
        }
        return total;
    }

    public float GetRangeBonus(TurretType type)
    {
        float total = 0f;
        foreach (var node in upgradeDict[type].upgrades)
        {
            if (node.unlocked) total += node.rangeBonus;
        }
        return total;
    }
}

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TurretUpgradeTree", menuName = "Game/Turret/Upgrade Tree")]
public class TurretUpgradeTree : ScriptableObject
{
    public TurretType turretType;
    public List<UpgradeNode> upgrades;

    [System.Serializable]
    public class UpgradeNode
    {
        public string name;
        public string description;
        public int cost; // meta currency cost
        public float damageMultiplier = 1f;
        public float fireRateMultiplier = 1f;
        public float rangeBonus = 0f;

        [HideInInspector] public bool unlocked;
    }
}

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TurretUpgradeChoice", menuName = "Game/Turret/Upgrade Choice")]
public class TurretUpgradeChoice : ScriptableObject
{
    public TurretType turretType;
    public int triggerLevel = 10; // level at which choice appears

    public List<UpgradeOption> options;

    [System.Serializable]
    public class UpgradeOption
    {
        public string name;
        public string description;
        public float damageMultiplier = 1f;
        public float fireRateMultiplier = 1f;
        public float rangeBonus = 0f;
        public Sprite icon; // for UI
    }
}

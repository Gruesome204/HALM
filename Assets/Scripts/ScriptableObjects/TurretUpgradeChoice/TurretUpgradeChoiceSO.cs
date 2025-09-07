using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TurretUpgradeChoice", menuName = "Game/Turret/Upgrade Choice")]
public class TurretUpgradeChoiceSO : ScriptableObject
{
    public TurretType turretType;
    public int triggerLevel = 10; // level at which choice appears

    public List<UpgradeOption> options;

    [System.Serializable]
    public class UpgradeOption
    {
        public string name;
        [TextArea] public string description;
        public float damageMultiplier = 1f;
        public float fireRateMultiplier = 1f;
        public float rangeBonus = 0f;
        public Sprite icon; // for UI
    }
}

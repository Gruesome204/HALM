using System.Collections.Generic;
using UnityEngine;
using static TurretUpgradeChoiceSO;


[CreateAssetMenu(fileName = "BuildMasterModifier", menuName = "Game/PlayerModifier/NewBuildMasterModifier")]
public class BuildMasterModifier : ScriptableObject
{
    public List<Modifier> options;
    [System.Serializable]
    public class Modifier
    {
        public string name;
        [TextArea] public string description;
        public Sprite icon;
        public float goldCost;
        public float additionalHealth;
        public float additionalMaxHealth;
        public float additionalRegen;
        public float additionalArmor;
        public float additionalMagicResistance;
    }
}

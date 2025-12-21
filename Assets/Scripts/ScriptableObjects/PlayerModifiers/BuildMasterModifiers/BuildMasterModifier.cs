using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BuildMasterModifier", menuName = "Game/PlayerModifier/NewBuildMasterModifier")]
public class BuildMasterModifier : ScriptableObject
{
    [Tooltip("List of modifier options the player can choose from.")]
    public Modifier options;
    [System.Serializable]
    public struct Modifier
    {
        public string name;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Costs")]
        public ResourceCost[] costs;
        public float additionalHealth;
        public float additionalMaxHealth;
        public float additionalRegen;
        public float additionalArmor;
        public float additionalMagicResistance;
        public float additionalMovementSpeed;
    }
}

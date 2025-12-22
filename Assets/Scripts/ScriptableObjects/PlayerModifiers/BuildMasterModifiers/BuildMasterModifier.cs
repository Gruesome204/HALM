using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;


[CreateAssetMenu(fileName = "BuildMasterModifier", menuName = "Game/Modifier/NewBuildMasterModifier")]
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

        [Header("Stats Bonuses")]
        public Stats additionalStats;
    }

    [System.Serializable]
    public struct Stats
    {
        // Player stats
        public float health;
        public float maxHealth;
        public float armor;
        public float movementSpeed;

        [Header("Turret Global Stats")]
        public float turretPlacementCooldownMultiplier; // e.g., 0.9 for 10% faster placement
        public float turretHealthMultiplier;            // e.g., 1.1 for +10% turret HP
        public float turretDamageMultiplier;            // e.g., 1.2 for +20% turret damage
        public float turretFireRateMultiplier;          // e.g., 1.15 for +15% fire rate
        public int turretProjectilesPerSalve;           // e.g., +1 projectile per attack
        public float turretProjectileSpeed;             // e.g., 1.1 for +10% projectile speed
        public int turretMaxCapacityBonus;
        public float turretPlacementRadiusMultiplier;
    }

}

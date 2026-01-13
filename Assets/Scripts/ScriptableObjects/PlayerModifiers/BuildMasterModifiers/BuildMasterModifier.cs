using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;


[CreateAssetMenu(fileName = "BuildMasterModifier", menuName = "Game/Modifier/NewBuildMasterModifier")]
public class BuildMasterModifier : ScriptableObject
{

    [Tooltip("List of modifier options the player can choose from.")]
    public Modifier options;

    [System.Serializable]
    public class Modifier
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

        [Tooltip("Multiplier applied to turret placement cooldown. 0 = no change, 0.1 = 10% faster.")]
        public float turretPlacementCooldownMultiplier;

        [Tooltip("Multiplier applied to turret health. 0 = no change, 0.1 = +10% HP.")]
        public float turretHealthMultiplier;

        [Tooltip("Multiplier applied to turret damage. 0 = no change, 0.2 = +20% damage.")]
        public float turretDamageMultiplier;

        [Tooltip("Additive bonus to shots per second for all turrets.")]
        public float shotsPerSecondBonus;

        [Tooltip("Additive bonus to number of projectiles per salve.")]
        public int turretProjectilesPerSalve;

        [Tooltip("Multiplier applied to projectile speed. 0 = no change, 0.1 = +10% speed.")]
        public float turretProjectileSpeed;

        [Tooltip("Additive bonus to maximum turret capacity.")]
        public int turretMaxCapacityBonus;

        [Tooltip("Multiplier applied to turret placement radius. 0 = no change, 0.2 = +20% radius.")]
        public float turretPlacementRadiusMultiplier;

        public void SetDefaults()
        {
            // percentage-style → 0 = no change
            turretPlacementCooldownMultiplier = 0f;
            turretHealthMultiplier = 0f;
            turretDamageMultiplier = 0f;
            turretProjectileSpeed = 0f;
            turretPlacementRadiusMultiplier = 0f;

            // additive stats
            shotsPerSecondBonus = 0f;
            turretProjectilesPerSalve = 0;
            turretMaxCapacityBonus = 0;
        }
    }



}

using UnityEngine;


[CreateAssetMenu(fileName = "BuildMasterModifier", menuName = "Game/Modifier/NewBuildMasterModifier")]
public class BuildMasterModifier : ScriptableObject
{

    [Tooltip("List of modifier options the player can choose from.")]
    public BuildMasterOption options;

    [System.Serializable]
    public class BuildMasterOption
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
        [Header("Player Stats")]
        public float health;
        public float maxHealth;
        public float armor;
        public float movementSpeed;


        [Header("Turret Global Stats")]
        [Tooltip("Percentage bonus to turret placement cooldown speed (0.1 = 10% faster)")]
        public float turretPlacementCooldownBonus; 

        [Tooltip("Percentage bonus to turret health (0.1 = +10%)")]
        public float turretHealthBonus;

        [Tooltip("Percentage bonus to turret damage (0.2 = +20%)")]
        public float turretDamageBonus;

        [Tooltip("Additive bonus to shots per second")]
        public float shotsPerSecondBonus;

        [Tooltip("Additive bonus to projectiles per salve")]
        public int turretProjectilesPerSalveBonus; 

        [Tooltip("Percentage bonus to projectile speed (0.1 = +10%)")]
        public float turretProjectileSpeedBonus;

        [Tooltip("Additive bonus to max turret capacity")]
        public int turretMaxCapacityBonus;

        [Tooltip("Percentage bonus to placement radius (0.2 = +20%)")]
        public float turretPlacementRadiusBonus;

        public void ResetToDefault()
        {
            // Percentage bonuses default to 0
            turretPlacementCooldownBonus = 0f;
            turretHealthBonus = 0f;
            turretDamageBonus = 0f;
            turretProjectileSpeedBonus = 0f;
            turretPlacementRadiusBonus = 0f;

            // Additive bonuses default to 0
            shotsPerSecondBonus = 0f;
            turretProjectilesPerSalveBonus = 0;
            turretMaxCapacityBonus = 0;
        }
    }
}

public enum FiringPattern
{
    None,
    SingleShot,
    FireSalve,
    BurstFire,        // New: Multiple shots in quick succession
    ScatterShot,      // New: Shots spread in a cone
    ChainLightning,   // New: Bounces between enemies
    HomingMissile,    // New: Projectiles track enemies
    AOEShot,          // New: Area of effect damage
    SprayShot         // New: Continuous spray of projectiles
}
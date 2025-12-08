using UnityEngine;

[System.Serializable]
public class TurretModifier
{
    public float turretPlacementCooldownMultiplier = 1f;
    public float healthMultiplier = 1f;
    public float damageMultiplier = 1f;
    public float fireRateMultiplier = 1f;
    public int projectilesPerSalve = 1;
    public float projectileSpeed = 1f;
    public float rangeBonus = 0f;

    public ProjectileTypeSO projectileType;
}

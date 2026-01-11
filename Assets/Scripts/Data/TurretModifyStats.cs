public struct TurretModifyStats
{
    public float damage;
    public float fireRate;
    public float range;
    public float projectileSpeed;
    public int projectilesPerSalve;

    public static TurretModifyStats operator *(TurretModifyStats a, float mult)
    {
        a.damage *= mult;
        a.fireRate *= mult;
        a.projectileSpeed *= mult;
        return a;
    }
}

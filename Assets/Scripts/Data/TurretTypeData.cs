using UnityEngine;

public class TurretTypeData : MonoBehaviour
{
    public TurretType turretType;
}

public enum TurretType
{
    ArcherTower,
    Fast,
    CannonTower,
    Barricade,
    BallistTower,
    RepeatingCrossbow
}

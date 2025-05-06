using UnityEngine;

[CreateAssetMenu(fileName = "New Turret Blueprint", menuName = "Game/Turret/New Turret Blueprint")]
public class TurretBlueprint : ScriptableObject
{
    [Header("Info")]
    public string turretName;
    [TextArea(3, 10)] public string description;

    [Header("Values")]
    public float fireRate = 1f;
    public float fireCountdown = 0f;
    public float projectileSpeed = 10f;
    public float attackRange = 5f;
    public float damage = 10f;
    [Header("Cost")]
    public int buildingCost;
    public GameObject turretPrefab; 
}
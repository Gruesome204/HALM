using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileType", menuName = "Game/Projectile/New Projectile Type")]
public class ProjectileTypeSO : ScriptableObject
{
    public GameObject prefab;
    public float speed;
    public float damage;
    public float knockbackStrength;
    public float knockbackDuration;
}
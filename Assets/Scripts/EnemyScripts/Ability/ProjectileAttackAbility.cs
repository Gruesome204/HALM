using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy/EnemyAbilities/New ProjectileAttack")]
public class ProjectileAbility : EnemyAbility
{
    public GameObject projectilePrefab;
    public float projectileSpeed;

    protected override void Activate(GameObject user, GameObject target)
    {
        Vector2 direction = (target.transform.position - user.transform.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, user.transform.position, Quaternion.identity);
        proj.GetComponent<Rigidbody2D>().linearVelocity = direction * projectileSpeed;
    }
}

using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy/AbilityEffects/Projectile")]
public class ProjectileEffect : AbilityEffect
{
    public GameObject projectilePrefab;
    public float speed = 10f;

    public override void Apply(GameObject user, GameObject target)
    {
        if (projectilePrefab == null || target == null) return;

        Vector2 dir = (target.transform.position - user.transform.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, user.transform.position, Quaternion.identity);
        proj.GetComponent<Rigidbody2D>().linearVelocity = dir * speed;
    }
}

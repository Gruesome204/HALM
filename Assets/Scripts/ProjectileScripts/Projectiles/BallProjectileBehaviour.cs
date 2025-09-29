
using UnityEngine;
using UnityEngine.UI;

public class BallProjectileBehaviour : MonoBehaviour
{
    private DamageData damageData;
    private GameObject owner; // The turret that fired this projectile

    public float knockbackStrength = 100f;
    public float knockbackDuration = 0.1f;
    public Vector2 direction;

    public int piercingHitsRemaining = 2;

    public void SetOwner(GameObject turret, float damageAmount)
    {
        owner = turret;
        damageData = new DamageData
        {
            source = turret,
            amount = damageAmount,
            type = DamageData.DamageType.Physical
        };
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Turret"))
            return;
        if (other.gameObject == owner)
            return;
        // Try to get the IDamagable interface from the collided object
        IDamagable damagable = other.GetComponent<IDamagable>();

        if (damagable != null)
        {
            Rigidbody2D targetRb = other.GetComponent<Rigidbody2D>();
            if (targetRb != null)
            {
                direction = (targetRb.transform.position - transform.position).normalized;
            }

            KnockbackData knockbackData = new KnockbackData
            {
                knockbackStrength = knockbackStrength,
                knockbackDuration = knockbackDuration,
                direction = direction,
            };

            damagable.TakeDamage(damageData, knockbackData);
            // Reduce piercing hits remaining
            if (piercingHitsRemaining > 0) // Only decrement if not infinite piercing
            {
                piercingHitsRemaining--;
            }

            // Destroy the projectile if it has no more piercing hits remaining
            if (piercingHitsRemaining == 0)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            // Handle collisions with non-damagable objects if needed
           // Destroy(gameObject);
        }
    }
}
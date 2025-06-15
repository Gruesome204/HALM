using UnityEngine;
using UnityEngine.UI;

public class BallProjectileBehaviour : MonoBehaviour
{
    public float damageAmount = 10f;
    public GameObject sourceOfDamage;

    public float knockbackStrength = 100f;
    public float knockbackDuration = 0.1f;
    public Vector2 direction;

    public int piercingHitsRemaining = 2;

    private void OnTriggerEnter2D(Collider2D other)
    {


        // Try to get the IDamagable interface from the collided object
        IDamagable damagable = other.GetComponent<IDamagable>();

        if (damagable != null)
        {
            DamageData damageData = new DamageData
            {
                amount = damageAmount,
                source = sourceOfDamage,
                type = DamageData.DamageType.Physical // Or whatever damage type is appropriate
            };

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
            Destroy(gameObject);
        }
    }
}
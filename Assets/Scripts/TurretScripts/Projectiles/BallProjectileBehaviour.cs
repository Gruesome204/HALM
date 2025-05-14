using UnityEngine;

public class BallProjectileBehaviour : MonoBehaviour
{
    public float damageAmount = 10f;
    public float knockbackRate;
    public GameObject sourceOfDamage;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Try to get the IDamagable interface from the collided object
        IDamagable damagable = collision.gameObject.GetComponent<IDamagable>();

        // If the collided object implements IDamagable, call its TakeDamage method
        if (damagable != null)
        {
            DamageData damageData = new DamageData
            {
                amount = damageAmount,
                source = sourceOfDamage,
                type = DamageData.DamageType.Physical // Or whatever damage type is appropriate
            };

            Vector2 knockbackDirection = this.gameObject.GetComponent<Rigidbody2D>().linearVelocity.normalized;
            damagable.TakeDamage(damageData, knockbackDirection);


            // Optionally handle what happens to the projectile after hitting something damagable
            Destroy(gameObject); // Destroy the projectile upon impact
        }
        else
        {
            // The collided object does not implement IDamagable.
            // You might want to handle collisions with non-damagable objects differently.
            // For example, you might still want to destroy the projectile.
            // Or you might want it to bounce or stick.
            // Example:
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // This is similar to OnCollisionEnter2D, but for trigger colliders (no physics collision)
        IDamagable damagable = other.GetComponent<IDamagable>();

        if (damagable != null)
        {
            DamageData damageData = new DamageData
            {
                amount = damageAmount,
                source = sourceOfDamage,
                type = DamageData.DamageType.Physical // Or whatever damage type is appropriate
            };
            Vector2 knockbackDirection = this.gameObject.GetComponent<Rigidbody2D>().linearVelocity.normalized;
            damagable.TakeDamage(damageData, knockbackDirection);
            Destroy(gameObject);
        }
        else
        {
            // Handle collisions with non-damagable objects if needed
            Destroy(gameObject);
        }
    }
}
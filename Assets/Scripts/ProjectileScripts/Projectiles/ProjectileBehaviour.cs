
using UnityEngine;
using UnityEngine.UI;

public class ProjectileBehaviour : MonoBehaviour
{
    private DamageData damageData;
    private GameObject owner; // The turret that fired this projectile

    public float knockbackStrength;
    public float knockbackDuration;
    public Vector2 direction;
    private Rigidbody2D rb;

    public int remainingPierces;

    [SerializeField] private LayerMask ignoreLayers;


    public void InitializePiercing(int pierces)
    {
        remainingPierces = pierces;
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            transform.up = rb.linearVelocity.normalized;
        }
    }
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
        if (((1 << other.gameObject.layer) & ignoreLayers) != 0)
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
            if (remainingPierces > 0)
            {
                remainingPierces--;

                if (remainingPierces < 0)
                    Destroy(gameObject);
            }
        }
    }
}
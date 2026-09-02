using UnityEngine;
using System.Collections;

public class DamageOverTime : MonoBehaviour
{
    private GameObject owner;
    private float damagePerTick;
    private float tickInterval;
    private int tickCount;
    private DamageData.DamageType damageType;
    private GameObject tickEffectPrefab;
    private float knockbackStrength = 0f; // Optional knockback on each tick
    private float knockbackDuration = 0f;

    private int currentTicks = 0;
    private float timer = 0f;
    private bool isActive = false;

    private IDamagable damagable;
    private Rigidbody2D rb;

    private void Awake()
    {
        damagable = GetComponent<IDamagable>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(GameObject owner, float damagePerTick, float tickInterval, int tickCount,
                          DamageData.DamageType damageType, GameObject tickEffectPrefab,
                          float knockbackStrength = 0f, float knockbackDuration = 0f)
    {
        this.owner = owner;
        this.damagePerTick = damagePerTick;
        this.tickInterval = tickInterval;
        this.tickCount = tickCount;
        this.damageType = damageType;
        this.tickEffectPrefab = tickEffectPrefab;
        this.knockbackStrength = knockbackStrength;
        this.knockbackDuration = knockbackDuration;

        currentTicks = 0;
        timer = 0f;
        isActive = true;

        // Start the DOT
        StartCoroutine(ApplyDOT());
    }

    private IEnumerator ApplyDOT()
    {
        while (currentTicks < tickCount && isActive)
        {
            yield return new WaitForSeconds(tickInterval);

            if (!isActive || gameObject == null)
                yield break;

            ApplyTick();
            currentTicks++;
        }

        // DOT finished
        Destroy(this);
    }

    private void ApplyTick()
    {
        if (damagable == null)
        {
            damagable = GetComponent<IDamagable>();
            if (damagable == null)
                return;
        }

        // Create tick visual effect
        if (tickEffectPrefab != null)
        {
            Instantiate(tickEffectPrefab, transform.position, Quaternion.identity);
        }

        // Create damage data
        DamageData damageData = new DamageData
        {
            source = owner,
            amount = damagePerTick,
            type = damageType
        };

        // Optional knockback on tick
        KnockbackData knockbackData = new KnockbackData();
        if (knockbackStrength > 0f && rb != null)
        {
            Vector2 direction = (transform.position - owner.transform.position).normalized;
            knockbackData = new KnockbackData
            {
                knockbackStrength = knockbackStrength,
                knockbackDuration = knockbackDuration,
                direction = direction
            };
        }

        // Apply damage with optional knockback
        damagable.TakeDamage(damageData, knockbackData);

        Debug.Log($"DOT tick {currentTicks + 1}/{tickCount}: {damagePerTick} damage to {gameObject.name}");
    }

    public void StopDOT()
    {
        isActive = false;
        StopAllCoroutines();
        Destroy(this);
    }

    private void OnDestroy()
    {
        isActive = false;
        StopAllCoroutines();
    }
}
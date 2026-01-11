using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyKnockback : MonoBehaviour
{
    [Header("References")]
    public EnemyStats stats;
    public EnemyHealth health;

    [Header("Knockback Settings")]
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.5f;

    private Rigidbody2D rb;
    private bool isKnockedBack;

    [Header("Knockback I-Frames")]
    [SerializeField] private float knockbackIFrameDuration = 0.4f;
    private bool hasKnockbackIFrames;

    public bool HasKnockbackIFrames => hasKnockbackIFrames;
    // Indicates whether the enemy is currently affected by knockback.
    public bool IsKnockedBack => isKnockedBack;

    private void OnEnable()
    {
        health = GetComponent<EnemyHealth>();
        health.OnDamaged += HandleDamaged;
    }

    private void HandleDamaged(DamageData damageData, KnockbackData knockbackData)
    {
        if (hasKnockbackIFrames)
            return;
        ApplyKnockback(knockbackData.direction, knockbackData.knockbackStrength);
    }
    private void OnDisable()
    {
        health.OnDamaged -= HandleDamaged;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats ??= GetComponent<EnemyStats>();
    }
    // Applies a knockback force in the specified direction if not already knocked back.
    public void ApplyKnockback(Vector2 direction, float strength)
    {
        if (isKnockedBack || stats == null) return;
        StartCoroutine(KnockbackRoutine(direction, strength));
    }
    // Coroutine that applies and resolves knockback b  
    private IEnumerator KnockbackRoutine(Vector2 direction, float strength)
    {
        isKnockedBack = true;
        hasKnockbackIFrames = true;
        rb.linearVelocity = Vector2.zero;

        float adjustedForce =
            strength * knockbackForce * (1f - Mathf.Clamp01(stats.currentKnockbackReduction));

        rb.AddForce(direction.normalized * adjustedForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        rb.linearVelocity = Vector2.zero;
        isKnockedBack = false;

        // Remaining i-frame duration (if any)
        yield return new WaitForSeconds(
            Mathf.Max(0f, knockbackIFrameDuration - knockbackDuration)
        );


        hasKnockbackIFrames = false;
    }
}
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
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

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
        ApplyKnockback(knockbackData.direction,
                  knockbackData.knockbackStrength,
                  knockbackData.knockbackDuration);
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
    public void ApplyKnockback(Vector2 direction, float strength, float knockbackDuration)
    {
        if (isKnockedBack || stats == null) return;
        StartCoroutine(KnockbackRoutine(direction, strength, knockbackDuration));
    }
    // Coroutine that applies and resolves knockback b  
    private IEnumerator KnockbackRoutine(Vector2 direction, float strength, float knockbackDuration)
    {
        isKnockedBack = true;
        hasKnockbackIFrames = true;
        rb.linearVelocity = Vector2.zero;

        float adjustedForce =
            strength * knockbackForce * (1f - Mathf.Clamp01(stats.currentKnockbackReduction));

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector2.up;

        rb.AddForce(direction.normalized * adjustedForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        rb.linearVelocity *= 0.1f;
        isKnockedBack = false;

        // Remaining i-frame duration (if any)
        yield return new WaitForSeconds(
            Mathf.Max(0f, knockbackIFrameDuration - knockbackDuration)
        );


        hasKnockbackIFrames = false;
    }
}
using System.Collections;
using UnityEngine;

public class EnemyKnockback : MonoBehaviour
{
    [Header("References")]
    public EnemyStats stats;

    [Header("Knockback Settings")]
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.5f;

    private Rigidbody2D rb;
    private bool isKnockedBack;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats ??= GetComponent<EnemyStats>();
    }
    // Applies a knockback force in the specified direction if not already knocked back.
    public void ApplyKnockback(Vector2 direction)
    {
        if (isKnockedBack || stats == null) return;
        StartCoroutine(KnockbackRoutine(direction));
    }
    // Coroutine that applies and resolves knockback behavior over time.
    private IEnumerator KnockbackRoutine(Vector2 direction)
    {
        isKnockedBack = true;
        rb.linearVelocity = Vector2.zero;

        float adjustedForce = knockbackForce * (1f - Mathf.Clamp01(stats.currentKnockbackReduction));
        rb.AddForce(direction.normalized * adjustedForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);
        rb.linearVelocity = Vector2.zero;
        isKnockedBack = false;
    }
    // Indicates whether the enemy is currently affected by knockback.
    public bool IsKnockedBack => isKnockedBack;
}
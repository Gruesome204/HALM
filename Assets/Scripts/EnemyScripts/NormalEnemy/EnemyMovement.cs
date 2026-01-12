using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyStats stats;
    private EnemyAnimator enemyAnimator;
    public EnemyKnockback knockback;

    private Rigidbody2D rb;
    private bool isPaused;
    private bool ignorePursueRange;

    public GameObject target;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<EnemyStats>();
        knockback = GetComponent<EnemyKnockback>();
        enemyAnimator = GetComponent<EnemyAnimator>();
    }

    private void FixedUpdate()
    {
        if (isPaused) return;
        if (knockback != null && knockback.IsKnockedBack) return;

        if (target != null)
            MoveTowardTarget(target);
        else
            Stop(); // Stop if no target
    }

    public void SetTarget(GameObject newTarget)
    {
        target = newTarget;
    }

    //Enables or disables movement and physics interaction
    public void SetPaused(bool paused)
    {
        isPaused = paused;

        if (paused)
            PauseMovement();
        else
            ResumeMovement();
    }

    // Moves the enemy toward the current target if within pursue range

    public void MoveTowardTarget(GameObject moveTarget)
    {
        if (target == null || stats == null) return;

        Vector2 direction = (moveTarget.transform.position - transform.position).normalized;
        rb.linearVelocity = direction * stats.currentMovementSpeed;
        enemyAnimator?.SetMoveSpeed(rb.linearVelocity.magnitude);
    }


    //Stops the enemy completely
    public void Stop()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        enemyAnimator?.SetMoveSpeed(0f);
    }

    private void PauseMovement()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        enemyAnimator?.SetMoveSpeed(0f);
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    private void ResumeMovement()
    {
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
}

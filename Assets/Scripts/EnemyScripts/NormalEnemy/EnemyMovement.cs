using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyStats stats;
    public GameObject target;
    public EnemyKnockback knockback;

    private Rigidbody2D rb;
    private bool isPaused;
    private bool ignorePursueRange;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<EnemyStats>();
        knockback = GetComponent<EnemyKnockback>();
    }
    public void EnableForcedChase()
    {
        ignorePursueRange = true;
    }

    public void DisableForcedChase()
    {
        ignorePursueRange = false;
    }


    private void FixedUpdate()
    {
        if (isPaused) return;
        if (knockback != null && knockback.IsKnockedBack)
            return; 
        MoveTowardTarget();
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
    public void MoveTowardTarget()
    {
        if (target == null || stats == null) return;

        float distance = Vector2.Distance(transform.position, target.transform.position);

        if (!ignorePursueRange && distance > stats.currentPursueRange)
            return;

        Vector2 dir = (target.transform.position - transform.position).normalized;
        rb.linearVelocity = Vector2.Lerp(
            rb.linearVelocity,
            dir * stats.currentMovementSpeed,
            10 * Time.fixedDeltaTime
        );
    }


    //Stops the enemy completely
    public void Stop()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void PauseMovement()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    private void ResumeMovement()
    {
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
}

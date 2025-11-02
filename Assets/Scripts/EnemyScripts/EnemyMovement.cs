using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public EnemyStats stats;
    public GameObject target;

    private Rigidbody2D rb;
    private bool isPaused;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        stats = GetComponent<EnemyStats>();
    }

    private void FixedUpdate()
    {

        if (isPaused)
        {
            rb.linearVelocity = Vector2.zero; // stop immediately while paused
            return;
        }
        if (target != null)
            MoveTowardTarget();
        else
            rb.linearVelocity = Vector2.zero;
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;

        if (paused)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void MoveTowardTarget()
    {
        if (isPaused || target == null) return;


        Vector2 direction = ((Vector2)target.transform.position - rb.position).normalized;
        rb.linearVelocity = direction * stats.currentMovementSpeed;
    }

    public void Stop()
    {
        rb.linearVelocity = Vector2.zero;
    }

}

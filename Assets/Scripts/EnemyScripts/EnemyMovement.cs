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
        //else
        //    rb.linearVelocity = Vector2.zero;
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
        if (target == null || stats == null) return;

        float distance = Vector2.Distance(transform.position, target.transform.position);
        if (distance > stats.currentPursueRange)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = (target.transform.position - transform.position).normalized;
        rb.linearVelocity = dir * stats.currentMovementSpeed;
    }

    public void Stop()
    {
        rb.linearVelocity = Vector2.zero;
    }

}

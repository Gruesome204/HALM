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
        if (isPaused) return;

        MoveTowardTarget();
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;

        if (paused)
        {
            // Stop all movement and freeze physics
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        else
        {
            // Unfreeze physics, allow rotation if needed
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    public void MoveTowardTarget()
    {
        LookForTarget();

        if (target == null || stats == null) return;

        float distance = Vector2.Distance(transform.position, target.transform.position);
        if (distance > stats.currentPursueRange) return;

        Vector2 dir = (target.transform.position - transform.position).normalized;
        rb.MovePosition(rb.position + dir * stats.currentMovementSpeed * Time.fixedDeltaTime);
    }

    public void Stop()
    {
        rb.linearVelocity = Vector2.zero;
    }

    private void LookForTarget()
    {
        //Only find and follow the Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            target = null;
            return;
        }

        // Assign player as the only target
        if (target != player)
        {
            target = player;
            // Debug.Log($"{gameObject.name} now targeting Player");
        }
    }
}

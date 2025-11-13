using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    public EnemyStats stats;
    public GameObject target;

    private Rigidbody2D rb;
    private bool isPaused;
    private bool hasForcedTarget;

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

    //Enables or disables movement and physics interaction
    public void SetPaused(bool paused)
    {
        isPaused = paused;

        if (paused)
            PauseMovement();
        else
            ResumeMovement();
    }

    public void SetTargetPlayer(bool ignorePursueRange = false)
    {
        Debug.Log("call set target");
        target = GameObject.FindGameObjectWithTag("Player");
        this.hasForcedTarget = false;
    }

    // Moves the enemy toward the current target if within pursue range
    public void MoveTowardTarget()
    {
        UpdateTarget();

        if (target == null || stats == null) return;

        float distance = Vector2.Distance(transform.position, target.transform.position);
        if (!hasForcedTarget && distance > stats.currentPursueRange) return;

        Vector2 dir = (target.transform.position - transform.position).normalized;
        rb.MovePosition(rb.position + dir * stats.currentMovementSpeed * Time.fixedDeltaTime);
    }

    //Stops the enemy completely
    public void Stop()
    {
        rb.linearVelocity = Vector2.zero;
    }

    private void UpdateTarget()
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

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
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject[] turrets = GameObject.FindGameObjectsWithTag("Turret");

        GameObject closest = null;
        float closestDistance = Mathf.Infinity;

        if (player != null)
        {
            float dist = Vector2.Distance(transform.position, player.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = player;
            }
        }

        foreach (GameObject turret in turrets)
        {
            float dist = Vector2.Distance(transform.position, turret.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = turret;
            }
        }

        if (closest != null && closest != target)
        {
            target = closest;
            Debug.Log($"{gameObject.name} found target: {target.name}");
        }
    }
}

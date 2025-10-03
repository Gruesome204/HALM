using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public EnemyStats stats;
    public GameObject target;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Call this in FixedUpdate from EnemyBehaviour
    public void MoveTowardTarget()
    {
        LookForTarget();

        if (target == null || stats == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

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
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    // Finds the closest target (player or turret)
    private void LookForTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject[] turrets = GameObject.FindGameObjectsWithTag("Turret");

        GameObject closest = null;
        float closestDistance = Mathf.Infinity;

        // Check player
        if (player != null)
        {
            float dist = Vector2.Distance(transform.position, player.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = player;
            }
        }

        // Check turrets
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

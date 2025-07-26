using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public EnemyStats stats;
    public GameObject target;
    public float pursueRange = 5f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (target == null)
        {
            LookForTarget();
        }
    }

    public void LookForTarget()
    {
        GameObject found = GameObject.FindGameObjectWithTag("Player");
        if (found != null)
        {
            target = found;
            Debug.Log($"{gameObject.name} found target: {target.name}");
        }
    }

    public void MoveTowardTarget()
    {
        if (target == null || stats == null) return;

        float distance = Vector2.Distance(transform.position, target.transform.position);
        if (distance > pursueRange)
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
}

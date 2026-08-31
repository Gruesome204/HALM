using System.Collections.Generic;
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
    public bool isAggroed = false;

    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float wallCheckDistance = 0.5f;

    private List<Vector2Int> currentPath = new();
    private int currentIndex = 0;

    [SerializeField] private float nodeReachDistance = 0.1f;

    [SerializeField] private float pathUpdateRate = 1f;
    private float pathTimer;

    [SerializeField] private int lookAheadSteps = 2;

    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float slowDownDistance = 0.3f;
    [SerializeField] private float arriveDistance = 0.08f;



    private Vector2 smoothVelocity;


    public GameObject target;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<EnemyStats>();
        knockback = GetComponent<EnemyKnockback>();
        enemyAnimator = GetComponent<EnemyAnimator>();
    }

    private void Start()
    {
        AcquirePlayerTarget();
        isAggroed = false;
    }
    public void AcquirePlayerTarget()
    {
        if (target != null) return; // Already have a target
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player;
        }
        else
        {
            Debug.LogWarning($"{name}: Player not found in scene!");
        }
    }

    private void Update()
    {
        pathTimer += Time.deltaTime;

        if (pathTimer >= pathUpdateRate)
        {
            pathTimer = 0f;

            GeneratePath();
        }
    }

    private void FixedUpdate()
    {
        if (isPaused) return;
        if (knockback != null && knockback.IsKnockedBack)
            return; 
        //MoveTowardTarget();

        FollowPath();
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

        Vector2 dir = (target.transform.position - transform.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            dir,
            wallCheckDistance,
            obstacleLayer
        );

        if (hit.collider != null)
        {
            // Wall detected → stop or steer
            rb.linearVelocity = Vector2.zero;
            enemyAnimator?.SetMoveSpeed(0f);

            return;
        }

        rb.linearVelocity = dir * stats.currentMovementSpeed;

        enemyAnimator?.SetMoveSpeed(rb.linearVelocity.magnitude);
    }


    public void GeneratePath()
    {
        if (target == null)
            return;

        if (GridManager.Instance == null || GridPathfinding.Instance == null)
            return;

        Vector2Int start =
            GridManager.Instance.GetGridCoordinates(transform.position);

        Vector2Int goal =
            GridManager.Instance.GetGridCoordinates(target.transform.position);

        // If the goal is not walkable, find the nearest walkable cell
        if (!GridManager.Instance.IsWalkable(goal))
        {
            goal = FindNearestWalkableCell(goal);
        }

        currentPath = GridPathfinding.Instance.FindPath(start, goal);

        // Only smooth if the path is long enough
        if (currentPath.Count > 3)
        {
            currentPath = SmoothPath(currentPath);
        }
        else
        {
            // For short paths, don't smooth (avoid cutting corners)
            currentPath = new List<Vector2Int>(currentPath);
        }

        currentIndex = 0;
    }

    Vector2Int FindNearestWalkableCell(Vector2Int start)
    {
        // Simple BFS to find nearest walkable cell
        Queue<Vector2Int> queue = new();
        HashSet<Vector2Int> visited = new();
        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (GridManager.Instance.IsWalkable(current))
                return current;

            foreach (var dir in new Vector2Int[] {
            new(1,0), new(-1,0), new(0,1), new(0,-1),
            new(1,1), new(1,-1), new(-1,1), new(-1,-1)
        })
            {
                Vector2Int next = current + dir;
                if (!visited.Contains(next) &&
                    next.x >= 0 && next.y >= 0 &&
                    next.x < GridManager.Instance.gridWidth &&
                    next.y < GridManager.Instance.gridHeight)
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }
        return start; // Fallback
    }
    public void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (currentIndex >= currentPath.Count)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Reduce lookAheadSteps for more accurate following
        int targetIndex = Mathf.Min(currentIndex + lookAheadSteps, currentPath.Count - 1);

        Vector3 targetWorld =
            GridManager.Instance.GetWorldPosition(currentPath[targetIndex], Vector2Int.one);

        Vector2 toTarget = (targetWorld - transform.position);
        float distance = toTarget.magnitude;

        // If very close to destination, stop
        if (distance < arriveDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = toTarget.normalized;

        // Slow down when approaching the next node
        float speedMultiplier = Mathf.Clamp01(distance / slowDownDistance);
        Vector2 targetVelocity = dir * stats.currentMovementSpeed * speedMultiplier;

        // More responsive movement
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * acceleration);

        // Advance to next node
        Vector3 nextNode =
            GridManager.Instance.GetWorldPosition(currentPath[currentIndex], Vector2Int.one);

        if (Vector2.Distance(transform.position, nextNode) < nodeReachDistance)
        {
            currentIndex++;
        }
    }
    public List<Vector2Int> SmoothPath(List<Vector2Int> path)
    {
        if (path.Count < 3)
            return path;

        List<Vector2Int> smooth = new();
        smooth.Add(path[0]);

        int current = 0;

        while (current < path.Count - 1)
        {
            int next = path.Count - 1;

            for (int i = path.Count - 1; i > current; i--)
            {
                if (HasLineOfSight(path[current], path[i]))
                {
                    next = i;
                    break;
                }
            }

            smooth.Add(path[next]);
            current = next;
        }

        return smooth;
    }
    bool HasLineOfSight(Vector2Int a, Vector2Int b)
    {
        Vector3 worldA = GridManager.Instance.GetWorldPosition(a, Vector2Int.one);
        Vector3 worldB = GridManager.Instance.GetWorldPosition(b, Vector2Int.one);

        Vector2 dir = (worldB - worldA).normalized;
        float dist = Vector2.Distance(worldA, worldB);

        // Use a sphere cast or box cast to account for enemy size
        float enemyRadius = 0.25f; // Adjust based on your enemy size

        RaycastHit2D hit = Physics2D.CircleCast(
            worldA + (Vector3)dir * 0.1f,
            enemyRadius,
            dir,
            dist - 0.2f,
            obstacleLayer
        );

        return hit.collider == null;
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
    private void OnDrawGizmos()
    {
        if (currentPath == null || currentPath.Count == 0) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < currentPath.Count - 1; i++)
        {
            Vector3 a = GridManager.Instance.GetWorldPosition(currentPath[i], Vector2Int.one);
            Vector3 b = GridManager.Instance.GetWorldPosition(currentPath[i + 1], Vector2Int.one);
            Gizmos.DrawLine(a, b);
            Gizmos.DrawWireSphere(a, 0.1f);
        }
    }
}

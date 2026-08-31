using System.Collections.Generic;
using System.Xml.Linq;
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

    // Variables for LOS tracking
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float directMovementRange = 2f;
    [SerializeField] private float loseSightTimerMax = 3f;
    private float loseSightTimer = 0f;
    private bool hasSeenPlayer = false;
    private Vector2 lastKnownPlayerPosition;

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
        hasSeenPlayer = false;
        loseSightTimer = 0f;
    }

    public void AcquirePlayerTarget()
    {
        if (target != null) return;
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

    // ADD THIS METHOD BACK - Enables or disables movement and physics interaction
    public void SetPaused(bool paused)
    {
        isPaused = paused;

        if (paused)
            PauseMovement();
        else
            ResumeMovement();
    }

    private void Update()
    {
        pathTimer += Time.deltaTime;

        if (pathTimer >= pathUpdateRate)
        {
            pathTimer = 0f;
            CheckPlayerVisibilityAndUpdatePath();
        }
    }

    private void CheckPlayerVisibilityAndUpdatePath()
    {
        if (target == null) return;

        bool canSeePlayer = HasLineOfSightToPlayer();
        float distanceToPlayer = Vector2.Distance(transform.position, target.transform.position);

        if (canSeePlayer && distanceToPlayer <= detectionRange)
        {
            // Player is visible and in range
            hasSeenPlayer = true;
            isAggroed = true;
            lastKnownPlayerPosition = target.transform.position;
            loseSightTimer = 0f;

            GeneratePath();
        }
        else if (hasSeenPlayer && isAggroed)
        {
            // We've seen the player before but lost sight
            loseSightTimer += pathUpdateRate;

            if (loseSightTimer >= loseSightTimerMax)
            {
                // Lost the player for too long - give up chase
                hasSeenPlayer = false;
                isAggroed = false;
                currentPath.Clear();
                rb.linearVelocity = Vector2.zero;
                enemyAnimator?.SetMoveSpeed(0f);
                Debug.Log($"{name} lost the player and stopped chasing");
            }
            else
            {
                // Continue moving to last known position
                float distToLastKnown = Vector2.Distance(transform.position, lastKnownPlayerPosition);

                if (distToLastKnown < arriveDistance)
                {
                    // Reached last known position, stop and wait
                    currentPath.Clear();
                    rb.linearVelocity = Vector2.zero;
                    enemyAnimator?.SetMoveSpeed(0f);
                }
                else
                {
                    // Move to last known position
                    GeneratePathToPosition(lastKnownPlayerPosition);
                }
            }
        }
        else
        {
            // Never seen the player or out of range
            if (!isAggroed)
            {
                currentPath.Clear();
                rb.linearVelocity = Vector2.zero;
                enemyAnimator?.SetMoveSpeed(0f);
            }
        }
    }

    public void GeneratePathToPosition(Vector2 targetPosition)
    {
        if (GridManager.Instance == null || GridPathfinding.Instance == null)
            return;

        Vector2Int start = GridManager.Instance.GetGridCoordinates(transform.position);
        Vector2Int goal = GridManager.Instance.GetGridCoordinates(targetPosition);

        if (!GridManager.Instance.IsWalkable(goal))
        {
            goal = FindNearestWalkableCell(goal);
        }

        currentPath = GridPathfinding.Instance.FindPath(start, goal);

        if (currentPath.Count > 3)
        {
            currentPath = SmoothPath(currentPath);
        }
        else
        {
            currentPath = new List<Vector2Int>(currentPath);
        }

        currentIndex = 0;
    }

    private void FixedUpdate()
    {
        if (isPaused) return;
        if (knockback != null && knockback.IsKnockedBack) return;

        if (!isAggroed || target == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Check if we're trying to follow the player or last known position
        float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);

        // If we can see the player and they're in range, track them directly
        if (HasLineOfSightToPlayer() && distanceToTarget <= detectionRange)
        {
            // Use direct movement when close
            if (distanceToTarget < directMovementRange)
            {
                MoveTowardTarget();
            }
            else
            {
                FollowPath();
            }
        }
        else if (hasSeenPlayer && loseSightTimer < loseSightTimerMax)
        {
            // Move toward last known position
            float distToLastKnown = Vector2.Distance(transform.position, lastKnownPlayerPosition);

            if (distToLastKnown < directMovementRange)
            {
                // Direct movement to last known position
                Vector2 dir = (lastKnownPlayerPosition - (Vector2)transform.position).normalized;

                RaycastHit2D hit = Physics2D.Raycast(
                    transform.position,
                    dir,
                    wallCheckDistance,
                    obstacleLayer
                );

                if (hit.collider != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    enemyAnimator?.SetMoveSpeed(0f);
                    return;
                }

                rb.linearVelocity = dir * stats.currentMovementSpeed;
                enemyAnimator?.SetMoveSpeed(rb.linearVelocity.magnitude);
            }
            else
            {
                FollowPath();
            }
        }
        else
        {
            // Not aggroed or lost the player
            rb.linearVelocity = Vector2.zero;
            enemyAnimator?.SetMoveSpeed(0f);
        }
    }

    private bool HasLineOfSightToPlayer()
    {
        if (target == null) return false;

        Vector2 direction = (target.transform.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, target.transform.position);

        if (distance > detectionRange) return false;

        float enemyRadius = 0.2f;

        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position,
            enemyRadius,
            direction,
            distance,
            obstacleLayer
        );

        if (hit.collider != null)
        {
            return hit.collider.CompareTag("Player");
        }

        return true;
    }

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

        Vector2Int start = GridManager.Instance.GetGridCoordinates(transform.position);
        Vector2Int goal = GridManager.Instance.GetGridCoordinates(target.transform.position);

        if (!GridManager.Instance.IsWalkable(goal))
        {
            goal = FindNearestWalkableCell(goal);
        }

        currentPath = GridPathfinding.Instance.FindPath(start, goal);

        if (currentPath.Count > 3)
        {
            currentPath = SmoothPath(currentPath);
        }
        else
        {
            currentPath = new List<Vector2Int>(currentPath);
        }

        currentIndex = 0;
    }

    Vector2Int FindNearestWalkableCell(Vector2Int start)
    {
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
        return start;
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

        // Check if the next node is reachable
        if (currentIndex < currentPath.Count)
        {
            Vector3 nextNodePos = GridManager.Instance.GetWorldPosition(currentPath[currentIndex], Vector2Int.one);
            Vector2 toNode = (nextNodePos - transform.position);

            RaycastHit2D wallHit = Physics2D.Raycast(
                transform.position,
                toNode.normalized,
                toNode.magnitude + 0.5f,
                obstacleLayer
            );

            if (wallHit.collider != null)
            {
                GeneratePath();
                return;
            }
        }

        int targetIndex = Mathf.Min(currentIndex + lookAheadSteps, currentPath.Count - 1);

        Vector3 targetWorld = GridManager.Instance.GetWorldPosition(currentPath[targetIndex], Vector2Int.one);

        Vector2 toTarget = (targetWorld - transform.position);
        float distance = toTarget.magnitude;

        if (distance < arriveDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = toTarget.normalized;

        float speedMultiplier = Mathf.Clamp01(distance / slowDownDistance);
        Vector2 targetVelocity = dir * stats.currentMovementSpeed * speedMultiplier;

        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * acceleration);

        Vector3 currentNodePos = GridManager.Instance.GetWorldPosition(currentPath[currentIndex], Vector2Int.one);

        if (Vector2.Distance(transform.position, currentNodePos) < nodeReachDistance)
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

        float enemyRadius = 0.25f;

        RaycastHit2D hit = Physics2D.CircleCast(
            worldA + (Vector3)dir * 0.1f,
            enemyRadius,
            dir,
            dist - 0.2f,
            obstacleLayer
        );

        return hit.collider == null;
    }

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

    public void ResetChaseState()
    {
        isAggroed = false;
        hasSeenPlayer = false;
        loseSightTimer = 0f;
        currentPath.Clear();
        rb.linearVelocity = Vector2.zero;
        enemyAnimator?.SetMoveSpeed(0f);
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

        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            if (hasSeenPlayer)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(lastKnownPlayerPosition, 0.3f);
            }
        }
    }
}
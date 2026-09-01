using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    #region Serialized Fields
    [Header("References")]
    [SerializeField] private EnemyStats stats;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Movement Settings")]
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float slowDownDistance = 0.3f;
    [SerializeField] private float arriveDistance = 0.08f;

    [Header("Pathfinding Settings")]
    [SerializeField] private float pathUpdateRate = 1f;
    [SerializeField] private float nodeReachDistance = 0.1f;
    [SerializeField] private int lookAheadSteps = 2;
    [SerializeField] private float wallCheckDistance = 0.5f;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float directMovementRange = 2f;
    [SerializeField] private float loseSightTimerMax = 3f;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private bool showPathGizmos = true;
    [SerializeField] private bool showDetectionRangeGizmo = true;
    [SerializeField] private bool showLastKnownPositionGizmo = true;
    [SerializeField] private Color pathColor = Color.yellow;
    [SerializeField] private Color detectionRangeColor = Color.red;
    [SerializeField] private Color lastKnownPositionColor = Color.cyan;
    #endregion

    #region Private Fields
    private Rigidbody2D rb;
    private EnemyAnimator enemyAnimator;
    private EnemyKnockback knockback;

    private List<Vector2Int> currentPath = new();
    private int currentIndex = 0;
    private float pathTimer;
    private Vector2 smoothVelocity;

    private bool isPaused;
    private bool hasSeenPlayer = false;
    private float loseSightTimer = 0f;
    private Vector2 lastKnownPlayerPosition;
    #endregion

    #region Public Properties
    public GameObject target { get; set; }
    public bool isAggroed { get; set; } = false;
    public EnemyKnockback knockbackComponent => knockback;
    #endregion

    #region Unity Callbacks
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
        ResetChaseState();
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

    private void FixedUpdate()
    {
        if (ShouldSkipMovement()) return;
        ProcessMovement();
    }
    #endregion

    #region Public Methods
    public void AcquirePlayerTarget()
    {
        if (target != null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        target = player != null ? player : null;

        if (target == null)
            Debug.LogWarning($"{name}: Player not found in scene!");
    }

    public void ForceAggroOnPlayer(GameObject playerTarget)
    {
        if (playerTarget == null) return;

        target = playerTarget;
        isAggroed = true;
        hasSeenPlayer = true;
        lastKnownPlayerPosition = playerTarget.transform.position;
        loseSightTimer = 0f;

        GeneratePathToPosition(lastKnownPlayerPosition);
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
        if (paused) PauseMovement();
        else ResumeMovement();
    }

    public void GeneratePathToPosition(Vector2 targetPosition)
    {
        if (!IsPathfindingAvailable()) return;

        Vector2Int start = GridManager.Instance.GetGridCoordinates(transform.position);
        Vector2Int goal = GridManager.Instance.GetGridCoordinates(targetPosition);

        if (!GridManager.Instance.IsWalkable(goal))
            goal = FindNearestWalkableCell(goal);

        currentPath = GridPathfinding.Instance.FindPath(start, goal);
        currentPath = SmoothPath(currentPath);
        currentIndex = 0;
    }

    public void GeneratePath()
    {
        if (target == null || !IsPathfindingAvailable()) return;

        Vector2Int start = GridManager.Instance.GetGridCoordinates(transform.position);
        Vector2Int goal = GridManager.Instance.GetGridCoordinates(target.transform.position);

        if (!GridManager.Instance.IsWalkable(goal))
            goal = FindNearestWalkableCell(goal);

        currentPath = GridPathfinding.Instance.FindPath(start, goal);
        currentPath = SmoothPath(currentPath);
        currentIndex = 0;
    }

    public void FollowPath()
    {
        if (!HasValidPath()) return;

        if (currentIndex >= currentPath.Count)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (IsPathBlocked())
        {
            GeneratePath();
            return;
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
        enemyAnimator?.SetMoveSpeed(rb.linearVelocity.magnitude);

        CheckNodeReached();
    }

    public void MoveTowardTarget()
    {
        if (target == null || stats == null) return;

        Vector2 dir = (target.transform.position - transform.position).normalized;

        if (IsWallInFront(dir))
        {
            rb.linearVelocity = Vector2.zero;
            enemyAnimator?.SetMoveSpeed(0f);
            return;
        }

        rb.linearVelocity = dir * stats.currentMovementSpeed;
        enemyAnimator?.SetMoveSpeed(rb.linearVelocity.magnitude);
    }

    public void Stop()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        enemyAnimator?.SetMoveSpeed(0f);
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

    public List<Vector2Int> SmoothPath(List<Vector2Int> path)
    {
        if (path == null || path.Count < 3) return path ?? new List<Vector2Int>();

        List<Vector2Int> smooth = new() { path[0] };
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

    public bool HasLineOfSight(Vector2Int a, Vector2Int b)
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

    public void ToggleDebugGizmos(bool enabled) => showDebugGizmos = enabled;
    public void TogglePathGizmos(bool enabled) => showPathGizmos = enabled;
    public void ToggleDetectionRangeGizmo(bool enabled) => showDetectionRangeGizmo = enabled;
    public void ToggleLastKnownPositionGizmo(bool enabled) => showLastKnownPositionGizmo = enabled;
    public void SetDebugColors(Color path, Color detection, Color lastKnown)
    {
        pathColor = path;
        detectionRangeColor = detection;
        lastKnownPositionColor = lastKnown;
    }
    #endregion

    #region Private Methods
    private bool ShouldSkipMovement()
    {
        return isPaused || (knockback != null && knockback.IsKnockedBack);
    }

    private void ProcessMovement()
    {
        if (!isAggroed || target == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);

        if (HasLineOfSightToPlayer() && distanceToTarget <= detectionRange)
        {
            if (distanceToTarget < directMovementRange)
                MoveTowardTarget();
            else
                FollowPath();
        }
        else if (hasSeenPlayer && loseSightTimer < loseSightTimerMax)
        {
            MoveToLastKnownPosition();
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            enemyAnimator?.SetMoveSpeed(0f);
        }
    }

    private void CheckPlayerVisibilityAndUpdatePath()
    {
        if (target == null) return;

        bool canSeePlayer = HasLineOfSightToPlayer();
        float distanceToPlayer = Vector2.Distance(transform.position, target.transform.position);

        if (canSeePlayer && distanceToPlayer <= detectionRange)
        {
            UpdatePlayerInSight();
        }
        else if (hasSeenPlayer && isAggroed)
        {
            HandlePlayerLost();
        }
        else if (!isAggroed)
        {
            currentPath.Clear();
            rb.linearVelocity = Vector2.zero;
            enemyAnimator?.SetMoveSpeed(0f);
        }
    }

    private void UpdatePlayerInSight()
    {
        hasSeenPlayer = true;
        isAggroed = true;
        lastKnownPlayerPosition = target.transform.position;
        loseSightTimer = 0f;
        GeneratePath();
    }

    private void HandlePlayerLost()
    {
        loseSightTimer += pathUpdateRate;

        if (loseSightTimer >= loseSightTimerMax)
        {
            HandleLoseSightTimeout();
        }
        else
        {
            MoveToLastKnownPosition();
        }
    }

    private void HandleLoseSightTimeout()
    {
        EnemyBehaviour behaviour = GetComponent<EnemyBehaviour>();
        if (behaviour != null && behaviour.IsAggroedByTurret())
        {
            float distToLastKnown = Vector2.Distance(transform.position, lastKnownPlayerPosition);
            if (distToLastKnown < arriveDistance)
            {
                currentPath.Clear();
                rb.linearVelocity = Vector2.zero;
                enemyAnimator?.SetMoveSpeed(0f);
            }
            else
            {
                GeneratePathToPosition(lastKnownPlayerPosition);
            }
            return;
        }

        // Lost the player for too long - give up chase
        hasSeenPlayer = false;
        isAggroed = false;
        currentPath.Clear();
        rb.linearVelocity = Vector2.zero;
        enemyAnimator?.SetMoveSpeed(0f);
    }

    private void MoveToLastKnownPosition()
    {
        float distToLastKnown = Vector2.Distance(transform.position, lastKnownPlayerPosition);

        if (distToLastKnown < arriveDistance)
        {
            currentPath.Clear();
            rb.linearVelocity = Vector2.zero;
            enemyAnimator?.SetMoveSpeed(0f);
            return;
        }

        if (distToLastKnown < directMovementRange)
        {
            Vector2 dir = (lastKnownPlayerPosition - (Vector2)transform.position).normalized;
            if (!IsWallInFront(dir))
            {
                rb.linearVelocity = dir * stats.currentMovementSpeed;
                enemyAnimator?.SetMoveSpeed(rb.linearVelocity.magnitude);
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                enemyAnimator?.SetMoveSpeed(0f);
            }
        }
        else
        {
            GeneratePathToPosition(lastKnownPlayerPosition);
            FollowPath();
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

        return hit.collider == null || hit.collider.CompareTag("Player");
    }

    private Vector2Int FindNearestWalkableCell(Vector2Int start)
    {
        Queue<Vector2Int> queue = new();
        HashSet<Vector2Int> visited = new();
        queue.Enqueue(start);
        visited.Add(start);

        Vector2Int[] directions = new Vector2Int[]
        {
            new(1,0), new(-1,0), new(0,1), new(0,-1),
            new(1,1), new(1,-1), new(-1,1), new(-1,-1)
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (GridManager.Instance.IsWalkable(current))
                return current;

            foreach (var dir in directions)
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

    private bool IsPathfindingAvailable()
    {
        return GridManager.Instance != null && GridPathfinding.Instance != null;
    }

    private bool HasValidPath()
    {
        return currentPath != null && currentPath.Count > 0;
    }

    private bool IsPathBlocked()
    {
        if (currentIndex >= currentPath.Count) return false;

        Vector3 nextNodePos = GridManager.Instance.GetWorldPosition(currentPath[currentIndex], Vector2Int.one);
        Vector2 toNode = (nextNodePos - transform.position);

        RaycastHit2D wallHit = Physics2D.Raycast(
            transform.position,
            toNode.normalized,
            toNode.magnitude + 0.5f,
            obstacleLayer
        );

        return wallHit.collider != null;
    }

    private void CheckNodeReached()
    {
        Vector3 currentNodePos = GridManager.Instance.GetWorldPosition(currentPath[currentIndex], Vector2Int.one);
        if (Vector2.Distance(transform.position, currentNodePos) < nodeReachDistance)
            currentIndex++;
    }

    private bool IsWallInFront(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direction,
            wallCheckDistance,
            obstacleLayer
        );
        return hit.collider != null;
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
    #endregion

    #region Gizmos
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        if (showPathGizmos && currentPath != null && currentPath.Count > 0)
        {
            Gizmos.color = pathColor;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Vector3 a = GridManager.Instance.GetWorldPosition(currentPath[i], Vector2Int.one);
                Vector3 b = GridManager.Instance.GetWorldPosition(currentPath[i + 1], Vector2Int.one);
                Gizmos.DrawLine(a, b);
                Gizmos.DrawWireSphere(a, 0.1f);
            }
        }

        if (showDetectionRangeGizmo && Application.isPlaying)
        {
            Gizmos.color = detectionRangeColor;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }

        if (showLastKnownPositionGizmo && Application.isPlaying && hasSeenPlayer)
        {
            Gizmos.color = lastKnownPositionColor;
            Gizmos.DrawWireSphere(lastKnownPlayerPosition, 0.3f);
            Gizmos.DrawLine(transform.position, lastKnownPlayerPosition);
        }
    }
    #endregion
}
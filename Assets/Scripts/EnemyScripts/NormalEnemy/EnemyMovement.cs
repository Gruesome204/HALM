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

        currentPath = GridPathfinding.Instance.FindPath(start, goal);

        currentIndex = 0;

        Debug.Log($"Path length: {currentPath.Count}");
    }
    public void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0)
            return;

        if (currentIndex >= currentPath.Count)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector3 targetWorld =
            GridManager.Instance.GetWorldPosition(currentPath[currentIndex], Vector2Int.one);

        Vector2 toTarget = (targetWorld - transform.position);
        float distance = toTarget.magnitude;

        Vector2 dir = toTarget.normalized;

        //Smooth slowing near node
        float speedMultiplier = 1f;
        if (distance < slowDownDistance)
        {
            speedMultiplier = distance / slowDownDistance;
        }

        Vector2 targetVelocity = dir * stats.currentMovementSpeed * speedMultiplier;

        //Smooth acceleration (no instant changes)
        rb.linearVelocity = Vector2.SmoothDamp(
            rb.linearVelocity,
            targetVelocity,
            ref smoothVelocity,
            0.08f
        );

        //Only advance when truly close
        if (distance < arriveDistance)
        {
            currentIndex++;
        }
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
}

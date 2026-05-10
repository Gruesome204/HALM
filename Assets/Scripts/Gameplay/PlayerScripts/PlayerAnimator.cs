using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private PlayerHealth health;

    private Vector3 lastPosition;
    private float moveSpeed;

    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int MoveSpeedParam = Animator.StringToHash("MoveSpeed");
    private static readonly int IsDashing = Animator.StringToHash("IsDashing");
    private static readonly int Death = Animator.StringToHash("Death");

    private PlayerMovement movement;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        movement = GetComponent<PlayerMovement>();
        health = GetComponent<PlayerHealth>();

        if (health != null)
            health.OnDeath += OnDeath;

        lastPosition = transform.position;
        animator.speed = 0.5f;
    }

    private void Update()
    {
        UpdateMovementAnimation();
    }

    private void UpdateMovementAnimation()
    {
        // Determine if player is moving
        Vector3 currentPosition = transform.position;
        bool isMoving = (currentPosition - lastPosition).sqrMagnitude > 0.0001f;
        lastPosition = currentPosition;

        // Update facing direction
        Vector2 facing = movement.FacingDirection;
        animator.SetFloat(MoveX, facing.x);
        animator.SetFloat(MoveY, facing.y);

        // Set MoveSpeedParam as a simple on/off
        animator.SetFloat(MoveSpeedParam, isMoving ? 1f : 0f);
        // Dashing still works as usual
        animator.SetBool(IsDashing, movement.IsDashing);
    }


    private void OnDeath(PlayerHealth _, DamageData __)
    {
        animator.SetTrigger(Death);
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnDeath -= OnDeath;
    }
}

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
    }

    private void Update()
    {
        UpdateMovementAnimation();
    }

    private void UpdateMovementAnimation()
    {
        // Calculate actual speed
        Vector3 currentPosition = transform.position;
        Vector3 delta = currentPosition - lastPosition;
        moveSpeed = delta.magnitude / Time.deltaTime; // units per second
        lastPosition = currentPosition;

        // Update Animator parameters
        Vector2 facing = movement.FacingDirection; // Use your existing facing direction
        animator.SetFloat(MoveX, facing.x);
        animator.SetFloat(MoveY, facing.y);
        animator.SetFloat(MoveSpeedParam, moveSpeed);
        animator.SetBool(IsDashing, movement.IsDashing);

        Debug.Log($"MoveSpeed: {moveSpeed}, Facing: {facing}, IsDashing: {movement.IsDashing}");
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

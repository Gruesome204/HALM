using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private PlayerMovement movement;
    private PlayerHealth health;

    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int IsDashing = Animator.StringToHash("IsDashing");
    private static readonly int Death = Animator.StringToHash("Death");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
        health = GetComponent<PlayerHealth>();

        if (health != null)
            health.OnDeath += OnDeath;
    }

    private void Update()
    {
        UpdateMovementAnimation();
    }

    private void UpdateMovementAnimation()
    {
        Vector2 facing = movement.FacingDirection;

        animator.SetFloat(MoveX, facing.x);
        animator.SetFloat(MoveY, facing.y);

        animator.SetBool(IsMoving, movement.IsMoving);
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

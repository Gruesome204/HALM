using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    private Animator animator;

    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Hit = Animator.StringToHash("Hit");
    private static readonly int Death = Animator.StringToHash("Death");
    private static readonly int Ability = Animator.StringToHash("Ability");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetMoveSpeed(float speed)
    {
        animator.SetFloat(MoveSpeed, speed);
    }

    public void PlayAttack()
    {
        animator.SetTrigger(Attack);
    }

    public void PlayAbility()
    {
        animator.SetTrigger(Ability);
    }

    public void PlayHit()
    {
        animator.SetTrigger(Hit);
    }

    public void PlayDeath()
    {
        animator.SetTrigger(Death);
    }
}

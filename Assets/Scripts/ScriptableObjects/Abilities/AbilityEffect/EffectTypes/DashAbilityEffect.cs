using UnityEngine;

[CreateAssetMenu(menuName = "Game/Abilities/Effects/Dash")]
public class DashAbilityEffect : AbilityEffect
{
    [Header("Dash Settings")]
    public float dashForce = 12f;
    public float dashDuration = 0.15f;

    [Header("Invulnerability")]
    public bool grantIFrames = true;
    public float invulnerabilityDuration = 0.25f;

    public override void Apply(GameObject user, GameObject target)
    {
        if (!user.TryGetComponent<IDashable>(out var dashable))
            return;

        Vector2 direction = GetDashDirection(user);
        if (direction.sqrMagnitude < 0.01f)
            return;

        dashable.Dash(direction, dashForce, dashDuration);

        if (grantIFrames && user.TryGetComponent<IInvulnerable>(out var inv))
        {
            inv.SetInvulnerable(invulnerabilityDuration);
        }
    }

    private Vector2 GetDashDirection(GameObject user)
    {
        // Player → mouse aimed
        if (user.CompareTag("Player"))
        {
            Vector3 mouseWorld =
                Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return (mouseWorld - user.transform.position);
        }

        // Enemy → forward / target-based
        return user.transform.right;
    }
}

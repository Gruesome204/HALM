using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour, IPausable, IDashable
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movementInput;
    private bool canMove = true;
    private bool isPaused;

    public Vector2 FacingDirection { get; private set; } = Vector2.right;

    private bool isDashing;
    private float dashTimer;
    private Vector2 dashDirection;
    private float dashSpeed;

    public bool IsDashing => isDashing;
    public bool IsMoving => movementInput.sqrMagnitude > 0.01f;

    private readonly List<Slow> activeSlows = new();

    private void OnEnable() => GameManager.Instance?.RegisterPausable(this);
    private void OnDisable() => GameManager.Instance?.UnregisterPausable(this);
    public void OnPause()
    {
        isPaused = true;
        rb.linearVelocity = Vector2.zero;
    }

    public void OnResume()
    {
        isPaused = false;
    }


    private class Slow
    {
        public float multiplier;
        public float remainingTime;

        public Slow(float multiplier, float duration)
        {
            this.multiplier = Mathf.Clamp(multiplier, 0f, 1f);
            remainingTime = duration;
        }
    }


    /// <summary>
    /// Apply a slow for a specific duration.
    /// multiplier = 0.5f → half speed, duration in seconds
    /// </summary>
    public void ApplySlow(float multiplier, float duration)
    {
        if (multiplier <= 0f || duration <= 0f) return;
        activeSlows.Add(new Slow(multiplier, duration));
    }

    /// <summary>
    /// Clears all active slows (optional)
    /// </summary>
    public void ClearSlows() => activeSlows.Clear();



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found on this GameObject!");
            enabled = false; // Disable the script if no Rigidbody2D is present
        }
    }

    private void Update()
    {
        if (isPaused || !canMove) return;
        movementInput = new Vector2(
        Input.GetAxisRaw("Horizontal"),
        Input.GetAxisRaw("Vertical")
        ).normalized;

        FacingDirection = GetMouseDirection();
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            rb.MovePosition(
                rb.position + dashDirection * Time.fixedDeltaTime * dashSpeed
            );

            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
                EndDash();

            return;
        }

        if (isPaused) return;

        // Update slows
        float deltaTime = Time.fixedDeltaTime;
        for (int i = activeSlows.Count - 1; i >= 0; i--)
        {
            activeSlows[i].remainingTime -= deltaTime;
            if (activeSlows[i].remainingTime <= 0f)
                activeSlows.RemoveAt(i);
        }

        // Calculate total slow multiplier (multiply all active slows)
        float slowMultiplier = 1f;
        foreach (var s in activeSlows)
            slowMultiplier *= s.multiplier;

        rb.MovePosition(rb.position + movementInput * moveSpeed * slowMultiplier * deltaTime);
    }

    public void EnableMovement(bool enable)
    {
        canMove = enable;
        if (!enable)
            rb.linearVelocity = Vector2.zero;
    }

    private Vector2 GetMouseDirection()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mouseWorldPos - transform.position);
        return direction.normalized;
    }

    public bool CanDash()
    {
        return !isPaused && canMove && !isDashing;
    }

    private void EndDash()
    {
        isDashing = false;
        canMove = true;
        rb.linearVelocity = Vector2.zero;
    }

    public void Dash(Vector2 direction, float force, float duration, float iFrameDuration = -1f)
    {
        if (!CanDash()) return;

        isDashing = true;
        canMove = false;

        dashDirection = direction.normalized;
        dashSpeed = force;
        dashTimer = duration;

        rb.linearVelocity = Vector2.zero;

        // Enable i-frames if PlayerHealth is present
        if (iFrameDuration != 0f)
        {
            PlayerHealth ph = GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.SetInvulnerable(iFrameDuration > 0f ? iFrameDuration : duration);
            }
        }
    }

}
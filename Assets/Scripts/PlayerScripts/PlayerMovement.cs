using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour, IPausable
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    private Rigidbody2D rb;

    private void OnEnable() => GameManager.Instance?.RegisterPausable(this);
    private void OnDisable() => GameManager.Instance?.UnregisterPausable(this);

    private bool isPaused;


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


    private List<Slow> activeSlows = new();

    public void OnPause()
    {
        isPaused = true;
        // Stop moving, stop attacking, etc.
    }

    public void OnResume()
    {
        isPaused = false;
        // Resume AI
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found on this GameObject!");
            enabled = false; // Disable the script if no Rigidbody2D is present
        }
    
    }       

    void FixedUpdate()
    {
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

        // Movement
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector2 movement2D = new Vector2(moveX, moveY).normalized;

        rb.MovePosition(rb.position + movement2D * moveSpeed * slowMultiplier * deltaTime);
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
}
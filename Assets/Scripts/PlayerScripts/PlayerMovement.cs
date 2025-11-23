using UnityEngine;

public class PlayerMovement : MonoBehaviour, IPausable
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    private Rigidbody2D rb;

    private void OnEnable() => GameManager.Instance?.RegisterPausable(this);
    private void OnDisable() => GameManager.Instance?.UnregisterPausable(this);

    private bool isPaused;

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
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical"); // You might only need Horizontal for 2D movement

        Vector2 movement2D = new Vector2(moveX, moveY).normalized; // Create a 2D vector
        rb.MovePosition(rb.position + movement2D * moveSpeed * Time.fixedDeltaTime);
    }
}
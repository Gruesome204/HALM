using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;

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
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical"); // You might only need Horizontal for 2D movement

        Vector2 movement2D = new Vector2(moveX, moveY).normalized; // Create a 2D vector
        rb.linearVelocity = movement2D * moveSpeed; // Apply velocity to the Rigidbody2D
    }
}
// PlayerMovement.cs
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;  

    void Start()
    {
            rb = GetComponent<Rigidbody>();  
    }

    void FixedUpdate()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector3 movement3D = new Vector3(moveX, 0f, moveY).normalized; 
        rb.linearVelocity = movement3D * moveSpeed;
        
    }
}
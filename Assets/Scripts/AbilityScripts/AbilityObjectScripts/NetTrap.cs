using UnityEngine;

public class NetTrap : MonoBehaviour
{
    [Range(0f, 1f)]
    public float slowMultiplier = 0.5f; // movement speed multiplier
    public float slowDuration = 3f;     // duration of the slow effect

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            player.ApplySlow(slowMultiplier, slowDuration);
        }
    }
}

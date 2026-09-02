using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class NetTrap : MonoBehaviour
{
    [Header("Slow Settings")]
    [SerializeField] private float slowMultiplier = 0.5f;
    [SerializeField] private float slowDuration = 3f;
    [SerializeField] private float radius = 1f;

    private Collider2D trapCollider;
    private bool isActive = true;

    // Track affected players to avoid duplicate slows
    private readonly HashSet<GameObject> affectedTargets = new HashSet<GameObject>();

    private void Awake()
    {
        trapCollider = GetComponent<Collider2D>();
        if (trapCollider != null)
        {
            trapCollider.isTrigger = true;
        }

        // Auto-configure circle collider if present
        if (TryGetComponent<CircleCollider2D>(out CircleCollider2D circle))
        {
            circle.radius = radius;
        }
    }

    public void Initialize(float multiplier, float duration, float trapRadius)
    {
        slowMultiplier = multiplier;
        slowDuration = duration;
        radius = trapRadius;

        if (TryGetComponent<CircleCollider2D>(out CircleCollider2D circle))
        {
            circle.radius = radius;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        if (affectedTargets.Contains(other.gameObject)) return;

        // Try to apply slow to player
        if (TryApplySlow(other.gameObject))
        {
            affectedTargets.Add(other.gameObject);
            Debug.Log($"Applied slow to {other.gameObject.name}: multiplier={slowMultiplier}, duration={slowDuration}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (affectedTargets.Contains(other.gameObject))
        {
            // Remove slow when exiting
            RemoveSlow(other.gameObject);
            affectedTargets.Remove(other.gameObject);
        }
    }

    private bool TryApplySlow(GameObject target)
    {
        // Check for PlayerMovement component
        PlayerMovement playerMovement = target.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.ApplySlow(slowMultiplier, slowDuration);
            return true;
        }

        // Fallback: Check for PlayerMovement on parent
        PlayerMovement parentMovement = target.GetComponentInParent<PlayerMovement>();
        if (parentMovement != null)
        {
            parentMovement.ApplySlow(slowMultiplier, slowDuration);
            return true;
        }

        Debug.LogWarning($"No PlayerMovement component found on {target.name}");
        return false;
    }

    private void RemoveSlow(GameObject target)
    {
        // For now, we'll just note that the player left the net
        Debug.Log($"Player {target.name} left the net area");

        PlayerMovement playerMovement = target.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.ClearSlows(); // This would clear ALL slows immediately
        }
    }

    public void Deactivate()
    {
        isActive = false;
        // Optionally clear slows from all affected targets when net is destroyed
        foreach (var target in affectedTargets)
        {
            PlayerMovement playerMovement = target.GetComponent<PlayerMovement>();
            if (target != null)
            {
                playerMovement.ClearSlows();
            }
        }
        affectedTargets.Clear();
    }

    private void OnDestroy()
    {
        Deactivate();
    }

    // Visual feedback - show the net's radius in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
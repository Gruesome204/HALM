using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))] // Or Collider for 3D
public class HubInteractable : MonoBehaviour
{
    [Header("Interaction Settings")]
    public string interactionName = "Unnamed Hub Object";
    public UnityEvent onInteract; // Assign in inspector

    private void OnMouseDown()
    {
        // For mouse clicks in 2D/3D
        Interact();
    }

    public void Interact()
    {
        Debug.Log($"Interacted with {interactionName}");
        onInteract?.Invoke(); // Triggers whatever is linked in Inspector
    }
}
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance = 5f; // Max distance for clicking
    public LayerMask interactableLayer; // Only interactable objects

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // 2D raycast at mouse position
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f, interactableLayer);
            if (hit.collider != null)
            {
                if (hit.collider.TryGetComponent<IInteractable>(out IInteractable interactable))
                {
                    interactable.Interact();
                }
            }
        }
    }
}

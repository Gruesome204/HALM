using UnityEngine;

public class TurretDemolitionController : MonoBehaviour
{
    public static TurretDemolitionController Instance { get; private set; }

    public LayerMask turretLayer; // Assign the turret layer in the Inspector
    public KeyCode toggleDestructionKey = KeyCode.X;
    private bool destructionModeActive = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        // Toggle Destruction Mode with X
        if (Input.GetKeyDown(toggleDestructionKey))
        {
            ToggleDestructionMode();
        }

        // Cancel with Right Click
        if (destructionModeActive && Input.GetMouseButtonDown(1))
        {
            DeactivateDestructionMode();
        }

        // If active, handle turret destruction
        if (destructionModeActive && Input.GetMouseButtonDown(0))
        {
            TryDestroyTurret();
        }
    }
    private void ToggleDestructionMode()
    {
        destructionModeActive = !destructionModeActive;
        Debug.Log("Destruction Mode: " + (destructionModeActive ? "Activated" : "Deactivated"));

        // Optional: Change cursor or UI indicator here
    }

    private void DeactivateDestructionMode()
    {
        destructionModeActive = false;
        Debug.Log("Destruction Mode: Deactivated");
        // Optional: Reset cursor/UI indicator
    }

    private void TryDestroyTurret()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero, Mathf.Infinity, turretLayer);

        Debug.Log(hit);
        if (hit.collider != null)
        {
            GameObject turret = hit.collider.transform.root.gameObject;
            // Remove from GridManager
            PlacableObject placableObject = turret.GetComponent<PlacableObject>();
            if (placableObject != null)
            {
                GridManager.Instance.RemoveObject(placableObject.gameObject, placableObject.currentGridCoordinates, placableObject.sizeInCells);
            }

            // Remove from InstantiatedTurretList
            TurretBehaviour turretBehaviour = turret.GetComponent<TurretBehaviour>();
            if (turretBehaviour != null && turretBehaviour.turretBlueprint != null)
            {
                TurretPlacementController.Instance.GetActiveTurrets().Remove(gameObject);
            }

            Destroy(turret);
            Debug.Log("Turret destroyed!");

            // Optional: Deactivate destruction mode after one destroy
            // DeactivateDestructionMode();
        }
        else
        {
            Debug.Log("No turret selected for destruction.");
        }
    }

    public bool IsDestructionModeActive()
    {
        return destructionModeActive;
    }
}

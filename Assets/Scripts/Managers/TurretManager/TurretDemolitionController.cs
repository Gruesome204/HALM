using UnityEngine;
using System;
using UnityEngine.UIElements;

/// <summary>
/// Handles demolition (removal) of turrets by the player.
/// Toggles demolition mode, listens for input, and destroys selected turrets.
/// </summary>
public class TurretDemolitionController : MonoBehaviour
{
    public static TurretDemolitionController Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("Layer mask for turret objects.")]
    public LayerMask turretLayer; // Assign the turret layer in the Inspector

    [Tooltip("Key used to toggle demolition mode.")]
    public KeyCode toggleDestructionKey = KeyCode.X;

    private bool destructionModeActive = false;

    public event Action OnDemolitionModeChange;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple TurretDemolitionControllers detected. Destroying the duplicate.");
            Destroy(gameObject);
        }
    }
    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // Toggle destruction mode with hotkey
        if (Input.GetKeyDown(toggleDestructionKey))
        {
            ToggleDestructionMode();
        }

        // Cancel with Right Click
        if (destructionModeActive && Input.GetMouseButtonDown(1))
        {
            DeactivateDestructionMode();
        }

        // Destroy turret with Left Click
        if (destructionModeActive && Input.GetMouseButtonDown(0))
        {
            TryDestroyTurret();
        }
    }
    private void ToggleDestructionMode()
    {
        destructionModeActive = !destructionModeActive;
        Debug.Log("Destruction Mode: " + (destructionModeActive ? "Activated" : "Deactivated"));

        if (destructionModeActive && TurretPlacementController.Instance != null)
        {
            // Remove preview and deselect blueprint
            TurretPlacementController.Instance.DeselectTurretBlueprint();
        }
        // Optional: Change cursor or UI indicator here

        //Sending out Event on mode Change so the UI can change accordingly
        OnDemolitionModeChange?.Invoke();
    }

    private void DeactivateDestructionMode()
    {
        destructionModeActive = false;
        Debug.Log("Destruction Mode: Deactivated");
        OnDemolitionModeChange?.Invoke();
        // Optional: Reset cursor/UI indicator
    }

    public void ForceDeactivateDestructionMode()
    {
        destructionModeActive = false;

        Debug.Log("Destruction Mode: Forced Deactivation");

        if (TurretPlacementController.Instance != null)
        {
            TurretPlacementController.Instance.DestroyPreview();
        }
        // Optional: reset cursor/UI indicator

        //Sending out Event on mode Change so the UI can change accordingly
        OnDemolitionModeChange?.Invoke();
    }

    public bool IsDestructionModeActive() => destructionModeActive;



    private void TryDestroyTurret()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero, Mathf.Infinity, turretLayer);

        if (hit.collider == null)
        {
            Debug.Log("[TurretDemolition] No turret selected for destruction.");
            return;
        }

        GameObject turret = hit.collider.gameObject;

        var placable = turret.GetComponent<PlacableObject>();
        var behaviour = turret.GetComponent<TurretBehaviour>();

        //Removes the tower from the grid 
        if (placable != null && behaviour != null)
        {
            GridManager.Instance.RemoveObject(
                turret.transform.parent.gameObject,
                placable.currentGridCoordinates,
                behaviour.turretBlueprint.sizeInCells);
            Debug.Log("Remove turret from grid");
        }

        // Remove from TurretPlacementController list
        TurretPlacementController.Instance.RemoveTurret(turret.transform.parent.gameObject);
        Destroy(turret);
        Debug.Log($"[TurretDemolition] Turret:{turret.name} destroyed!");
    }


}

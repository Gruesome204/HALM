using System.Collections.Generic;
using UnityEngine;

public class TurretPlacementController : MonoBehaviour
{
    public List<TurretBlueprint> turretBlueprintList = new List<TurretBlueprint>();
    public TurretBlueprint currentBlueprint;

    public LayerMask groundLayer; // Assign the layer of your ground in the Inspector
    public LayerMask turretLayer;

    public GameObject previewObject; // Optional: Prefab for placement preview
    private PlacableObject previewPlacableObject;

    //Instantiated turretList
    [SerializeField]private HashSet<TurretBlueprint> InstantiatedTurretList = new HashSet<TurretBlueprint>();

    [Header("Placement Cooldown")] // Organize variables in the Inspector
    public float placementCooldown = 1.0f; // Cooldown duration in seconds
    private float lastPlacementTime; // Time when the last turret was placed

    public static TurretPlacementController Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple TurretPlacementManagers found in the scene! Destroying the newest one.");
            Destroy(gameObject);
            return;
        }
    }

    public HashSet<TurretBlueprint> GetInstantiatedTurretList()
    {
        return InstantiatedTurretList;
    }
    public List<TurretBlueprint> GetTurretBlueprintList()
    {
        return turretBlueprintList;
    }

    void Update()
    {
        // --- Input for selecting turret blueprint ---
        if (Input.GetKeyDown(KeyCode.Alpha1) && turretBlueprintList.Count > 0)
        {
            SelectTurretBlueprint(turretBlueprintList[0]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && turretBlueprintList.Count > 1)
        {
            SelectTurretBlueprint(turretBlueprintList[1]);
        }
        // Add more keybinds or UI selection as needed

        // --- Handle Placement Preview ---
        if (currentBlueprint != null)
        {
            HandlePlacementPreview();
        }
        else
        {
            // If no blueprint is selected, ensure preview is destroyed
            if (previewObject != null)
            {
                Destroy(previewObject);
                previewObject = null;
                previewPlacableObject = null;
            }
        }

        // --- Handle Actual Placement on Mouse Click ---
        if (Input.GetMouseButtonDown(0) && currentBlueprint != null) // Left mouse button click
        {
            TryPlaceTurret();
        }

        // --- Handle canceling placement (e.g., right click) ---
        if (Input.GetMouseButtonDown(1)) // Right click
        {
            DeselectTurretBlueprint();
        }
    }

    // Call this method when a new turret blueprint is selected (e.g., from UI button)
    public void SelectTurretBlueprint(TurretBlueprint blueprint)
    {
        if (currentBlueprint == blueprint) return; // Already selected this one

        currentBlueprint = blueprint;
        Debug.Log("Selected Blueprint: " + (currentBlueprint != null ? currentBlueprint.name : "None"));

        // Immediately create or update the preview object when a blueprint is selected
        CreateOrUpdatePreviewObject();
    }

    public void DeselectTurretBlueprint()
    {
        currentBlueprint = null;
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
            previewPlacableObject = null;
        }
        Debug.Log("Placement canceled.");
    }

    private void CreateOrUpdatePreviewObject()
    {
        if (previewObject != null)
        {
            Destroy(previewObject); // Destroy old preview if it exists
        }

        // Instantiate the actual turret prefab for the preview to ensure correct size/model
        if (currentBlueprint != null && currentBlueprint.turretPrefab != null)
        {
            previewObject = Instantiate(currentBlueprint.turretPrefab); // Use the actual turret prefab
            previewPlacableObject = previewObject.GetComponent<PlacableObject>();

            if (previewPlacableObject == null)
            {
                Debug.LogError("Turret Prefab '" + currentBlueprint.turretPrefab.name + "' is missing a PlacableObject component!");
                Destroy(previewObject);
                previewObject = null;
                currentBlueprint = null; // Deselect to prevent further errors
                return;
            }

            // Make the preview transparent or change its material for visual feedback
            Renderer renderer = previewObject.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                // To make it transparent: change rendering mode to Fade
                // Save original material/shader if you want to restore it.
                // For simplicity, just assign a transparent material or change color.
                renderer.material = new Material(Shader.Find("Standard")); // Or a custom transparent shader
                renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, 0.5f); // 50% opacity
                renderer.material.SetFloat("_Mode", 2); // Set rendering mode to 'Fade' for transparency (Standard shader)
                renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                renderer.material.SetInt("_ZWrite", 0);
                renderer.material.DisableKeyword("_ALPHATEST_ON");
                renderer.material.EnableKeyword("_ALPHABLEND_ON");
                renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                renderer.material.renderQueue = 3000;
            }
        }
    }

    private void HandlePlacementPreview()
    {
        // Raycast to get world position under mouse
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, Mathf.Infinity, groundLayer);

        if (hit.collider != null)
        {
            Vector2Int gridCoords = GridManager.Instance.GetGridCoordinates(hit.point);
            Vector3 snappedWorldPos = GridManager.Instance.GetWorldPosition(gridCoords);

            // Ensure preview object exists and has the PlacableObject component
            if (previewObject == null || previewPlacableObject == null)
            {
                CreateOrUpdatePreviewObject(); // Recreate if somehow lost
                if (previewObject == null) return; // Still null, something went wrong
            }

            previewObject.transform.position = snappedWorldPos;

            bool canPlace = GridManager.Instance.CanPlaceObject(gridCoords, previewPlacableObject.sizeInCells);

            // Update preview object's color based on validity
            Renderer renderer = previewObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Preserve transparency
                Color targetColor = canPlace ? Color.green : Color.red;
                targetColor.a = 0.5f; // Keep it semi-transparent
                renderer.material.color = targetColor;
            }
        }
        else
        {
            // Mouse is not over the ground, hide/destroy preview
            if (previewObject != null)
            {
                previewObject.SetActive(false); // Or Destroy(previewObject);
            }
        }
    }

    private void TryPlaceTurret()
    {
        if (Time.time < lastPlacementTime + placementCooldown)
        {
            Debug.Log("Turret placement on cooldown. Remaining: " + (lastPlacementTime + placementCooldown - Time.time).ToString("F2") + " seconds.");
            return;
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, Mathf.Infinity, groundLayer);

        if (hit.collider != null)
        {
            Vector2Int gridCoords = GridManager.Instance.GetGridCoordinates(hit.point);

            // Use the size from the selected blueprint's prefab for actual placement
            PlacableObject blueprintPlacableObject = currentBlueprint.turretPrefab.GetComponent<PlacableObject>();
            if (blueprintPlacableObject == null)
            {
                Debug.LogError("Turret Prefab '" + currentBlueprint.turretPrefab.name + "' is missing a PlacableObject component!");
                return;
            }

            if (GridManager.Instance.CanPlaceObject(gridCoords, blueprintPlacableObject.sizeInCells))
            {
                GameObject newTurret = Instantiate(currentBlueprint.turretPrefab);
               
                PlacableObject placedPlacableObject = newTurret.GetComponent<PlacableObject>();
                placedPlacableObject.currentGridCoordinates = gridCoords;

                GridManager.Instance.PlaceObject(newTurret, gridCoords, placedPlacableObject.sizeInCells);

                // Destroy the active preview object
                if (previewObject != null)
                {
                    Destroy(previewObject);
                    previewObject = null;
                    previewPlacableObject = null;
                }

                // Initialize TurretAttack component
                TurretAttack turretAttack = newTurret.GetComponent<TurretAttack>();
                if (turretAttack != null)
                {
                    turretAttack.turretBlueprint = currentBlueprint;
                    turretAttack.InitializeFromBlueprint(); // Optional initialization method
                    InstantiatedTurretList.Add(currentBlueprint); // Add blueprint to tracking list
                }

                lastPlacementTime = Time.time;
                Debug.Log("Turret placed! Cooldown started.");
                // Optionally deselect the blueprint after placing
                // DeselectTurretBlueprint();
            }
            else
            {
                Debug.Log("Cannot place turret here: grid occupied or out of bounds.");
            }
        }
        else
        {
            Debug.Log("Cannot place turret: mouse not over ground.");
        }
    }
}
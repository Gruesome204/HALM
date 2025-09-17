using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Handles turret blueprint selection, placement, previewing, and active turret management.
/// </summary>
public class TurretPlacementController : MonoBehaviour
{
    public static TurretPlacementController Instance { get; private set; }

    [Header("Turret Blueprints")]
    [Tooltip("List of available turret blueprints.")]
    public List<TurretBlueprint> turretBlueprintList = new List<TurretBlueprint>();

    [Tooltip("Currently selected turret blueprint.")]
    public TurretBlueprint currentSelectedBlueprint;

    [Header("Layers")]
    [Tooltip("Layer for ground placement.")]
    public LayerMask groundLayer; // Assign the layer of your ground in the Inspector
    [Tooltip("Layer for turrets.")]
    public LayerMask turretLayer;

    [Header("Placement Preview")]
    [Tooltip("Prefab used to show placement preview (optional).")]
    public GameObject previewObject; // Optional: Prefab for placement preview
    private PlacableObject previewPlacableObject;

    [Header("Placement Rules")]
    [Tooltip("Maximum number of turrets allowed.")]
    public int maxTurretNumber = 4;

    [Tooltip("Cooldown time between placements (seconds).")]
    public float placementCooldown = 1.0f;

    [Header("Hierarchy Organization")]
    [SerializeField] private Transform turretContainer;

    //Currently Active Turrets
    private List<GameObject> activeTurrets = new List<GameObject>();
    private float lastPlacementTime; // Time when the last turret was placed


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("[TurretPlacement] Duplicate instance detected. Destroying this object.");
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        HandleBlueprintSelectionInput();
        HandlePlacementInput();
    }
    private void HandleBlueprintSelectionInput()
    {
        // Example: hotkeys 1 and 2 for selecting blueprints
        if (Input.GetKeyDown(KeyCode.Alpha1) && turretBlueprintList.Count > 0)
            SelectTurretBlueprint(turretBlueprintList[0]);

        if (Input.GetKeyDown(KeyCode.Alpha2) && turretBlueprintList.Count > 1)
            SelectTurretBlueprint(turretBlueprintList[1]);
    }
    private void HandlePlacementInput()
    {
        // Skip placement if in demolition mode
        if (TurretDemolitionController.Instance != null && TurretDemolitionController.Instance.IsDestructionModeActive())
        {
            if (currentSelectedBlueprint != null)
                DeselectTurretBlueprint();

            return;
        }

        // Left click = place turret
        if (Input.GetMouseButtonDown(0) && currentSelectedBlueprint != null)
        {
            TryPlaceTurret();
        }

        // Right click = cancel placement
        if (Input.GetMouseButtonDown(1))
        {
            DeselectTurretBlueprint();
        }

        //// Handle preview update
        //if (currentSelectedBlueprint != null)
        //{
        //    //HandlePlacementPreview();
        //}
        //else if (previewObject != null)
        //{
        //    DestroyPreview();
        //}
    }



    // Call this method when a new turret blueprint is selected (e.g., from UI button)
    public void SelectTurretBlueprint(TurretBlueprint blueprint)
    {

        if (currentSelectedBlueprint == blueprint) return; // Already selected this one

        // Cancel demolition mode if active
        if (TurretDemolitionController.Instance != null && TurretDemolitionController.Instance.IsDestructionModeActive())
        {
            TurretDemolitionController.Instance.ForceDeactivateDestructionMode();
        }
        currentSelectedBlueprint = blueprint;
        Debug.Log("Selected Blueprint: " + (currentSelectedBlueprint != null ? currentSelectedBlueprint.name : "None"));

        //// Immediately create or update the preview object when a blueprint is selected
        //CreateOrUpdatePreviewObject();
    }

    public void DeselectTurretBlueprint()
    {
        currentSelectedBlueprint = null;
        //DestroyPreview();
        Debug.Log("[TurretPlacement] Placement canceled.");
    }


    //private void CreateOrUpdatePreviewObject()
    //{
    //    DestroyPreview();

    //    if (currentSelectedBlueprint?.turretPrefab == null) return;

    //    previewObject = Instantiate(currentSelectedBlueprint.turretPrefab);
    //    previewPlacableObject = previewObject.GetComponent<PlacableObject>();

    //    if (previewPlacableObject == null)
    //    {
    //        Debug.LogError($"[TurretPlacement] Prefab '{currentSelectedBlueprint.turretPrefab.name}' is missing PlacableObject!");
    //        DestroyPreview();
    //        currentSelectedBlueprint = null;
    //        return;
    //    }

    //    MakePreviewTransparent(previewObject);
    //}

    //private void HandlePlacementPreview()
    //{
    //    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //    RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, Mathf.Infinity, groundLayer);

    //    if (hit.collider == null)
    //    {
    //        if (previewObject != null) previewObject.SetActive(false);
    //        return;
    //    }

    //    Vector2Int gridCoords = GridManager.Instance.GetGridCoordinates(hit.point);
    //    Vector3 snappedWorldPos = GridManager.Instance.GetWorldPosition(gridCoords);

    //    previewObject.transform.position = snappedWorldPos;

    //    bool canPlace = GridManager.Instance.CanPlaceObject(gridCoords, previewPlacableObject.sizeInCells);

    //    UpdatePreviewColor(canPlace);
    //}

    //private void MakePreviewTransparent(GameObject obj)
    //{
    //    Renderer renderer = obj.GetComponent<Renderer>();
    //    if (renderer == null) return;

    //    renderer.material = new Material(Shader.Find("Standard"));
    //    renderer.material.color = new Color(1, 1, 1, 0.5f); // Semi-transparent
    //    renderer.material.SetFloat("_Mode", 2); // Fade
    //    renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
    //    renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
    //    renderer.material.SetInt("_ZWrite", 0);
    //    renderer.material.DisableKeyword("_ALPHATEST_ON");
    //    renderer.material.EnableKeyword("_ALPHABLEND_ON");
    //    renderer.material.renderQueue = 3000;
    //}

    //private void UpdatePreviewColor(bool canPlace)
    //{
    //    Renderer renderer = previewObject?.GetComponent<Renderer>();
    //    if (renderer == null) return;

    //    Color targetColor = canPlace ? Color.green : Color.red;
    //    targetColor.a = 0.5f;
    //    renderer.material.color = targetColor;
    //}

    //private void DestroyPreview()
    //{
    //    if (previewObject != null)
    //    {
    //        Destroy(previewObject);
    //        previewObject = null;
    //        previewPlacableObject = null;
    //    }
    //}

    public void RemoveTurret(GameObject turret)
    {
        if (!activeTurrets.Contains(turret)) return;

        activeTurrets.Remove(turret);
        Destroy(turret);

        Debug.Log($"[TurretPlacement] Turret removed");
    }

    private void TryPlaceTurret()
    {
        if (GetActiveTurrets().Count >= maxTurretNumber)
        {
            Debug.Log($"[TurretPlacement] Max turret limit reached ({maxTurretNumber}).");
            return;
        }

        if (Time.time < lastPlacementTime + placementCooldown)
        {
            float remaining = lastPlacementTime + placementCooldown - Time.time;
            Debug.Log($"[TurretPlacement] Placement on cooldown. {remaining:F2}s left.");
            return;
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, Mathf.Infinity, groundLayer);

        if (hit.collider == null)
        {
            Debug.Log("[TurretPlacement] Cannot place: mouse not over ground.");
            return;
        }

        Vector2Int gridCoords = GridManager.Instance.GetGridCoordinates(hit.point);
        PlacableObject blueprintPlacable = currentSelectedBlueprint.turretPrefab.GetComponent<PlacableObject>();

        if (blueprintPlacable == null)
        {
            Debug.LogError($"[TurretPlacement] Prefab '{currentSelectedBlueprint.turretPrefab.name}' missing PlacableObject!");
            return;
        }

        if (!GridManager.Instance.CanPlaceObject(gridCoords, blueprintPlacable.sizeInCells))
        {
            Debug.Log("[TurretPlacement] Cannot place: grid occupied or out of bounds.");
            return;
        }

        // Instantiate and register turret
        GameObject newTurret = Instantiate(
            currentSelectedBlueprint.turretPrefab,
            GridManager.Instance.GetWorldPosition(gridCoords),
            Quaternion.identity,
            turretContainer
        );

        TurretBehaviour behaviour = newTurret.GetComponent<TurretBehaviour>();
        if (behaviour != null)
        {
            behaviour.turretBlueprint = currentSelectedBlueprint;
            behaviour.InitializeFromBlueprint();
        }

        activeTurrets.Add(newTurret);
        lastPlacementTime = Time.time;

        //DestroyPreview();
        Debug.Log("[TurretPlacement] Turret placed.");
    }

    public List<GameObject> GetActiveTurrets() => activeTurrets;
    public List<TurretBlueprint> GetTurretBlueprintList() => turretBlueprintList;
}
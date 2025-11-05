using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// Handles turret blueprint selection, placement, previewing, and active turret management.
/// </summary>
public class TurretPlacementController : MonoBehaviour
{
    public static TurretPlacementController Instance { get; private set; }

    public event Action OnTurretsChanged;

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
    [Tooltip("Maximum turret capacity (not just number).")]
    public int maxTurretCapacity = 6;// e.g. total "points" you can spend

    [Header("Hierarchy Organization")]
    [SerializeField] private Transform turretContainer;

    //Currently Active Turrets
    [SerializeField]private List<GameObject> activeTurrets = new List<GameObject>();
     [SerializeField]private List<TurretHealth> placedTurrets = new List<TurretHealth>();
    private Dictionary<TurretBlueprint, float> lastPlacementTimes = new Dictionary<TurretBlueprint, float>();


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

    public void RegisterTurret(TurretHealth turret)
    {
        if (!placedTurrets.Contains(turret))
            placedTurrets.Add(turret);
    }

    public void UnregisterTurret(TurretHealth turret)
    {
        placedTurrets.Remove(turret);
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

        // Handle preview update
        if (currentSelectedBlueprint != null)
        {
            HandlePlacementPreview();
        }
        else if (previewObject != null)
        {
            DestroyPreview();
        }
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
        CreateOrUpdatePreviewObject();
    }

    public void DeselectTurretBlueprint()
    {
        currentSelectedBlueprint = null;
        DestroyPreview();
        Debug.Log("[TurretPlacement] Placement canceled.");
    }


    private void CreateOrUpdatePreviewObject()
    {
        DestroyPreview();

        if (currentSelectedBlueprint?.previewPrefab == null) return;

        previewObject = Instantiate(currentSelectedBlueprint.previewPrefab);
        MakePreviewTransparent(previewObject);

        // Optional: scale preview if needed (e.g., sprite size)
        previewObject.transform.localScale = new Vector3(
            currentSelectedBlueprint.sizeInCells.x,
            currentSelectedBlueprint.sizeInCells.y,
            1f
        );
    }

    private void HandlePlacementPreview()
    {
        if (previewObject == null || currentSelectedBlueprint == null) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, Mathf.Infinity, groundLayer);

        if (hit.collider == null)
        {
            previewObject.SetActive(false);
            return;
        }

        previewObject.SetActive(true);

        // Snap to grid using blueprint size
        Vector2Int gridCoords = GridManager.Instance.GetGridCoordinates(hit.point);
        Vector3 snappedWorldPos = GridManager.Instance.GetWorldPosition(gridCoords, currentSelectedBlueprint.sizeInCells);

        previewObject.transform.position = snappedWorldPos;

        // Check if placement is valid
        bool canPlace = GridManager.Instance.CanPlaceObject(gridCoords, currentSelectedBlueprint.sizeInCells);
        UpdatePreviewColor(canPlace);
    }
    private void MakePreviewTransparent(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null) return;

        // Use sprite-friendly material
        Material previewMat = new Material(Shader.Find("Sprites/Default"));
        previewMat.color = new Color(1f, 1f, 1f, 0.5f); // Semi-transparent

        // Ensure it draws on top of everything
        previewMat.renderQueue = 4000;

        renderer.material = previewMat;
    }

    private void UpdatePreviewColor(bool canPlace)
    {
        Renderer renderer = previewObject?.GetComponent<Renderer>();
        if (renderer == null) return;

        Color targetColor = canPlace ? Color.green : Color.red;
        targetColor.a = 0.5f;
        renderer.material.color = targetColor;
    }

    public void DestroyPreview()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
            previewPlacableObject = null;
        }
    }

    public void RemoveTurret(GameObject turret)
    {
        if (!activeTurrets.Contains(turret)) return;

        activeTurrets.Remove(turret);
        OnTurretsChanged?.Invoke();
        Destroy(turret);

        Debug.Log($"[TurretPlacement] Turret removed");
    }

    private void TryPlaceTurret()
    {
        int currentCapacity = GetUsedCapacity();
        int cost = currentSelectedBlueprint.buildCapacityValue;

        if (currentCapacity + cost > maxTurretCapacity)
        {
            Debug.Log($"[TurretPlacement] Not enough capacity. Current: {currentCapacity}/{maxTurretCapacity}, Cost: {cost}");
            return;
        }

        float lastTime;
        lastPlacementTimes.TryGetValue(currentSelectedBlueprint, out lastTime);

        // Only check cooldown if the turret has been placed before
        if (lastPlacementTimes.ContainsKey(currentSelectedBlueprint) && Time.time < lastTime + currentSelectedBlueprint.placementCooldown)
        {
            float remaining = lastTime + currentSelectedBlueprint.placementCooldown - Time.time;
            Debug.Log($"[TurretPlacement] {currentSelectedBlueprint.name} on cooldown. {remaining:F2}s left.");
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

        if (!GridManager.Instance.CanPlaceObject(gridCoords, currentSelectedBlueprint.sizeInCells))
        {
            Debug.Log("[TurretPlacement] Cannot place: grid occupied or out of bounds.");
            return;
        }

        // Instantiate turret at centered position
        GameObject newTurret = Instantiate(
            currentSelectedBlueprint.turretPrefab,
            GridManager.Instance.GetWorldPosition(gridCoords, currentSelectedBlueprint.sizeInCells),
            Quaternion.identity,
            turretContainer
        );

        // Register turret in grid
        PlacableObject turretPlacable = newTurret.GetComponentInChildren<PlacableObject>();
        if (turretPlacable != null)
        {
            GridManager.Instance.PlaceObject(newTurret, gridCoords, currentSelectedBlueprint.sizeInCells);
        }
        TurretBehaviour behaviour = newTurret.GetComponentInChildren<TurretBehaviour>();
        if (behaviour != null)
        {
            behaviour.turretBlueprint = currentSelectedBlueprint;
            behaviour.InitializeFromBlueprint();
        }

        // Setup TurretHealth
        TurretHealth turretHealth = newTurret.GetComponentInChildren<TurretHealth>();
        if (turretHealth != null)
        {
            // Assign stats from blueprint (clone if ScriptableObject)
            if (currentSelectedBlueprint != null)
                turretHealth.Initialize(currentSelectedBlueprint);
                
            // Register turret for tracking and cleanup
            turretHealth.OnDeath += OnTurretDeath;
            RegisterTurret(turretHealth);
        }
        activeTurrets.Add(newTurret);
        OnTurretsChanged?.Invoke();
        lastPlacementTimes[currentSelectedBlueprint] = Time.time;

        DeselectTurretBlueprint();
        //DestroyPreview();
        Debug.Log($"[TurretPlacement] Turret placed. Used Capacity: {GetUsedCapacity()}/{maxTurretCapacity}");
    }
    private void OnTurretDeath(TurretHealth turret, DamageData data)
    {
        UnregisterTurret(turret);

        if (turret != null && activeTurrets.Contains(turret.gameObject))
        activeTurrets.Remove(turret.gameObject);

        OnTurretsChanged?.Invoke();
    }
    public int GetUsedCapacity()
    {
        int total = 0;
        foreach (var turret in activeTurrets)
        {
            var behaviour = turret.GetComponentInChildren<TurretBehaviour>();
            if (behaviour != null && behaviour.turretBlueprint != null)
            {
                total += behaviour.turretBlueprint.buildCapacityValue;
            }
        }
        return total;
    }

    public List<GameObject> GetActiveTurrets() => activeTurrets;
    public List<TurretBlueprint> GetTurretBlueprintList() => turretBlueprintList;


    public void SetupFromGameData(GameDataSO gameData)
    {
        turretBlueprintList = gameData.allTurretBlueprints
            .Where(b => gameData.unlockedTurrets.Contains(b.turretType))
            .ToList();

        Debug.Log($"[TurretPlacement] Loaded {turretBlueprintList.Count} unlocked turrets from GameDataSO.");
        OnTurretsChanged?.Invoke();
    }
}
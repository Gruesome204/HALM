using System;
using System.Collections;
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
    public event Action<TurretBlueprint, bool> OnPlacementCooldownStateChanged;

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

    [Header("Blocking Layers")]
    [Tooltip("Layer for player characters (placement blocked).")]
    public LayerMask playerLayer;

    [Tooltip("Layer for enemies (placement blocked).")]
    public LayerMask enemyLayer;

    [Tooltip("Layer for walls (placement blocked).")]
    public LayerMask wallLayer;

    [Header("Placement Preview")]
    [Tooltip("Prefab used to show placement preview (optional).")]
    public GameObject previewObject; // Optional: Prefab for placement preview
    private PlacableObject previewPlacableObject;

    [Header("Placement Rules")]
    [Tooltip("Maximum turret capacity (not just number).")]
    public int maxTurretCapacity = 6;// e.g. total "points" you can spend

    [Header("Hierarchy Organization")]
    [SerializeField] private Transform turretContainer;

    [Header("Placement Range")]
    [Tooltip("Maximum distance from the player where turrets can be placed.")]
    public float placementRadius = 30f;
    [Header("Placement Radius Visual (LineRenderer)")]
    [SerializeField] private Material radiusLineMaterial;
    [SerializeField] private float radiusLineWidth = 0.05f;
    [SerializeField] private int radiusSegments = 64;

    private LineRenderer radiusLineRenderer;

    [Tooltip("Reference to the player transform.")]
    public Transform playerTransform;

    // Active turret tracking
    private List<GameObject> activeTurrets = new();
    private List<TurretHealth> placedTurrets = new();
    private Dictionary<TurretBlueprint, float> cooldownEndTimes = new();


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

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void OnEnable() => OnPlacementCooldownStateChanged += HandleCooldownEvent;
    private void OnDisable() => OnPlacementCooldownStateChanged -= HandleCooldownEvent;


    public void SetupFromGameData(GameDataSO gameData)
    {
        turretBlueprintList = new List<TurretBlueprint>(gameData.GetUnlockedBlueprints());
        Debug.Log($"[TurretPlacement] Loaded {turretBlueprintList.Count} unlocked turrets from GameDataSO.");
        OnTurretsChanged?.Invoke();
    }

    private void Update()
    {
        HandleBlueprintSelectionInput();
        HandlePlacementInput();

        if (radiusLineRenderer != null)
        {
            DrawRadiusCircle(); // only needed if radius can change
        }


    }


    private void HandleBlueprintSelectionInput()
    {
        // Loop through all possible blueprint indices
        for (int i = 0; i < turretBlueprintList.Count; i++)
        {
            // KeyCode.Alpha1 corresponds to 1, Alpha2 to 2, etc.
            // So we use (i + 1) to match the keys
            KeyCode key = KeyCode.Alpha1 + i;

            if (Input.GetKeyDown(key))
            {
                SelectTurretBlueprint(turretBlueprintList[i]);
                break; // Stop after selecting one blueprint
            }
        }
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
        ShowPlacementRadius();
    }

    public void DeselectTurretBlueprint()
    {
        currentSelectedBlueprint = null;
        DestroyPreview();
        HidePlacementRadius();
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

        // --- NEW: only show preview if on ground ---
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 0f, groundLayer);
        if (hit.collider == null)
        {
            previewObject.SetActive(false);
            return;
        }

        // Snap to grid
        Vector2Int gridCoords = GridManager.Instance.GetGridCoordinates(mouseWorldPos);
        Vector3 snappedWorldPos = GridManager.Instance.GetWorldPosition(gridCoords, currentSelectedBlueprint.sizeInCells);

        // Check if placement is valid on the Tilemap + occupancy
        bool canPlace = GridManager.Instance.CanPlaceObject(gridCoords, currentSelectedBlueprint.sizeInCells);

        // Check if blocked by player/enemy
        if (IsPlacementBlocked(snappedWorldPos, currentSelectedBlueprint.sizeInCells))
            canPlace = false;

        if (!IsPlacementOnGround(gridCoords, currentSelectedBlueprint.sizeInCells))
            canPlace = false;

        // Check distance to player
        float distanceToPlayer = playerTransform != null
            ? Vector3.Distance(playerTransform.position, snappedWorldPos)
            : 0f;
        if (playerTransform != null && distanceToPlayer > placementRadius)
            canPlace = false;

        // Check cooldown
        if (IsBlueprintOnCooldown(currentSelectedBlueprint))
            canPlace = false;

        // Check capacity
        int currentCapacity = GetUsedCapacity();
        int cost = currentSelectedBlueprint.buildCapacityValue;
        if (currentCapacity + cost > maxTurretCapacity)
            canPlace = false;

        // Update preview
        previewObject.SetActive(true);
        previewObject.transform.position = snappedWorldPos;
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
        Debug.Log("Why is this not working?");
        if (!activeTurrets.Contains(turret)) return;

        activeTurrets.Remove(turret);

        Destroy(turret);
        OnTurretsChanged?.Invoke();

        Debug.Log($"[TurretPlacement] Turret removed");
    }

    private void TryPlaceTurret()
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("[TurretPlacement] Player transform not assigned.");
            return;
        }

        Vector3 worldPos = GetMouseWorldPosition();

        // --- NEW: check if the mouse is on the ground for tilemap---
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f, groundLayer);
        if (hit.collider == null)
        {
            Debug.Log("Cannot place turret: not on ground tilemap.");
            return;
        }


        Vector2Int gridCoords = GridManager.Instance.GetGridCoordinates(worldPos);
        Vector3 snappedPos = GridManager.Instance.GetWorldPosition(gridCoords, currentSelectedBlueprint.sizeInCells);

        if (!CanPlaceTurretAtPosition(snappedPos, gridCoords)) return;

        GameObject turret = Instantiate(currentSelectedBlueprint.turretPrefab, snappedPos, Quaternion.identity, turretContainer);
        RegisterPlacedTurret(turret, gridCoords);

        float cd = currentSelectedBlueprint.placementCooldown * TurretGlobalModifierManager.Instance.globalTurretPlacementCooldownMultiplier;
        cooldownEndTimes[currentSelectedBlueprint] = Time.time + cd;
        StartCoroutine(StartAndEndCooldown(currentSelectedBlueprint));

        DeselectTurretBlueprint();
        OnTurretsChanged?.Invoke();

        Debug.Log($"[TurretPlacement] Turret placed. Used Capacity: {GetUsedCapacity()}/{maxTurretCapacity}");
    }

    private bool IsPlacementOnGround(Vector2Int startCoords, Vector2Int size)
    {
        // Check each cell the turret would occupy
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int cellCoords = startCoords + new Vector2Int(x, y);
                Vector3 worldPos = GridManager.Instance.GetWorldPosition(cellCoords, Vector2Int.one);

                // Raycast down from the center of each cell to check for ground
                RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f, groundLayer);
                if (hit.collider == null)
                {
                    // This cell is not on the ground -> cannot place
                    return false;
                }

                // Optional: also check walls specifically
                Collider2D wallHit = Physics2D.OverlapBox(worldPos, Vector2.one * GridManager.Instance.cellSize * 0.9f, 0f, wallLayer);
                if (wallHit != null)
                    return false;
            }
        }

        return true;
    }



    private Vector3 GetMouseWorldPosition()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0f;
        return pos;
    }

    private bool CanPlaceTurretAtPosition(Vector3 worldPos, Vector2Int gridCoords)
    {
        // Check blocked layers, grid occupancy, distance, capacity, cooldown
        if (!GridManager.Instance.CanPlaceObject(gridCoords, currentSelectedBlueprint.sizeInCells)) return false;
        if (!IsPlacementOnGround(gridCoords, currentSelectedBlueprint.sizeInCells))
            return false;
        if (IsPlacementBlocked(worldPos, currentSelectedBlueprint.sizeInCells)) return false;
        if (Vector3.Distance(playerTransform.position, worldPos) > placementRadius) return false;
        if (GetUsedCapacity() + currentSelectedBlueprint.buildCapacityValue > maxTurretCapacity) return false;
        if (IsBlueprintOnCooldown(currentSelectedBlueprint)) return false;
        return true;
    }

    private void RegisterPlacedTurret(GameObject turret, Vector2Int gridCoords)
    {
        // Place in grid
        var placable = turret.GetComponentInChildren<PlacableObject>();
        if (placable != null)
        {
            placable.currentGridCoordinates = gridCoords;
            GridManager.Instance.PlaceObject(turret, gridCoords, currentSelectedBlueprint.sizeInCells);
        }

        // Assign behaviour and stats
        var behaviour = turret.GetComponentInChildren<TurretBehaviour>();
        if (behaviour != null)
        {
            behaviour.turretBlueprint = currentSelectedBlueprint;
            behaviour.RecalculateStats();
        }

        var health = turret.GetComponentInChildren<TurretHealth>();
        if (health != null)
        {
            health.Initialize(currentSelectedBlueprint);
            health.OnDeath += OnTurretDeath;
            RegisterTurret(health);
        }

        activeTurrets.Add(turret);
    }

    private IEnumerator HandleCooldown(TurretBlueprint blueprint)
    {
        // announce cooldown start
        OnPlacementCooldownStateChanged?.Invoke(blueprint, true);

        yield return new WaitForSeconds(blueprint.placementCooldown);

        // announce cooldown end
        OnPlacementCooldownStateChanged?.Invoke(blueprint, false);
    }
    private IEnumerator StartAndEndCooldown(TurretBlueprint blueprint)
    {
        // Start
        OnPlacementCooldownStateChanged?.Invoke(blueprint, true);

        yield return new WaitForSeconds(blueprint.placementCooldown);

        // End
        OnPlacementCooldownStateChanged?.Invoke(blueprint, false);
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



    private bool IsPlacementBlocked(Vector3 position, Vector2 size)
    {
        // You can adjust this depending on how your turrets are sized
        Vector2 checkSize = size * 0.9f; // slightly smaller to avoid false positives

        // Check if any collider overlaps with the given area on restricted layers
        Collider2D playerHit = Physics2D.OverlapBox(position, checkSize, 0f, playerLayer);
        Collider2D enemyHit = Physics2D.OverlapBox(position, checkSize, 0f, enemyLayer);

        return playerHit != null || enemyHit != null;
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

    public bool IsBlueprintOnCooldown(TurretBlueprint blueprint)
    {
        if (!cooldownEndTimes.TryGetValue(blueprint, out float endTime))
            return false;

        return Time.time < endTime;
    }
    private void HandleCooldownEvent(TurretBlueprint blueprint, bool active)
    {
        if (currentSelectedBlueprint == blueprint)
            Debug.Log($"Cooldown changed: {blueprint.name} active={active}");
    }

    public float GetCooldownRemaining(TurretBlueprint blueprint)
    {
        if (!cooldownEndTimes.TryGetValue(blueprint, out float endTime))
            return 0f;

        return Mathf.Max(0f, endTime - Time.time);
    }
    public void ClearAllTurrets()
    {
        // Destroy all turret GameObjects
        foreach (var turret in activeTurrets)
        {
            if (turret != null)
                Destroy(turret);
        }

        // Clear tracking lists
        activeTurrets.Clear();
        placedTurrets.Clear();

        // Notify any listeners that turrets changed
        OnTurretsChanged?.Invoke();

        // Destroy any active preview
        DestroyPreview();
    }



    private void OnDrawGizmos()
    {
        //if (currentSelectedBlueprint == null || playerTransform == null) return;

        //Gizmos.color = new Color(0f, 1f, 0f, 0.35f);
        //Gizmos.DrawWireSphere(playerTransform.position, placementRadius);
    }

    private void ShowPlacementRadius()
    {
        if (playerTransform == null || radiusLineRenderer != null)
            return;

        GameObject radiusObj = new GameObject("PlacementRadiusCircle");
        radiusObj.transform.position = Vector3.zero;

        radiusLineRenderer = radiusObj.AddComponent<LineRenderer>();

        // CRITICAL SETTINGS
        radiusLineRenderer.useWorldSpace = false;
        radiusLineRenderer.loop = true;
        radiusLineRenderer.alignment = LineAlignment.TransformZ;
        radiusLineRenderer.textureMode = LineTextureMode.Stretch;

        radiusLineRenderer.material = radiusLineMaterial;
        radiusLineRenderer.startWidth = radiusLineWidth;
        radiusLineRenderer.endWidth = radiusLineWidth;

        radiusLineRenderer.positionCount = radiusSegments + 1;

        // Force visibility
        radiusLineRenderer.sortingLayerName = "Default";
        radiusLineRenderer.sortingOrder = 1000;

        radiusObj.transform.SetParent(playerTransform, false);

        DrawRadiusCircle();
    }


    private void DrawRadiusCircle()
    {
        if (radiusLineRenderer == null) return;

        float angleStep = 2f * Mathf.PI / radiusSegments;

        for (int i = 0; i <= radiusSegments; i++)
        {
            float angle = angleStep * i;

            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * placementRadius,
                Mathf.Sin(angle) * placementRadius,
                0f
            );

            radiusLineRenderer.SetPosition(i, pos);
        }
    }


    private void HidePlacementRadius()
    {
        if (radiusLineRenderer != null)
        {
            Destroy(radiusLineRenderer.gameObject);
            radiusLineRenderer = null;
        }
    }
}
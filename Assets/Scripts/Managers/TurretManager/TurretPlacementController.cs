using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
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

    [Header("Blocking Layers")]
    [Tooltip("Layer for player characters (placement blocked).")]
    public LayerMask playerLayer;

    [Tooltip("Layer for enemies (placement blocked).")]
    public LayerMask enemyLayer;

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

    [Tooltip("Reference to the player transform.")]
    public Transform playerTransform;

    //Currently Active Turrets
    [SerializeField]private List<GameObject> activeTurrets = new List<GameObject>();
    [SerializeField]private List<TurretHealth> placedTurrets = new List<TurretHealth>();
    private Dictionary<TurretBlueprint, float> cooldownEndTimes = new();

    public event Action<TurretBlueprint, bool> OnPlacementCooldownStateChanged;


    private void OnEnable()
    {
        OnPlacementCooldownStateChanged += HandleCooldownEvent;

    }
    private void OnDisable()
    {
       OnPlacementCooldownStateChanged -= HandleCooldownEvent;

    }

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

        // Snap to grid
        Vector2Int gridCoords = GridManager.Instance.GetGridCoordinates(mouseWorldPos);
        Vector3 snappedWorldPos = GridManager.Instance.GetWorldPosition(gridCoords, currentSelectedBlueprint.sizeInCells);

        // Check if placement is valid on the Tilemap + occupancy
        bool canPlace = GridManager.Instance.CanPlaceObject(gridCoords, currentSelectedBlueprint.sizeInCells);

        // Check if blocked by player/enemy
        if (IsPlacementBlocked(snappedWorldPos, currentSelectedBlueprint.sizeInCells))
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
            Debug.LogWarning("[TurretPlacement] No player transform assigned. Please set it in the inspector.");
            return;
        }

        int currentCapacity = GetUsedCapacity();
        int cost = currentSelectedBlueprint.buildCapacityValue;

        if (currentCapacity + cost > maxTurretCapacity)
        {
            Debug.Log($"[TurretPlacement] Not enough capacity. Current: {currentCapacity}/{maxTurretCapacity}, Cost: {cost}");
            return;
        }

        if (IsBlueprintOnCooldown(currentSelectedBlueprint))
        {
            Debug.Log($"[TurretPlacement] {currentSelectedBlueprint.name} is on cooldown ({GetCooldownRemaining(currentSelectedBlueprint):F2}s left).");
            return;
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector2Int gridCoords = GridManager.Instance.GetGridCoordinates(mouseWorldPos);
        Vector3 targetWorldPos = GridManager.Instance.GetWorldPosition(gridCoords, currentSelectedBlueprint.sizeInCells);

        float distanceToPlayer = Vector3.Distance(playerTransform.position, targetWorldPos);
        if (distanceToPlayer > placementRadius)
        {
            Debug.Log($"[TurretPlacement] Too far from player! Distance: {distanceToPlayer:F2}, Max: {placementRadius}");
            return;
        }


        if (IsPlacementBlocked(targetWorldPos, currentSelectedBlueprint.sizeInCells))
        {
            Debug.Log("[TurretPlacement] Cannot place: blocked by enemy or player.");
            return;
        }

        // Instantiate turret at centered position
        GameObject newTurret = Instantiate(
            currentSelectedBlueprint.turretPrefab,
            GridManager.Instance.GetWorldPosition(gridCoords, currentSelectedBlueprint.sizeInCells),
            Quaternion.identity,
            turretContainer
        );

        // Register turret in grid and give the turret its coordiantes
        PlacableObject turretPlacable = newTurret.GetComponentInChildren<PlacableObject>();
        if (turretPlacable != null)
        {
            GridManager.Instance.PlaceObject(newTurret, gridCoords, currentSelectedBlueprint.sizeInCells);
            turretPlacable.currentGridCoordinates = gridCoords;
        }

        TurretBehaviour behaviour = newTurret.GetComponentInChildren<TurretBehaviour>();
        if (behaviour != null)
        {
            behaviour.turretBlueprint = currentSelectedBlueprint;
            behaviour.RecalculateStats();
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

        float cd = currentSelectedBlueprint.placementCooldown *
                   TurretModifierManager.Instance.globalTurretPlacementCooldownMultiplier;

        float endTime = Time.time + cd;
        cooldownEndTimes[currentSelectedBlueprint] = endTime;

        StartCoroutine(StartAndEndCooldown(currentSelectedBlueprint));

        DeselectTurretBlueprint();
        //DestroyPreview();
        Debug.Log($"[TurretPlacement] Turret placed. Used Capacity: {GetUsedCapacity()}/{maxTurretCapacity}");
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


    public void SetupFromGameData(GameDataSO gameData)
    {
        turretBlueprintList = gameData.allTurretBlueprints
            .Where(b => gameData.unlockedTurrets.Contains(b.turretType))
            .ToList();

        Debug.Log($"[TurretPlacement] Loaded {turretBlueprintList.Count} unlocked turrets from GameDataSO.");
        OnTurretsChanged?.Invoke();
    }

    private bool IsPlacementBlocked(Vector3 position, Vector2 size)
    {
        // You can adjust this depending on how your turrets are sized
        Vector2 checkSize = size * 0.9f; // slightly smaller to avoid false positives

        // Check if any collider overlaps with the given area on restricted layers
        Collider2D playerHit = Physics2D.OverlapBox(position, checkSize, 0f, playerLayer);
        Collider2D enemyHit = Physics2D.OverlapBox(position, checkSize, 0f, enemyLayer);

        return playerHit != null || enemyHit != null;
    }



    private void OnDrawGizmos()
    {
        if (currentSelectedBlueprint == null || playerTransform == null) return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.35f);
        Gizmos.DrawWireSphere(playerTransform.position, placementRadius);
    }
}
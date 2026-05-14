using System;
using System.Collections;
using System.Collections.Generic;
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
    public LayerMask groundLayer;
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
    public GameObject previewObject;
    private PlacableObject previewPlacableObject;

    [Header("Placement Rules")]
    [Tooltip("Maximum turret capacity (not just number).")]
    public int defaultMaxTurretCapacity = 6;
    public int maxTurretCapacity = 6;

    [Header("Hierarchy Organization")]
    [SerializeField] private Transform turretContainer;

    [Header("Placement Range")]
    [Tooltip("Maximum distance from the player where turrets can be placed.")]
    public float defaultPlacementRadius = 30f;
    public float placementRadius = 30f;
    public float PlacementRadius
    {
        get => placementRadius;
        set
        {
            placementRadius = value;
            DrawRadiusCircle();
        }
    }

    [Header("Placement Radius Visual (LineRenderer)")]
    [SerializeField] private Material radiusLineMaterial;
    [SerializeField] private float radiusLineWidth = 0.05f;
    [SerializeField] private int radiusSegments = 64;

    private LineRenderer radiusLineRenderer;

    [Tooltip("Reference to the player transform.")]
    public Transform playerTransform;

    // Active turret tracking
    public List<GameObject> activeTurrets = new();
    private List<TurretHealth> placedTurrets = new();
    private Dictionary<TurretBlueprint, float> cooldownEndTimes = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Debug.LogWarning("[TurretPlacement] Duplicate instance detected. Destroying this object.");
            Destroy(gameObject);
        }

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void OnEnable() => OnPlacementCooldownStateChanged += HandleCooldownEvent;
    private void OnDisable() => OnPlacementCooldownStateChanged -= HandleCooldownEvent;

    // ========================
    // Public Methods
    // ========================

    public void SetupFromGameData(GameDataSO gameData)
    {
        turretBlueprintList = new List<TurretBlueprint>(gameData.GetSelectedBlueprints());
        Debug.Log($"[TurretPlacement] Loaded {turretBlueprintList.Count} unlocked turrets from GameDataSO.");
        OnTurretsChanged?.Invoke();
    }

    public void SelectTurretBlueprint(TurretBlueprint blueprint)
    {
        if (currentSelectedBlueprint == blueprint) return;

        if (TurretDemolitionController.Instance != null && TurretDemolitionController.Instance.IsDestructionModeActive())
            TurretDemolitionController.Instance.ForceDeactivateDestructionMode();

        currentSelectedBlueprint = blueprint;
        Debug.Log("Selected Blueprint: " + (currentSelectedBlueprint != null ? currentSelectedBlueprint.name : "None"));

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

    public void RemoveTurret(GameObject turret)
    {
        if (!activeTurrets.Contains(turret)) return;

        activeTurrets.Remove(turret);
        Destroy(turret);
        OnTurretsChanged?.Invoke();

        Debug.Log($"[TurretPlacement] Turret removed");
    }

    public void RegisterTurret(TurretHealth turret)
    {
        if (!placedTurrets.Contains(turret))
            placedTurrets.Add(turret);
    }

    public void UnregisterTurret(TurretHealth turret)
    {
        placedTurrets.Remove(turret);
        OnTurretsChanged.Invoke();
    }

    public bool IsBlueprintOnCooldown(TurretBlueprint blueprint)
    {
        if (!cooldownEndTimes.TryGetValue(blueprint, out float endTime))
            return false;
        return Time.time < endTime;
    }

    public float GetCooldownRemaining(TurretBlueprint blueprint)
    {
        if (!cooldownEndTimes.TryGetValue(blueprint, out float endTime))
            return 0f;
        return Mathf.Max(0f, endTime - Time.time);
    }

    public int GetUsedCapacity()
    {
        int total = 0;
        foreach (var turret in activeTurrets)
        {
            var behaviour = turret.GetComponentInChildren<TurretBehaviour>();
            if (behaviour?.turretBlueprint != null)
                total += behaviour.turretBlueprint.buildCapacityValue;
        }
        return total;
    }

    public List<GameObject> GetActiveTurrets() => activeTurrets;
    public List<TurretBlueprint> GetTurretBlueprintList() => turretBlueprintList;

    public void ClearAllTurrets()
    {
        foreach (var turret in activeTurrets)
        {
            if (turret != null)
                Destroy(turret);
        }
        activeTurrets.Clear();
        placedTurrets.Clear();
        DestroyPreview();
        OnTurretsChanged?.Invoke();
    }

    // ========================
    // Update Loop
    // ========================

    private void Update()
    {
        HandleBlueprintSelectionInput();
        HandlePlacementInput();
        if (radiusLineRenderer != null)
            DrawRadiusCircle();
    }

    // ========================
    // Input Handling
    // ========================

    private void HandleBlueprintSelectionInput()
    {
        for (int i = 0; i < turretBlueprintList.Count; i++)
        {
            KeyCode key = KeyCode.Alpha1 + i;
            if (Input.GetKeyDown(key))
            {
                SelectTurretBlueprint(turretBlueprintList[i]);
                break;
            }
        }
    }

    private void HandlePlacementInput()
    {
        if (TurretDemolitionController.Instance != null && TurretDemolitionController.Instance.IsDestructionModeActive())
        {
            if (currentSelectedBlueprint != null)
                DeselectTurretBlueprint();
            return;
        }

        if (Input.GetMouseButtonDown(0) && currentSelectedBlueprint != null)
            TryPlaceTurret();

        if (Input.GetMouseButtonDown(1))
            DeselectTurretBlueprint();

        if (currentSelectedBlueprint != null)
            HandlePlacementPreview();
        else if (previewObject != null)
            DestroyPreview();
    }

    // ========================
    // Placement Preview
    // ========================

    private void CreateOrUpdatePreviewObject()
    {
        DestroyPreview();
        if (currentSelectedBlueprint?.previewPrefab == null) return;

        previewObject = Instantiate(currentSelectedBlueprint.previewPrefab);
        MakePreviewTransparent(previewObject);

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

        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 0f, groundLayer);
        if (hit.collider == null)
        {
            previewObject.SetActive(false);
            return;
        }

        Vector2Int gridCoords = GridManager.Instance.GetGridCoordinates(mouseWorldPos);
        Vector3 snappedWorldPos = GridManager.Instance.GetWorldPosition(gridCoords, currentSelectedBlueprint.sizeInCells);

        bool canPlace = CanPlaceTurretAtPosition(snappedWorldPos, gridCoords);

        previewObject.SetActive(true);
        previewObject.transform.position = snappedWorldPos;
        UpdatePreviewColor(canPlace);
    }

    private void MakePreviewTransparent(GameObject obj)
    {
        if (obj.TryGetComponent(out Renderer renderer))
        {
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = new Color(1f, 1f, 1f, 0.5f);
            mat.renderQueue = 4000;
            renderer.material = mat;
        }
    }

    private void UpdatePreviewColor(bool canPlace)
    {
        if (previewObject == null) return;

        if (previewObject.TryGetComponent(out Renderer renderer))
        {
            Color color = canPlace ? Color.green : Color.red;
            color.a = 0.5f;
            renderer.material.color = color;
        }
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

    // ========================
    // Turret Placement
    // ========================

    private void TryPlaceTurret()
    {

        if (playerTransform == null) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f, groundLayer);
        if (hit.collider == null) return;

        Vector2Int gridCoords = GridManager.Instance.GetGridCoordinates(worldPos);
        Vector3 snappedPos = GridManager.Instance.GetWorldPosition(gridCoords, currentSelectedBlueprint.sizeInCells);

        if (!CanPlaceTurretAtPosition(snappedPos, gridCoords)) return;

        GameObject turret = Instantiate(currentSelectedBlueprint.turretPrefab, snappedPos, Quaternion.identity, turretContainer);
        RegisterPlacedTurret(turret, gridCoords);


        float cd = GetModifiedPlacementCooldown(currentSelectedBlueprint);
        cooldownEndTimes[currentSelectedBlueprint] = Time.time + cd;

        StartCoroutine(StartAndEndCooldown(currentSelectedBlueprint, cd));

        //Play a Click sound to give audio feedback to the Player
        SoundManager.Instance.PlayTowerBuild();

        DeselectTurretBlueprint();
        OnTurretsChanged?.Invoke();
    }

    private bool CanPlaceTurretAtPosition(Vector3 worldPos, Vector2Int gridCoords)
    {
        if (!GridManager.Instance.CanPlaceObject(gridCoords, currentSelectedBlueprint.sizeInCells))
            return false;

        if (!IsPlacementOnGround(gridCoords, currentSelectedBlueprint.sizeInCells))
            return false;

        if (IsPlacementBlocked(worldPos, currentSelectedBlueprint.sizeInCells))
            return false;

        if (Vector3.Distance(playerTransform.position, worldPos) > placementRadius)
            return false;

        if (GetUsedCapacity() + currentSelectedBlueprint.buildCapacityValue > maxTurretCapacity)
            return false;

        if (IsBlueprintOnCooldown(currentSelectedBlueprint))
            return false;

        return true;
    }

    private bool IsPlacementOnGround(Vector2Int startCoords, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int cellCoords = startCoords + new Vector2Int(x, y);
                Vector3 worldPos = GridManager.Instance.GetWorldPosition(cellCoords, Vector2Int.one);

                if (Physics2D.Raycast(worldPos, Vector2.zero, 0f, groundLayer).collider == null)
                    return false;

                if (Physics2D.OverlapBox(worldPos, Vector2.one * GridManager.Instance.cellSize * 0.9f, 0f, wallLayer) != null)
                    return false;
            }
        }

        return true;
    }

    private bool IsPlacementBlocked(Vector3 position, Vector2 size)
    {
        Vector2 checkSize = size * GridManager.Instance.cellSize * 0.9f;

        return Physics2D.OverlapBox(position, checkSize, 0f, playerLayer) != null ||
               Physics2D.OverlapBox(position, checkSize, 0f, enemyLayer) != null ||
               Physics2D.OverlapBox(position, checkSize, 0f, turretLayer) != null;
    }
    private void RegisterPlacedTurret(GameObject turret, Vector2Int gridCoords)
    {
        if (turret.TryGetComponent(out PlacableObject placable))
        {
            placable.currentGridCoordinates = gridCoords;
            GridManager.Instance.PlaceObject(turret, gridCoords, currentSelectedBlueprint.sizeInCells);
        }

        var behaviour = turret.GetComponentInChildren<TurretBehaviour>();
        var stats = turret.GetComponentInChildren<TurretStats>();

        if (behaviour != null && stats != null)
        {
            behaviour.turretBlueprint = currentSelectedBlueprint;

            int level = TurretLevelManager.Instance.GetLevel(currentSelectedBlueprint.turretType);

            var upgrade =
                TurretUpgradeChoiceManager.Instance != null
                    ? TurretUpgradeChoiceManager.Instance.GetCombinedModifier(currentSelectedBlueprint.turretType)
                    : null;

            stats.RecalculateStats(
                behaviour,
                currentSelectedBlueprint,
                level,
                upgrade,
                TurretGlobalModifierManager.Instance
            );
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

    private IEnumerator StartAndEndCooldown(TurretBlueprint blueprint, float cooldown)
    {
        OnPlacementCooldownStateChanged?.Invoke(blueprint, true);
        yield return new WaitForSeconds(cooldown);
        OnPlacementCooldownStateChanged?.Invoke(blueprint, false);
    }

    private void OnTurretDeath(TurretHealth turret, DamageData data)
    {
        UnregisterTurret(turret);
        if (turret != null && activeTurrets.Contains(turret.gameObject))
            activeTurrets.Remove(turret.gameObject);

        activeTurrets.Remove(turret.transform.parent.gameObject);
        OnTurretsChanged?.Invoke();
    }

    private void HandleCooldownEvent(TurretBlueprint blueprint, bool active)
    {
        if (currentSelectedBlueprint == blueprint)
            Debug.Log($"Cooldown changed: {blueprint.name} active={active}");
    }

    public float GetModifiedPlacementCooldown(TurretBlueprint blueprint)
    {
        float baseCooldown = blueprint.placementCooldown;
        float multiplier = 1f - TurretGlobalModifierManager.Instance.globalTurretPlacementCooldownMultiplier;
        return Mathf.Max(0.05f, baseCooldown * multiplier); // prevent zero or negative cooldown
    }

    // ========================
    // Placement Radius
    // ========================

    private void ShowPlacementRadius()
    {
        if (playerTransform == null || radiusLineRenderer != null)
            return;

        GameObject radiusObj = new GameObject("PlacementRadiusCircle");
        radiusObj.transform.position = Vector3.zero;

        radiusLineRenderer = radiusObj.AddComponent<LineRenderer>();
        radiusLineRenderer.useWorldSpace = false;
        radiusLineRenderer.loop = true;
        radiusLineRenderer.alignment = LineAlignment.TransformZ;
        radiusLineRenderer.textureMode = LineTextureMode.Stretch;

        radiusLineRenderer.material = radiusLineMaterial;
        radiusLineRenderer.startWidth = radiusLineWidth;
        radiusLineRenderer.endWidth = radiusLineWidth;
        radiusLineRenderer.positionCount = radiusSegments + 1;
        radiusLineRenderer.sortingLayerName = "Default";
        radiusLineRenderer.sortingOrder = 1000;

        radiusObj.transform.SetParent(playerTransform, false);

        DrawRadiusCircle();
    }

    public void DrawRadiusCircle()
    {
        if (radiusLineRenderer == null) return;

        float angleStep = 2f * Mathf.PI / radiusSegments;
        for (int i = 0; i <= radiusSegments; i++)
        {
            float angle = angleStep * i;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * placementRadius, Mathf.Sin(angle) * placementRadius, 0f);
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

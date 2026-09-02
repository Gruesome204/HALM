using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretPlacementController : MonoBehaviour, IGameSystem
{
    public static TurretPlacementController Instance { get; private set; }
    public event Action OnTurretsChanged;
    public event Action<TurretBlueprint, bool> OnPlacementCooldownStateChanged;

    // ========================
    // BLUEPRINTS
    // ========================
    [Header("Turret Blueprints")]
    public List<TurretBlueprint> turretBlueprintList = new();
    public TurretBlueprint currentSelectedBlueprint;

    // ========================
    // LAYERS
    // ========================
    [Header("Layers")]
    public LayerMask groundLayer;
    public LayerMask turretLayer;

    [Header("Blocking Layers")]
    public LayerMask playerLayer;
    public LayerMask enemyLayer;
    public LayerMask wallLayer;

    // ========================
    // PREVIEW
    // ========================
    [Header("Placement Preview")]
    public GameObject previewObject;
    private PlacableObject previewPlacableObject;

    // ========================
    // RULES
    // ========================
    [Header("Placement Rules")]
    public int defaultMaxTurretCapacity = 6;
    public int maxTurretCapacity = 6;

    // ========================
    // HIERARCHY
    // ========================
    [Header("Hierarchy Organization")]
    [SerializeField] private Transform turretContainer;

    // ========================
    // RADIUS
    // ========================
    [Header("Placement Range")]
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

    [Header("Placement Radius Visual")]
    [SerializeField] private Material radiusLineMaterial;
    [SerializeField] private float radiusLineWidth = 0.05f;
    [SerializeField] private int radiusSegments = 64;

    private LineRenderer radiusLineRenderer;

    [Tooltip("Reference to the player transform.")]
    public Transform playerTransform;

    // ========================
    // STATE
    // ========================
    public List<GameObject> activeTurrets = new();
    private List<TurretHealth> placedTurrets = new();
    private Dictionary<TurretBlueprint, float> cooldownEndTimes = new();
    private bool isInitialized = false;

    // ========================
    // IGameSystem Implementation
    // ========================
    public int InitializePriority => 3;

    public void Initialize()
    {
        if (isInitialized)
        {
            Debug.Log("[TurretPlacementController] Already initialized.");
            return;
        }

        // Find required dependencies
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
            else
                Debug.LogWarning("[TurretPlacementController] Player not found!");
        }

        // Validate required components
        if (GridManager.Instance == null)
        {
            Debug.LogError("[TurretPlacementController] GridManager not found!");
            return;
        }

        if (SoundManager.Instance == null)
        {
            Debug.LogWarning("[TurretPlacementController] SoundManager not found!");
        }

        if (TurretGlobalModifierManager.Instance == null)
        {
            Debug.LogWarning("[TurretPlacementController] TurretGlobalModifierManager not found!");
        }

        // Setup turret container if not assigned
        if (turretContainer == null)
        {
            GameObject container = new GameObject("Turrets");
            container.transform.SetParent(transform);
            turretContainer = container.transform;
        }

        // Reset state
        activeTurrets.Clear();
        placedTurrets.Clear();
        cooldownEndTimes.Clear();
        maxTurretCapacity = defaultMaxTurretCapacity;
        placementRadius = defaultPlacementRadius;

        isInitialized = true;
        Debug.Log("[TurretPlacementController] Initialized");
    }

    public void PostInitialize()
    {
        Debug.Log("[TurretPlacementController] Post-Initializing...");

        // Load turret blueprints from GameData if available
        if (GameManager.Instance != null && GameManager.Instance.gameDataSO != null)
        {
            SetupFromGameData(GameManager.Instance.gameDataSO);
        }
        else
        {
            Debug.LogWarning("[TurretPlacementController] No GameData available, using default blueprints.");
        }

        // Setup radius visual if we have a player
        if (playerTransform != null)
        {
            ShowPlacementRadius();
        }
    }

    // ========================
    // UNITY
    // ========================
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Auto-initialize if not already done
        if (!isInitialized)
        {
            Debug.LogWarning("[TurretPlacementController] Auto-initializing in Start()");
            Initialize();
            PostInitialize();
        }
    }

    private void OnEnable()
    {
        OnPlacementCooldownStateChanged += HandleCooldownEvent;
    }

    private void OnDisable()
    {
        OnPlacementCooldownStateChanged -= HandleCooldownEvent;
    }

    private void Update()
    {
        if (!isInitialized) return;

        HandleBlueprintSelectionInput();
        HandlePlacementInput();

        if (radiusLineRenderer != null)
            DrawRadiusCircle();
    }

    private void OnDestroy()
    {
        // Cleanup
        DestroyPreview();
        HidePlacementRadius();
        ClearAllTurrets();
    }

    // ========================
    // SETUP
    // ========================
    public void SetupFromGameData(GameDataSO gameData)
    {
        if (gameData == null)
        {
            Debug.LogError("[TurretPlacementController] GameData is null!");
            return;
        }

        var selectedBlueprints = gameData.GetSelectedBlueprints();

        // Clear existing list and add all selected blueprints
        turretBlueprintList.Clear();

        if (selectedBlueprints != null && selectedBlueprints.Count > 0)
        {
            foreach (var blueprint in selectedBlueprints)
            {
                if (blueprint != null)
                    turretBlueprintList.Add(blueprint);
            }
            Debug.Log($"[TurretPlacementController] Loaded {turretBlueprintList.Count} turret blueprints from GameData");
        }
        else
        {
            Debug.LogWarning("[TurretPlacementController] No blueprints selected in GameData, using default list");
        }

        OnTurretsChanged?.Invoke();
    }

    // ========================
    // SELECTION
    // ========================
    public void SelectTurretBlueprint(TurretBlueprint blueprint)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[TurretPlacementController] Not initialized!");
            return;
        }

        if (currentSelectedBlueprint == blueprint) return;

        if (TurretDemolitionController.Instance?.IsDestructionModeActive() == true)
            TurretDemolitionController.Instance.ForceDeactivateDestructionMode();

        currentSelectedBlueprint = blueprint;

        CreateOrUpdatePreviewObject();
        ShowPlacementRadius();
    }

    public void DeselectTurretBlueprint()
    {
        currentSelectedBlueprint = null;
        DestroyPreview();
        HidePlacementRadius();
    }

    // ========================
    // INPUT
    // ========================
    private void HandleBlueprintSelectionInput()
    {
        for (int i = 0; i < turretBlueprintList.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectTurretBlueprint(turretBlueprintList[i]);
                return;
            }
        }
    }

    private void HandlePlacementInput()
    {
        if (TurretDemolitionController.Instance?.IsDestructionModeActive() == true)
        {
            if (currentSelectedBlueprint != null)
                DeselectTurretBlueprint();
            return;
        }

        if (Input.GetMouseButtonDown(1))
            DeselectTurretBlueprint();

        if (currentSelectedBlueprint == null)
        {
            DestroyPreview();
            return;
        }

        HandlePlacementPreview();

        if (Input.GetMouseButtonDown(0))
            TryPlaceTurret();
    }

    // ========================
    // PREVIEW
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

        Vector3 mouseWorld = GetMouseWorld();
        RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero, 0f, groundLayer);

        if (!hit)
        {
            previewObject.SetActive(false);
            return;
        }

        Vector2Int grid = GridManager.Instance.GetGridCoordinates(mouseWorld);
        Vector3 snapped = GridManager.Instance.GetWorldPosition(grid, currentSelectedBlueprint.sizeInCells);

        bool canPlace = CanPlaceTurretAtPosition(snapped, grid);

        previewObject.SetActive(true);
        previewObject.transform.position = snapped;

        UpdatePreviewColor(canPlace);
    }

    private Vector3 GetMouseWorld()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0f;
        return pos;
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
        if (!previewObject || !previewObject.TryGetComponent(out Renderer renderer)) return;

        Color color = canPlace ? Color.green : Color.red;
        color.a = 0.5f;

        renderer.material.color = color;
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
    // PLACEMENT
    // ========================
    private void TryPlaceTurret()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[TurretPlacementController] Cannot place turret - not initialized!");
            return;
        }

        if (playerTransform == null || currentSelectedBlueprint == null) return;

        Vector3 world = GetMouseWorld();
        RaycastHit2D hit = Physics2D.Raycast(world, Vector2.zero, 0f, groundLayer);
        if (!hit) return;

        Vector2Int grid = GridManager.Instance.GetGridCoordinates(world);
        Vector3 snapped = GridManager.Instance.GetWorldPosition(grid, currentSelectedBlueprint.sizeInCells);

        if (!CanPlaceTurretAtPosition(snapped, grid)) return;

        GameObject turret = Instantiate(
            currentSelectedBlueprint.turretPrefab,
            snapped,
            Quaternion.identity,
            turretContainer
        );

        RegisterPlacedTurret(turret, grid);

        float cd = GetModifiedPlacementCooldown(currentSelectedBlueprint);
        cooldownEndTimes[currentSelectedBlueprint] = Time.time + cd;

        StartCoroutine(StartAndEndCooldown(currentSelectedBlueprint, cd));

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayTowerBuild();

        DeselectTurretBlueprint();
        OnTurretsChanged?.Invoke();
    }

    // ========================
    // VALIDATION
    // ========================
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
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int cell = startCoords + new Vector2Int(x, y);
                Vector3 worldPos = GridManager.Instance.GetWorldPosition(cell, Vector2Int.one);

                if (!Physics2D.Raycast(worldPos, Vector2.zero, 0f, groundLayer))
                    return false;

                if (Physics2D.OverlapBox(worldPos, Vector2.one * GridManager.Instance.cellSize * 0.9f, 0f, wallLayer))
                    return false;
            }

        return true;
    }

    private bool IsPlacementBlocked(Vector3 position, Vector2 size)
    {
        Vector2 checkSize = size * GridManager.Instance.cellSize * 0.9f;

        return Physics2D.OverlapBox(position, checkSize, 0f, playerLayer) ||
               Physics2D.OverlapBox(position, checkSize, 0f, enemyLayer) ||
               Physics2D.OverlapBox(position, checkSize, 0f, turretLayer);
    }

    // ========================
    // TURRET REGISTRATION 
    // ========================
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
            behaviour.SetTurretBlueprint(currentSelectedBlueprint);

            int level = TurretLevelManager.Instance != null
                ? TurretLevelManager.Instance.GetLevel(currentSelectedBlueprint.turretType)
                : 1;

            var upgrade = TurretUpgradeChoiceManager.Instance != null
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

    // ========================
    // COOLDOWN
    // ========================
    private IEnumerator StartAndEndCooldown(TurretBlueprint blueprint, float cooldown)
    {
        OnPlacementCooldownStateChanged?.Invoke(blueprint, true);
        yield return new WaitForSeconds(cooldown);
        OnPlacementCooldownStateChanged?.Invoke(blueprint, false);
    }

    private void HandleCooldownEvent(TurretBlueprint blueprint, bool active)
    {
        if (currentSelectedBlueprint == blueprint)
            Debug.Log($"Cooldown changed: {blueprint.name} active={active}");
    }

    public float GetModifiedPlacementCooldown(TurretBlueprint blueprint)
    {
        float baseCooldown = blueprint.placementCooldown;
        float multiplier = TurretGlobalModifierManager.Instance != null
            ? 1f - TurretGlobalModifierManager.Instance.GlobalTurretPlacementCooldownMultiplier
            : 1f;
        return Mathf.Max(0.05f, baseCooldown * multiplier);
    }

    // ========================
    // RADIUS VISUAL
    // ========================
    private void ShowPlacementRadius()
    {
        if (playerTransform == null || radiusLineRenderer != null)
            return;

        GameObject obj = new GameObject("PlacementRadiusCircle");
        obj.transform.SetParent(playerTransform, false);

        radiusLineRenderer = obj.AddComponent<LineRenderer>();
        radiusLineRenderer.useWorldSpace = false;
        radiusLineRenderer.loop = true;
        radiusLineRenderer.material = radiusLineMaterial;
        radiusLineRenderer.startWidth = radiusLineWidth;
        radiusLineRenderer.endWidth = radiusLineWidth;
        radiusLineRenderer.positionCount = radiusSegments + 1;
        radiusLineRenderer.sortingOrder = 1000;

        DrawRadiusCircle();
    }

    public void DrawRadiusCircle()
    {
        if (radiusLineRenderer == null) return;

        float step = 2f * Mathf.PI / radiusSegments;

        for (int i = 0; i <= radiusSegments; i++)
        {
            float angle = step * i;
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

    // ========================
    // PUBLIC METHODS
    // ========================
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
            if (turret == null) continue;

            var behaviour = turret.GetComponentInChildren<TurretBehaviour>();
            if (behaviour?.TurretBlueprint != null)
                total += behaviour.TurretBlueprint.buildCapacityValue;
        }

        return total;
    }

    public List<GameObject> GetActiveTurrets() => new List<GameObject>(activeTurrets);
    public List<TurretBlueprint> GetTurretBlueprintList() => new List<TurretBlueprint>(turretBlueprintList);
    public bool IsInitialized => isInitialized;

    public void ClearAllTurrets()
    {
        foreach (var turret in activeTurrets)
            if (turret != null)
                Destroy(turret);

        activeTurrets.Clear();
        placedTurrets.Clear();
        DestroyPreview();

        OnTurretsChanged?.Invoke();
    }

    private void OnTurretDeath(TurretHealth turret, DamageData data)
    {
        UnregisterTurret(turret);

        if (turret != null && turret.gameObject != null)
        {
            // Remove the parent turret object
            GameObject turretParent = turret.transform.parent?.gameObject;
            if (turretParent != null)
                activeTurrets.Remove(turretParent);
        }

        OnTurretsChanged?.Invoke();
    }

    public void RemoveTurret(GameObject turret)
    {
        if (turret == null) return;

        if (activeTurrets.Contains(turret))
            activeTurrets.Remove(turret);

        Destroy(turret);
        OnTurretsChanged?.Invoke();
    }

    public void RegisterTurret(TurretHealth turret)
    {
        if (!placedTurrets.Contains(turret))
            placedTurrets.Add(turret);
    }

    public void UnregisterTurret(TurretHealth turret)
    {
        placedTurrets.Remove(turret);
        OnTurretsChanged?.Invoke();
    }

    // ========================
    // GAME STATE HANDLING
    // ========================
    public void OnGamePaused(bool paused)
    {
        if (paused)
        {
            DeselectTurretBlueprint();
        }
    }

    public void OnGameReset()
    {
        ClearAllTurrets();
        DeselectTurretBlueprint();
        cooldownEndTimes.Clear();
        maxTurretCapacity = defaultMaxTurretCapacity;
        placementRadius = defaultPlacementRadius;
    }
}
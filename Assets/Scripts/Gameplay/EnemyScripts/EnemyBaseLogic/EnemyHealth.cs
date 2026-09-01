using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour, IDamagable, IParryable
{
    #region Serialized Fields
    [Header("References")]
    [SerializeField] private EnemyStats stats;
    [SerializeField] private EnemyMovement movement;

    [Header("UI")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private BossBarUI bossBarUIPrefab;

    [Header("Damage Flash")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float flashDuration = 0.3f;
    #endregion

    #region Private Fields
    private Canvas canvas;
    private MaterialPropertyBlock mpb;
    private Coroutine flashRoutine;
    private BossBarUI currentBossBarUI;
    private BossEnemyBehaviour bossBehaviour;
    private bool isBossBarSetup = false;
    #endregion

    #region Public Properties
    public bool IsInvulnerable { get; set; }
    public EnemyStats Stats => stats;
    #endregion

    #region Events
    public event Action<EnemyHealth, DamageData> OnDeath;
    public event Action<DamageData, KnockbackData> OnDamaged;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();
        spriteRenderer ??= GetComponentInChildren<SpriteRenderer>();
        mpb = new MaterialPropertyBlock();

        if (stats == null || stats.baseStats == null)
        {
            Debug.LogError($"[EnemyHealth] {gameObject.name} has no stats assigned!");
            return;
        }

        // Initialize health
        stats.currentHealth = stats.maxHealth;
        Debug.Log($"[EnemyHealth] {gameObject.name}: Health initialized: {stats.currentHealth}/{stats.maxHealth}");

        canvas = FindObjectOfType<Canvas>();
        bossBehaviour = GetComponent<BossEnemyBehaviour>();

        Debug.Log($"[EnemyHealth] {gameObject.name}: BossBehaviour found: {(bossBehaviour != null ? "Yes" : "No")}");
        Debug.Log($"[EnemyHealth] {gameObject.name}: BossBarPrefab assigned: {(bossBarUIPrefab != null ? "Yes" : "No")}");
        Debug.Log($"[EnemyHealth] {gameObject.name}: Stats type: {stats.baseStats?.GetType().Name ?? "null"}");

        SetupHealthUI();
    }

    private void Start()
    {
        // Try setting up boss bar again in Start() if it failed in Awake()
        if (!isBossBarSetup && bossBehaviour != null && bossBarUIPrefab != null)
        {
            Debug.Log($"[EnemyHealth] {gameObject.name}: Retrying boss bar setup in Start()");
            SetupBossBarUI();
        }
    }
    #endregion

    #region Public Methods
    public void TakeDamage(DamageData damageData, KnockbackData knockbackData)
    {
        if (IsInvulnerable || stats == null) return;

        OnDamaged?.Invoke(damageData, knockbackData);

        float damage = CalculateTakenDamage(damageData);
        stats.currentHealth = Mathf.Clamp(stats.currentHealth - damage, 0f, stats.maxHealth);

        PlayDamageFlash();
        UpdateHealthUI();

        Debug.Log($"{gameObject.name} took {damage} {damageData.type} damage with {knockbackData.knockbackStrength} knockback.");

        if (stats.currentHealth <= 0)
            Die(damageData);
    }

    public void Die(DamageData damageData)
    {
        // Hide boss bar on death
        if (currentBossBarUI != null)
        {
            currentBossBarUI.HideBossBar();
        }
        OnDeath?.Invoke(this, damageData);
    }

    public void OnDamageTaken(float amount) { }

    public bool IsAlive() => stats != null && stats.currentHealth > 0;

    public Transform GetTransform() => transform;

    public TargetType GetTargetType() => TargetType.Enemy;

    public void OnParried(GameObject source, float counterDamage)
    {
        TakeDamage(new DamageData
        {
            amount = counterDamage,
            type = DamageData.DamageType.Physical,
            source = source
        }, new KnockbackData { knockbackStrength = 0 });

        Debug.Log($"{gameObject.name} was parried and took {counterDamage} damage!");
    }

    public void UpdateHealthBar()
    {
        if (stats == null || stats.maxHealth <= 0) return;

        else if (healthBar != null)
        {
            float healthNormalized = stats.currentHealth / stats.maxHealth;
            healthBar.SetValueWithoutNotify(healthNormalized);
        }
    }
    #endregion

    #region Private Methods
    private void SetupHealthUI()
    {
        canvas ??= FindObjectOfType<Canvas>();

        Debug.Log($"[EnemyHealth] {gameObject.name}: Setting up health UI. BossBehaviour: {(bossBehaviour != null ? "Yes" : "No")}, BossBarPrefab: {(bossBarUIPrefab != null ? "Yes" : "No")}, Canvas: {(canvas != null ? "Yes" : "No")}");
        Debug.Log($"[EnemyHealth] {gameObject.name}: Current health: {stats.currentHealth}, Max health: {stats.maxHealth}");

        if (healthBar != null)
        {
            healthBar.minValue = 0f;
            healthBar.maxValue = 1f;
            healthBar.value = 1f;
        }

        // Setup boss bar if this is a boss
        if (bossBehaviour != null && bossBarUIPrefab != null && canvas != null)
        {
            Debug.Log($"[EnemyHealth] {gameObject.name}: Conditions met for boss bar setup. Creating boss bar UI...");
            SetupBossBarUI();
        }
        else
        {
            Debug.Log($"[EnemyHealth] {gameObject.name}: Skipping boss bar - BossBehaviour: {(bossBehaviour != null ? "Yes" : "No")}, BossBarPrefab: {(bossBarUIPrefab != null ? "Yes" : "No")}, Canvas: {(canvas != null ? "Yes" : "No")}");
        }
    }

    private void SetupBossBarUI()
    {
        if (isBossBarSetup)
        {
            Debug.Log($"[EnemyHealth] {gameObject.name}: Boss bar already set up, skipping");
            return;
        }

        Debug.Log($"[EnemyHealth] {gameObject.name}: Entering SetupBossBarUI()");

        if (stats.baseStats is EnemyBaseBossStatsSO bossStats)
        {
            // Find the main UI Canvas
            Canvas mainCanvas = FindMainUICanvas();
            if (mainCanvas == null)
            {
                Debug.LogError("[EnemyHealth] No ScreenSpace Canvas found in scene!");
                return;
            }

            Debug.Log($"[EnemyHealth] {gameObject.name}: Found Canvas: {mainCanvas.name} (RenderMode: {mainCanvas.renderMode})");

            try
            {
                // Instantiate boss bar UI on the canvas
                currentBossBarUI = Instantiate(bossBarUIPrefab, mainCanvas.transform);
                Debug.Log($"[EnemyHealth] {gameObject.name}: BossBarUI instantiated successfully!");

                // Position at top of screen
                RectTransform rectTransform = currentBossBarUI.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchorMin = new Vector2(0.5f, 1f);
                    rectTransform.anchorMax = new Vector2(0.5f, 1f);
                    rectTransform.pivot = new Vector2(0.5f, 1f);
                    rectTransform.anchoredPosition = new Vector2(0f, -50f);
                    rectTransform.sizeDelta = new Vector2(600f, 80f);
                    Debug.Log($"[EnemyHealth] {gameObject.name}: Positioned boss bar at top of screen");
                }

                // Ensure the boss bar is active
                currentBossBarUI.gameObject.SetActive(true);

                // Setup the boss bar with stats
                currentBossBarUI.SetupBossBar(bossStats);
                currentBossBarUI.SetBossName(bossStats.bossBarName);
                currentBossBarUI.SetHealth(stats.currentHealth, stats.maxHealth);
                currentBossBarUI.ShowBossBar();

                isBossBarSetup = true;
                Debug.Log($"[EnemyHealth] {gameObject.name}: Boss bar setup COMPLETE!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[EnemyHealth] {gameObject.name}: Exception during boss bar setup: {e.Message}\n{e.StackTrace}");
            }
        }
        else
        {
            Debug.LogWarning($"[EnemyHealth] {gameObject.name}: Stats is not EnemyBaseBossStatsSO! Type: {stats.baseStats?.GetType().Name ?? "null"}");
        }
    }

    private Canvas FindMainUICanvas()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas c in canvases)
        {
            // Find a ScreenSpace canvas (not WorldSpace)
            if (c.renderMode == RenderMode.ScreenSpaceOverlay ||
                c.renderMode == RenderMode.ScreenSpaceCamera)
            {
                return c;
            }
        }
        return null;
    }
    private float CalculateTakenDamage(DamageData data)
    {
        float dmg = data.amount;

        switch (data.type)
        {
            case DamageData.DamageType.Physical:
                dmg -= stats.currentArmor;
                break;
            case DamageData.DamageType.Magical:
                float resist = stats.currentMagicResistance / 100f;
                dmg *= (1 - Mathf.Clamp01(resist));
                break;
        }

        return Mathf.Max(dmg, 0f);
    }

    private void PlayDamageFlash()
    {
        if (spriteRenderer == null) return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator DamageFlashRoutine()
    {
        spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = Color.white;
        flashRoutine = null;
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            float healthNormalized = stats.currentHealth / stats.maxHealth;
            healthBar.SetValueWithoutNotify(healthNormalized);
        }

        // Update boss bar if it exists
        if (currentBossBarUI != null && stats != null)
        {
            if (stats.maxHealth > 0)
            {
                currentBossBarUI.SetHealth(stats.currentHealth, stats.maxHealth);
            }
            else
            {
                Debug.LogWarning($"[EnemyHealth] {gameObject.name}: Cannot update boss bar - maxHealth is {stats.maxHealth}");
            }
        }
    }
    #endregion
}
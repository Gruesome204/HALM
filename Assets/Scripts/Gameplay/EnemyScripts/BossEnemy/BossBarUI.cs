using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BossBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image bossPortraitImage;
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private Slider healthSlider;

    [Header("Phase UI")]
    [SerializeField] private TextMeshProUGUI phaseText;
    [SerializeField] private Image phaseIndicatorImage;
    [SerializeField] private Color[] phaseColors = new Color[] { Color.green, Color.yellow, Color.red };

    [Header("Enrage UI")]
    [SerializeField] private GameObject enrageIndicator;
    [SerializeField] private Image enrageFillImage;
    [SerializeField] private TextMeshProUGUI enrageTimerText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color enrageColor = Color.red;

    private EnemyBaseBossStatsSO bossStats;
    private float maxHealth;
    private int currentPhase;
    private bool isEnraged;
    private Coroutine enrageCoroutine;
    private Coroutine phaseTransitionCoroutine;

    // NEW: Track the previous phase to detect actual phase changes
    private int previousPhase = -1;
    // NEW: Track the current phase name to avoid unnecessary updates
    private string currentPhaseName = "";

    #region Setup

    public void SetupBossBar(EnemyBaseBossStatsSO stats)
    {
        Debug.Log($"[BossBarUI] SetupBossBar called for: {(stats != null ? stats.bossBarName : "null")}");

        bossStats = stats;

        if (stats == null)
        {
            Debug.LogWarning("[BossBarUI] Stats is null - hiding boss bar");
            HideBossBar();
            return;
        }

        SetBossName(stats.bossBarName);
        SetBossPortrait(stats.bossPortrait);
        SetHealthBarColor(stats.bossBarColor);

        currentPhase = 0;
        previousPhase = -1; // NEW: Initialize previous phase
        currentPhaseName = ""; // NEW: Initialize phase name
        SetupPhaseUI();
        SetupEnrage();

        // Ensure the GameObject is active
        gameObject.SetActive(true);
        Debug.Log($"[BossBarUI] GameObject active: {gameObject.activeSelf}");

        // Check Canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            Debug.Log($"[BossBarUI] Canvas found: {canvas.name}, Render Mode: {canvas.renderMode}");
        }
        else
        {
            Debug.LogError("[BossBarUI] No Canvas found in parent hierarchy!");
        }

        ShowBossBar();
        Debug.Log($"[BossBarUI] Setup complete for: {stats.bossBarName}");
    }

    public void SetBossName(string name)
    {
        Debug.Log($"[BossBarUI] SetBossName called with: '{name}'");

        if (bossNameText == null)
        {
            Debug.LogError("[BossBarUI] bossNameText is NULL! Check the Inspector assignment.");
            return;
        }

        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("[BossBarUI] Name is null or empty - setting default.");
            bossNameText.text = "BOSS";
        }
        else
        {
            bossNameText.text = name;
            Debug.Log($"[BossBarUI] Name set to: '{bossNameText.text}'");
        }

        Debug.Log($"[BossBarUI] Final name displayed: '{bossNameText.text}'");
    }

    private void SetBossPortrait(Sprite portrait)
    {
        if (bossPortraitImage != null)
            bossPortraitImage.sprite = portrait;
    }

    private void SetHealthBarColor(Color color)
    {
        if (healthSlider?.fillRect != null)
        {
            Image fillImage = healthSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
                fillImage.color = color;
        }
    }

    #endregion

    #region Phase Management

    private void SetupPhaseUI()
    {
        if (bossStats == null) return;

        // Get the number of phases from the phaseConfigs array
        int numberOfPhases = bossStats.phaseConfigs?.Length ?? 0;
        bool isMultiPhase = bossStats.isMultiStageBoss && numberOfPhases > 1;

        if (phaseText != null)
        {
            phaseText.gameObject.SetActive(isMultiPhase);
            if (isMultiPhase)
                phaseText.text = $"Phase {currentPhase + 1}/{numberOfPhases}";
        }

        if (phaseIndicatorImage != null && phaseColors.Length > 0)
        {
            int index = Mathf.Min(currentPhase, phaseColors.Length - 1);
            phaseIndicatorImage.color = phaseColors[index];
        }

        // MODIFIED: Only update boss name if this is an actual phase change
        if (isMultiPhase && bossStats.phaseConfigs != null && currentPhase < bossStats.phaseConfigs.Length)
        {
            var phaseConfig = bossStats.phaseConfigs[currentPhase];
            if (phaseConfig != null)
            {
                // Only update if phase actually changed
                if (currentPhase != previousPhase)
                {
                    UpdateBossNameForPhase(phaseConfig.phaseName);
                    previousPhase = currentPhase;
                }
            }
        }
    }

    private void CheckPhaseTransition(float healthPercent)
    {
        if (bossStats?.phaseConfigs == null || bossStats.phaseConfigs.Length == 0)
            return;

        // Check from highest threshold to lowest
        for (int i = bossStats.phaseConfigs.Length - 1; i >= 0; i--)
        {
            float threshold = bossStats.phaseConfigs[i].healthThreshold;
            if (healthPercent <= threshold && i != currentPhase)
            {
                // MODIFIED: Store the new phase before changing
                int newPhase = i;

                // Only proceed if this is actually a new phase
                if (newPhase != currentPhase)
                {
                    currentPhase = newPhase;
                    SetupPhaseUI();

                    // Update health bar color for the new phase
                    if (phaseColors.Length > 0 && healthSlider?.fillRect != null)
                    {
                        Image fillImage = healthSlider.fillRect.GetComponent<Image>();
                        if (fillImage != null)
                        {
                            int index = Mathf.Min(currentPhase, phaseColors.Length - 1);
                            fillImage.color = phaseColors[index];
                        }
                    }
                }
                break;
            }
        }
    }

    public void ForcePhaseChange(int phaseIndex)
    {
        if (bossStats == null || !bossStats.isMultiStageBoss) return;

        int numberOfPhases = bossStats.phaseConfigs?.Length ?? 0;
        int newPhase = Mathf.Clamp(phaseIndex, 0, numberOfPhases - 1);

        // MODIFIED: Only update if phase actually changes
        if (newPhase != currentPhase)
        {
            currentPhase = newPhase;
            SetupPhaseUI();

            if (phaseColors.Length > 0 && healthSlider?.fillRect != null)
            {
                Image fillImage = healthSlider.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    int index = Mathf.Min(currentPhase, phaseColors.Length - 1);
                    fillImage.color = phaseColors[index];
                }
            }
        }
    }

    /// <summary>
    /// Shows a phase change notification in the UI and updates the boss name
    /// </summary>
    public void ShowPhaseChange(string phaseName, float healthThreshold)
    {
        // MODIFIED: Only update if the phase name has actually changed
        if (!string.IsNullOrEmpty(phaseName) && phaseName != currentPhaseName)
        {
            // Update phase info with the new name
            SetPhaseInfo(phaseName, healthThreshold);
            currentPhaseName = phaseName;

            // Optionally, you can add visual effects for phase change
            // For example, a flash effect or animation
            StartCoroutine(PhaseChangeAnimation());
        }
    }

    private IEnumerator PhaseChangeAnimation()
    {
        // Example: Flash the boss name text
        if (bossNameText != null)
        {
            Color originalColor = bossNameText.color;
            bossNameText.color = Color.yellow;
            yield return new WaitForSeconds(0.3f);
            bossNameText.color = originalColor;
        }
    }


    /// <summary>
    /// Sets the current phase information in the UI
    /// </summary>
    public void SetPhaseInfo(string phaseName, float healthThreshold)
    {
        // MODIFIED: Store the new phase name
        currentPhaseName = phaseName;

        // Update the phase text if it exists
        if (phaseText != null)
        {
            int numberOfPhases = bossStats?.phaseConfigs?.Length ?? 0;
            phaseText.text = $"Phase {currentPhase + 1}: {phaseName}";
        }

        // Update the phase indicator color
        if (phaseIndicatorImage != null && phaseColors.Length > 0)
        {
            int index = Mathf.Min(currentPhase, phaseColors.Length - 1);
            phaseIndicatorImage.color = phaseColors[index];
        }

        // Update the health bar color based on the phase
        if (healthSlider?.fillRect != null && phaseColors.Length > 0)
        {
            Image fillImage = healthSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                int index = Mathf.Min(currentPhase, phaseColors.Length - 1);
                fillImage.color = phaseColors[index];
            }
        }

        // MODIFIED: Only update boss name if phase name has changed
        UpdateBossNameForPhase(phaseName);
    }

    #endregion

    #region Health

    public void SetHealth(float currentHealth, float maxHealth)
    {
        this.maxHealth = maxHealth;
        UpdateHealthUI(currentHealth);
    }

    public void SetHealth(float currentHealth)
    {
        if (maxHealth > 0)
            UpdateHealthUI(currentHealth);
    }

    private void UpdateHealthUI(float currentHealth)
    {
        if (healthSlider == null)
        {
            Debug.LogWarning("[BossBarUI] healthSlider is null!");
            return;
        }

        // Make sure maxHealth is set
        if (maxHealth <= 0)
        {
            Debug.LogWarning($"[BossBarUI] maxHealth is {maxHealth}, cannot calculate percentage");
            return;
        }

        float healthPercent = Mathf.Clamp01(currentHealth / maxHealth);
        healthSlider.value = healthPercent;

        Debug.Log($"[BossBarUI] Health updated: {currentHealth}/{maxHealth} = {healthPercent:P0}");

        if (bossStats?.isMultiStageBoss == true)
            CheckPhaseTransition(healthPercent);
    }

    #endregion

    #region Enrage

    private void SetupEnrage()
    {
        if (bossStats == null) return;

        bool hasEnrage = bossStats.enrageTimer > 0;

        if (enrageIndicator != null)
            enrageIndicator.SetActive(hasEnrage);

        if (hasEnrage)
        {
            StopEnrageCoroutine();
            enrageCoroutine = StartCoroutine(EnrageTimerRoutine());
        }
    }

    private IEnumerator EnrageTimerRoutine()
    {
        float elapsed = 0f;
        isEnraged = false;

        while (elapsed < bossStats.enrageTimer && !isEnraged)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / bossStats.enrageTimer;

            if (enrageFillImage != null)
                enrageFillImage.fillAmount = progress;

            if (enrageTimerText != null)
            {
                float remaining = bossStats.enrageTimer - elapsed;
                enrageTimerText.text = remaining > 0 ? $"{remaining:F1}s" : "ENRAGED!";
            }

            yield return null;
        }

        TriggerEnrage();
    }

    private void TriggerEnrage()
    {
        isEnraged = true;

        if (enrageFillImage != null)
            enrageFillImage.color = enrageColor;

        if (enrageTimerText != null)
            enrageTimerText.text = "ENRAGED!";

        SetHealthBarColor(enrageColor);
    }

    public void ResetEnrage()
    {
        isEnraged = false;
        StopEnrageCoroutine();

        if (bossStats != null && bossStats.enrageTimer > 0)
            enrageCoroutine = StartCoroutine(EnrageTimerRoutine());
    }

    private void StopEnrageCoroutine()
    {
        if (enrageCoroutine != null)
        {
            StopCoroutine(enrageCoroutine);
            enrageCoroutine = null;
        }
    }

    #endregion

    #region Visibility

    public void ShowBossBar()
    {
        Debug.Log("[BossBarUI] ShowBossBar called");
        gameObject.SetActive(true);

        if (bossStats != null && bossStats.enrageTimer > 0 && !isEnraged)
        {
            StopEnrageCoroutine();
            enrageCoroutine = StartCoroutine(EnrageTimerRoutine());
        }
    }

    public void HideBossBar()
    {
        Debug.Log("[BossBarUI] HideBossBar called");
        gameObject.SetActive(false);
        StopEnrageCoroutine();
    }

    /// <summary>
    /// Updates the boss name for a specific phase
    /// </summary>
    /// <param name="phaseName">The name of the current phase</param>
    /// <param name="customBossName">Optional custom boss name for the phase</param>
    public void UpdateBossNameForPhase(string phaseName, string customBossName = null)
    {
        if (bossNameText == null || bossStats == null)
            return;

        // MODIFIED: Only update if the phase name has changed
        if (string.IsNullOrEmpty(phaseName) || phaseName == currentPhaseName)
            return;

        // If custom name is provided, use it
        if (!string.IsNullOrEmpty(customBossName))
        {
            bossNameText.text = customBossName;
            currentPhaseName = phaseName;
            return;
        }

        // If phase name is provided and not empty, show boss name with phase
        if (!string.IsNullOrEmpty(phaseName))
        {
            bossNameText.text = $"{bossStats.bossBarName}\n({phaseName})";
            currentPhaseName = phaseName;
        }
        else
        {
            // If no phase name, just show the boss name
            bossNameText.text = bossStats.bossBarName;
        }
    }

    public void ResetBossName()
    {
        if (bossNameText == null || bossStats == null)
            return;

        bossNameText.text = bossStats.bossBarName;
        currentPhaseName = "";
    }

    #endregion

    #region Getters

    public bool IsEnraged() => isEnraged;
    public int GetCurrentPhase() => currentPhase;

    #endregion
}
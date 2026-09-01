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
    [SerializeField] private GameObject bossBarPanel;

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
        SetupPhaseUI();
        SetupEnrage();

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

        // Verify the text was actually set
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

        bool isMultiPhase = bossStats.isMultiStageBoss && bossStats.numberOfPhases > 1;

        if (phaseText != null)
        {
            phaseText.gameObject.SetActive(isMultiPhase);
            if (isMultiPhase)
                phaseText.text = $"Phase {currentPhase + 1}/{bossStats.numberOfPhases}";
        }

        if (phaseIndicatorImage != null && phaseColors.Length > 0)
        {
            int index = Mathf.Min(currentPhase, phaseColors.Length - 1);
            phaseIndicatorImage.color = phaseColors[index];
        }
    }

    private void CheckPhaseTransition(float healthPercent)
    {
        if (bossStats?.phaseThresholds == null || bossStats.phaseThresholds.Length == 0)
            return;

        for (int i = bossStats.phaseThresholds.Length - 1; i >= 0; i--)
        {
            if (healthPercent <= bossStats.phaseThresholds[i] && i != currentPhase)
            {
                currentPhase = i;
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
                break;
            }
        }
    }

    public void ForcePhaseChange(int phaseIndex)
    {
        if (bossStats == null || !bossStats.isMultiStageBoss) return;

        currentPhase = Mathf.Clamp(phaseIndex, 0, bossStats.numberOfPhases - 1);
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

    #endregion

    #region Health

    public void SetHealth(float currentHealth, float maxHealth)
    {
        Debug.Log($"[BossBarUI] SetHealth called: {currentHealth}/{maxHealth}");
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

        float healthPercent = currentHealth / maxHealth;
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
        if (bossBarPanel != null)
            bossBarPanel.SetActive(true);

        if (bossStats != null && bossStats.enrageTimer > 0 && !isEnraged)
        {
            StopEnrageCoroutine();
            enrageCoroutine = StartCoroutine(EnrageTimerRoutine());
        }
    }

    public void HideBossBar()
    {
        Debug.Log("[BossBarUI] HideBossBar called");
        if (bossBarPanel != null)
            bossBarPanel.SetActive(false);

        StopEnrageCoroutine();
    }

    #endregion

    #region Getters

    public bool IsEnraged() => isEnraged;
    public int GetCurrentPhase() => currentPhase;

    #endregion
}
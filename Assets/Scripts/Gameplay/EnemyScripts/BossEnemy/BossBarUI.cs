using UnityEngine;
using UnityEngine.UI;

public class BossBarUI : MonoBehaviour
{
    [Header("Boss Bar References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text bossNameText;
    [SerializeField] private Text bossPhaseText;
    [SerializeField] private Image bossPortraitImage;
    [SerializeField] private Image bossBarBackground;
    [SerializeField] private GameObject bossBarContainer;

    private EnemyBaseBossStatsSO currentBossStats;
    private float currentHealth;
    private float maxHealth;

    public void SetupBossBar(EnemyBaseBossStatsSO bossStats)
    {
        currentBossStats = bossStats;

        if (bossNameText != null)
            bossNameText.text = bossStats.bossBarName;

        if (bossPortraitImage != null && bossStats.bossPortrait != null)
            bossPortraitImage.sprite = bossStats.bossPortrait;

        if (bossBarBackground != null)
            bossBarBackground.color = bossStats.bossBarColor;

        // Set up phase info if multi-stage
        if (bossStats.isMultiStageBoss && bossStats.phases != null && bossStats.phases.Length > 0)
        {
            UpdatePhaseUI(0); // Start at phase 1
        }
    }

    public void SetHealth(float currentHealth, float maxHealth)
    {
        this.currentHealth = currentHealth;
        this.maxHealth = maxHealth;

        if (healthSlider != null)
            healthSlider.value = currentHealth / maxHealth;
    }

    public void ShowBossBar()
    {
        if (bossBarContainer != null)
            bossBarContainer.SetActive(true);
    }

    public void HideBossBar()
    {
        if (bossBarContainer != null)
            bossBarContainer.SetActive(false);
    }

    public void ForcePhaseChange(int phaseIndex)
    {
        UpdatePhaseUI(phaseIndex);
    }

    private void UpdatePhaseUI(int phaseIndex)
    {
        if (currentBossStats == null || !currentBossStats.isMultiStageBoss)
            return;

        if (currentBossStats.phases != null && phaseIndex < currentBossStats.phases.Length)
        {
            var phase = currentBossStats.phases[phaseIndex];

            if (bossPhaseText != null)
                bossPhaseText.text = phase.phaseName;

            // Optionally update color
            if (bossBarBackground != null)
                bossBarBackground.color = phase.phaseColor;
        }
    }

    // Legacy compatibility method - remove if not needed
    public void UpdateBossBar(float currentHealth, float maxHealth, int phaseIndex)
    {
        SetHealth(currentHealth, maxHealth);
        ForcePhaseChange(phaseIndex);
    }
}
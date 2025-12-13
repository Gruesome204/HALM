using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider healthSlider;           // Use a Slider like normal enemies
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private Image bossPortraitImage;

    [Header("Follow Settings")]
    public Transform target;                                // Boss to follow
    public Vector3 offset = new Vector3(0, 2f, 0);         // Offset above boss
    private Camera mainCamera;

    private float maxHealth;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;   // We'll use normalized health
            healthSlider.value = 1f;
        }
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(target.position + offset);
            transform.position = screenPos;
        }
    }

    // Call this to initialize the boss bar
    public void SetupBossBar(EnemyBaseStats bossStats)
    {
        if (bossStats == null) return;

        // Set health first
        maxHealth = bossStats.baseMaxHealth;
        SetHealthInstant(maxHealth);

        // Name
        if (bossNameText != null) {
            bossNameText.text = base.name;
        }

        // Portrait
        if (bossPortraitImage != null && bossStats.bossPortrait != null)
            bossPortraitImage.sprite = bossStats.bossPortrait;

        // Health bar color
        if (healthSlider != null)
            healthSlider.fillRect.GetComponent<Image>().color = bossStats.bossBarColor;

        // Multi-stage bosses (optional)
        if (bossStats.isMultiStageBoss)
        {
            // Example: show phase indicators if you have a UI for phases
        }

        // Set health
        maxHealth = bossStats.baseMaxHealth;
        SetHealth(maxHealth);

    }

    public void SetHealth(float currentHealth)
    {
        if (healthSlider == null || maxHealth <= 0) return;

        float normalized = Mathf.Clamp01(currentHealth / maxHealth);

        // Smooth transition for health like a slider
        healthSlider.value = Mathf.Lerp(healthSlider.value, normalized, Time.deltaTime * 10f);
    }
    public void SetHealthInstant(float currentHealth)
    {
        if (healthSlider == null || maxHealth <= 0) return;

        float normalized = Mathf.Clamp01(currentHealth / maxHealth);
        healthSlider.value = normalized;
    }
}

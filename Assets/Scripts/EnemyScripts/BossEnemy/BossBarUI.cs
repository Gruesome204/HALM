using NUnit.Framework.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] public Slider healthSlider;
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private Image bossPortraitImage;

    private float maxHealth;
    public void SetupBossBar(EnemyBaseStats bossStats)
    {
        Debug.Log($"SetupBossBar");
        if (bossStats == null)
        {
            Debug.LogError("BossBarUI: bossStats is null!");
            return;
        }

        maxHealth = bossStats.baseMaxHealth;
        

        if (bossNameText != null)
            bossNameText.text = bossStats.baseName;

        if (bossPortraitImage != null)
            bossPortraitImage.sprite = bossStats.bossPortrait;

        SetHealth(maxHealth);
        SetBossName(bossNameText.text);
        Debug.Log($"{bossNameText}");
    }
    public void SetHealth(float currentHealth)
    {
        if (maxHealth <= 0) return;

        healthSlider.SetValueWithoutNotify(
            currentHealth / maxHealth
        );
    }

    public void SetBossName(string newName)
    {
        if (bossNameText != null)
            bossNameText.text = newName;
    }


}

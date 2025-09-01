using UnityEngine;

public class TurretLevelBehaviour : MonoBehaviour
{
    public int currentLevel = 1;
    public int maxLevel = 10;
    public float currentXP = 0;
    public float xpToNextLevel = 50f;
    public float xpGrowthMultiplier = 1.5f;

    public float levelDamage = 10f;
    public float levelFireRate = 1f;
    public float levelRange = 5f;
    public void AddXP(float amount)
    {
        if (currentLevel >= maxLevel) return;

        currentXP += amount;

        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }
    private void LevelUp()
    {
        currentLevel++;
        currentXP = 0;
        xpToNextLevel *= xpGrowthMultiplier;

        // Upgrade stats on level up
        levelDamage *= 1.2f;    // +20% damage
        levelFireRate *= 0.9f;  // Faster fire rate
        levelRange += 0.5f;     // Slightly bigger range

        Debug.Log($"Turret leveled up to {currentLevel}! Damage: {levelDamage}, FireRate: {levelFireRate}, Range: {levelRange}");
    }
}

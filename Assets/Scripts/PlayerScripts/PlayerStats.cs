using UnityEngine;

[System.Serializable]
public class Stat
{
    [SerializeField] private float baseValue;
    private float modifier = 0f;

    public float Value => baseValue + modifier;

    public void SetBase(float value) => baseValue = value;
    public void AddModifier(float value) => modifier += value;
    public void RemoveModifier(float value) => modifier -= value;
}

public class PlayerStats : MonoBehaviour
{
    [Header("Core Stats")]
    public Stat maxHealth = new Stat();
    public Stat attackDamage = new Stat();
    public Stat defense = new Stat();
    public Stat moveSpeed = new Stat();

    [Header("Runtime Values")]
    [SerializeField] private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth.Value;
    }

    #region Health Methods
    public void TakeDamage(float amount)
    {
        float damageTaken = Mathf.Max(0, amount - defense.Value);
        currentHealth -= damageTaken;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth.Value);

        Debug.Log($"Player took {damageTaken} damage. HP: {currentHealth}/{maxHealth.Value}");

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth.Value);
        Debug.Log($"Player healed {amount}. HP: {currentHealth}/{maxHealth.Value}");
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        // TODO: Trigger respawn, game over, etc.
    }
    #endregion

    #region Upgrade Methods
    public void ApplyUpgrade(string statName, float value)
    {
        switch (statName)
        {
            case "Health": maxHealth.AddModifier(value); break;
            case "Damage": attackDamage.AddModifier(value); break;
            case "Defense": defense.AddModifier(value); break;
            case "Speed": moveSpeed.AddModifier(value); break;
            default: Debug.LogWarning("Unknown stat upgrade: " + statName); break;
        }

        Debug.Log($"Applied upgrade: {statName} +{value}");
    }
    #endregion
}

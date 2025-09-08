using Unity.Collections;
using UnityEngine;

public enum StatType
{
    Health,
    Damage,
    Defense,
    Speed
}

[System.Serializable]
public class Stat
{
    [SerializeField] private float baseValue;
    [SerializeField, ReadOnly] private float modifier = 0f; // custom attribute or ignore in inspector

    public float Value => baseValue + modifier;

    public void SetBase(float value) => baseValue = value;
    public void AddModifier(float value) => modifier += value;
    public void RemoveModifier(float value) => modifier -= value;
    public void ResetModifiers() => modifier = 0f;

    public override string ToString() => $"Base: {baseValue}, Mod: {modifier}, Total: {Value}";
}

public class PlayerStats : MonoBehaviour
{
    [Header("Core Stats")]
    public Stat maxHealth = new Stat();
    public Stat attackDamage = new Stat();
    public Stat defense = new Stat();
    public Stat moveSpeed = new Stat();

    [Header("Runtime")]
    [SerializeField, ReadOnly] private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth.Value;
    }

    #region Public Access
    public float GetStatValue(StatType type)
    {
        return type switch
        {
            StatType.Health => maxHealth.Value,
            StatType.Damage => attackDamage.Value,
            StatType.Defense => defense.Value,
            StatType.Speed => moveSpeed.Value,
            _ => 0f
        };
    }

    public void ModifyStat(StatType type, float amount)
    {
        switch (type)
        {
            case StatType.Health: maxHealth.AddModifier(amount); break;
            case StatType.Damage: attackDamage.AddModifier(amount); break;
            case StatType.Defense: defense.AddModifier(amount); break;
            case StatType.Speed: moveSpeed.AddModifier(amount); break;
        }

        Debug.Log($"[Stats] {type} modified by {amount}. Now {GetStatValue(type)}");
    }
    #endregion

    #region Health
    public void TakeDamage(float amount)
    {
        float damageTaken = Mathf.Max(0, amount - defense.Value);
        currentHealth -= damageTaken;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth.Value);

        Debug.Log($"[Stats] Took {damageTaken} damage. HP: {currentHealth}/{maxHealth.Value}");

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth.Value);
        Debug.Log($"[Stats] Healed {amount}. HP: {currentHealth}/{maxHealth.Value}");
    }

    private void Die()
    {
        Debug.Log("[Stats] Player has died!");
        // TODO: respawn / game over
    }
    #endregion
}

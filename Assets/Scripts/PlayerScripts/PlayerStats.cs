using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    public float baseHealth = 100f;
    public float baseRegen = 1.0f;
    public float baseArmor = 0f;
    public float baseMagicResistance = 0f;
    public float moveSpeed = 5f;

    [Header("Runtime Values (Read-only)")]
     public float currentHealth;
     public float currentMaxHealth;
     public float currentRegen;
     public float currentArmor;
     public float currentMagicResistance;
    public float currentMoveSpeed;

    /// <summary>
    /// All currently applied modifiers.
    /// </summary>
    private List<BuildMasterModifier.Modifier> appliedModifiers = new List<BuildMasterModifier.Modifier>();

    public void Initialize()
    {
        appliedModifiers.Clear();

        RecalculateStats(fullReheal: true);
    }

    public void AddModifier(BuildMasterModifier.Modifier modifier)
    {
        appliedModifiers.Add(modifier);
        RecalculateStats(fullReheal: true);
    }

    public void RemoveModifier(BuildMasterModifier.Modifier modifier)
    {
        appliedModifiers.Remove(modifier);
        RecalculateStats();
    }


    /// <summary>
    /// Clears all modifiers (useful when resetting the player).
    /// </summary>
    public void ClearModifiers()
    {
        appliedModifiers.Clear();
        RecalculateStats(fullReheal: true);
    }
    private void RecalculateStats(bool fullReheal = false)
    {
        // Start from base stats
        currentMaxHealth = baseHealth;
        currentRegen = baseRegen;
        currentArmor = baseArmor;
        currentMagicResistance = baseMagicResistance;
        // Apply all modifiers
        foreach (var mod in appliedModifiers)
        {
            currentMaxHealth += mod.additionalStats.maxHealth;
            currentArmor += mod.additionalStats.armor;
            currentMoveSpeed += mod.additionalStats.movementSpeed;
}

        // Adjust current health
        if (fullReheal)
        {
            currentHealth = currentMaxHealth;
        }
        else
        {
            if (currentHealth > currentMaxHealth)
                currentHealth = currentMaxHealth;
        }
    }

    /// <summary>
    /// Returns a read-only list for UI or debugging.
    /// </summary>
    public IReadOnlyList<BuildMasterModifier.Modifier> GetAppliedModifiers()
    {
        return appliedModifiers.AsReadOnly();
    }

}

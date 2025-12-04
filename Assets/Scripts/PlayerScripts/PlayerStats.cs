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

    public void Initialize()
    {
        currentMaxHealth = baseHealth;
        currentRegen = baseRegen;
        currentHealth = baseHealth;
        currentArmor = baseArmor;
        currentMagicResistance = baseMagicResistance;
    }
}

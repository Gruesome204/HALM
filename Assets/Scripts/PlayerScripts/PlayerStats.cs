using UnityEngine;

[System.Serializable]
public class PlayerStats : MonoBehaviour
{
    public float baseHealth = 100f;
    public float baseArmor = 5f;
    public float baseMagicResistance = 10f;
    public float moveSpeed = 5f;

    [HideInInspector] public float currentHealth;
    [HideInInspector] public float currentMaxHealth;
    [HideInInspector] public float currentArmor;
    [HideInInspector] public float currentMagicResistance;

    public void Initialize()
    {
        currentMaxHealth = baseHealth;
        currentHealth = baseHealth;
        currentArmor = baseArmor;
        currentMagicResistance = baseMagicResistance;
    }
}

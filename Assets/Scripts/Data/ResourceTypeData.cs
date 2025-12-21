using System;
using UnityEngine;
public enum ResourceType
{
    Currency,
    Wood,
    Stone,
    Metal,
    Pulver
}

[System.Serializable]
public struct ResourceCost
{
    public ResourceType resourceType;
    public int amount;
}


public class ResourceTypeData : MonoBehaviour
{
    public ResourceType resourceType;

    [System.Serializable]
    public struct ResourceDrop
    {
        public ResourceType resourceType;

        public int minAmount;
        public int maxAmount;

        [Range(0f, 1f)]
        public float dropChance; // 0.0 = 0%, 1.0 = 100%
    }

    public static event Action<ResourceType, int> OnResourceDropped;

    public static void TryDrop(ResourceDrop drop)
    {
        float roll = UnityEngine.Random.Range(0f, 1f);
        if (roll <= drop.dropChance)
        {
            int amount = UnityEngine.Random.Range(drop.minAmount, drop.maxAmount + 1);
            if (amount > 0)
            {
                OnResourceDropped?.Invoke(drop.resourceType, amount);
                Debug.Log($"Dropped {amount} of {drop.resourceType}");
            }
        }
        else
        {
            Debug.Log($"{drop.resourceType} did not drop (roll: {roll})");
        }
    }

}

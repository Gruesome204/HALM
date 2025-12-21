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

[Serializable]
public struct ResourceCost
{
    public ResourceType resourceType;
    public int amount;
}

public static class ResourceDropper
{
    [Serializable]
    public struct ResourceDrop
    {
        public ResourceType resourceType;
        public int minAmount;
        public int maxAmount;
        [Range(0f, 1f)]
        public float dropChance; // 0 = 0%, 1 = 100%
    }

    public static event Action<ResourceType, int> OnResourceDropped;

    /// <summary>
    /// Try dropping a single resource.
    /// </summary>
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

    /// <summary>
    /// Try dropping multiple resources from an array or list.
    /// </summary>
    public static void TryDropAll(ResourceDrop[] drops)
    {
        if (drops == null || drops.Length == 0) return;

        foreach (var drop in drops)
        {
            TryDrop(drop);
        }
    }

    /// <summary>
    /// Overload to accept a List<ResourceDrop>
    /// </summary>
    public static void TryDropAll(System.Collections.Generic.List<ResourceDrop> drops)
    {
        if (drops == null || drops.Count == 0) return;

        foreach (var drop in drops)
        {
            TryDrop(drop);
        }
    }
}

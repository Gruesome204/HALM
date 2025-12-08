using UnityEngine;

public class ResourceTypeData : MonoBehaviour
{
    public ResourceType resourceType;


    public enum ResourceType
    {
        WoodResource,
        StoneResource,
        MetalResource,
        PulverResource
    }

    [System.Serializable]
    public struct ResourceDrop
    {
        public ResourceTypeData.ResourceType resourceType;

        public int minAmount;
        public int maxAmount;

        [Range(0f, 1f)]
        public float dropChance; // 0.0 = 0%, 1.0 = 100%
    }

}

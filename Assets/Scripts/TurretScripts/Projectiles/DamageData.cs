using UnityEngine;
public struct DamageData
{
    public float amount;
    public GameObject source;
    public DamageType type;

    public DamageData(float amount, GameObject source, DamageType type)
    {
        this.amount = amount;
        this.source = source;
        this.type = type;
    }
    public enum DamageType
    {
        Physical,
        Magical,
    }
}

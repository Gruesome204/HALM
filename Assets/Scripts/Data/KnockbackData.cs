using UnityEngine;
public struct KnockbackData
{
    public float knockbackStrength;
    public float knockbackDuration;
    public Vector2 direction;

    public KnockbackData(float knockbackStrength, float knockbackDuration, Vector2 direction)
    {
        this.knockbackStrength = knockbackStrength;
        this.knockbackDuration = knockbackDuration;
        this.direction = direction;
    }
}

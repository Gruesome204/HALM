using UnityEngine;

[System.Serializable]
public class ActiveStatus
{
    public string name;
    public float duration;
    public float remainingTime;

    public ActiveStatus(string name, float duration)
    {
        this.name = name;
        this.duration = duration;
        this.remainingTime = duration;
    }
}

using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy/Effects/Status")]
public class StatusEffect : AbilityEffect
{
    public string statusName;
    public float duration;

    public override void Apply(GameObject user, GameObject target)
    {
    //    if (target == null) return;

    //    var statusHandler = target.GetComponent<StatusHandler>();
    //    if (statusHandler != null)
    //    {
    //        statusHandler.ApplyStatus(statusName, duration);
    //    }

    //    Debug.Log($"{target.name} is afflicted with {statusName} for {duration} seconds");
    }
}

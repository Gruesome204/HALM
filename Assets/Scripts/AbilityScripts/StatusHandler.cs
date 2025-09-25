using UnityEngine;
using System.Collections.Generic;

public class StatusHandler : MonoBehaviour
{
    //private class ActiveStatus
    //{
    //    public Stat status;
    //    public float remainingTime;
    //    public float tickTimer;
    //}

    //private List<ActiveStatus> activeStatuses = new();

    //public void ApplyStatus(Status status, float duration)
    //{
    //    var existing = activeStatuses.Find(s => s.status == status);
    //    if (existing != null)
    //    {
    //        existing.remainingTime = duration; // refresh duration
    //    }
    //    else
    //    {
    //        var newStatus = new ActiveStatus
    //        {
    //            status = status,
    //            remainingTime = duration,
    //            tickTimer = status.tickInterval
    //        };
    //        activeStatuses.Add(newStatus);
    //        status.OnApply(gameObject);
    //    }
    //}

    //private void Update()
    //{
    //    for (int i = activeStatuses.Count - 1; i >= 0; i--)
    //    {
    //        var s = activeStatuses[i];
    //        s.remainingTime -= Time.deltaTime;

    //        // Handle damage over time (DoT)
    //        if (s.status.tickDamage > 0)
    //        {
    //            s.tickTimer -= Time.deltaTime;
    //            if (s.tickTimer <= 0f)
    //            {
    //                var health = GetComponent<IDamagable>();
    //                if (health != null)
    //                {
    //                    health.TakeDamage(new DamageData(
    //                        s.status.tickDamage,
    //                        gameObject, // source can be self or tracked
    //                        DamageData.DamageType.Magical // example type
    //                    ));
    //                }
    //                s.tickTimer = s.status.tickInterval;
    //            }
    //        }

    //        // Expire status
    //        if (s.remainingTime <= 0)
    //        {
    //            s.status.OnExpire(gameObject);
    //            activeStatuses.RemoveAt(i);
    //        }
    //    }
    //}
}

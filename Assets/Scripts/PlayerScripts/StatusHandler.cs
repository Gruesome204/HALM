using UnityEngine;
using System.Collections.Generic;

public class StatusHandler : MonoBehaviour
{
    private List<ActiveStatus> activeStatuses = new List<ActiveStatus>();

    private void Update()
    {
        for (int i = activeStatuses.Count - 1; i >= 0; i--)
        {
            activeStatuses[i].remainingTime -= Time.deltaTime;

            if (activeStatuses[i].remainingTime <= 0)
            {
                Debug.Log($"{gameObject.name} is no longer affected by {activeStatuses[i].name}");
                OnStatusExpired(activeStatuses[i]);
                activeStatuses.RemoveAt(i);
            }
        }
    }

    public void ApplyStatus(string statusName, float duration)
    {
        // If already applied, refresh duration
        var existing = activeStatuses.Find(s => s.name == statusName);
        if (existing != null)
        {
            existing.remainingTime = duration;
            Debug.Log($"{gameObject.name}'s {statusName} refreshed for {duration} seconds");
        }
        else
        {
            var newStatus = new ActiveStatus(statusName, duration);
            activeStatuses.Add(newStatus);
            Debug.Log($"{gameObject.name} gains {statusName} for {duration} seconds");
            OnStatusApplied(newStatus);
        }
    }

    private void OnStatusApplied(ActiveStatus status)
    {
        // Hook logic here (e.g., slow movement, disable attacks, etc.)
        switch (status.name.ToLower())
        {
            //case "stun":
            //    GetComponent<PlayerMovement>()?.enabled = false; // Example
            //    break;
            //case "slow":
            //    GetComponent<Movement>()?.ModifySpeed(0.5f);
            //    break;
        }
    }

    private void OnStatusExpired(ActiveStatus status)
    {
        // Undo effects
        switch (status.name.ToLower())
        {
            //case "stun":
            //    GetComponent<CharacterController>()?.enabled = true;
            //    break;
            //case "slow":
            //    GetComponent<Movement>()?.ResetSpeed();
            //    break;
        }
    }
}

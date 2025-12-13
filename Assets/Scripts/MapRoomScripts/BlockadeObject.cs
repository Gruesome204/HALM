using UnityEngine;

public class BlockadeObject : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        // Only allow interaction if the room is cleared
        if (!MapProgressionManager.Instance.roomClearedWaitingForPlayer)
            return;

        Debug.Log("[Blockade] Player clicked exit blocker.");

        MapProgressionManager.Instance.PlayerClickedExitBlocker();

        gameObject.SetActive(false);
    }
}

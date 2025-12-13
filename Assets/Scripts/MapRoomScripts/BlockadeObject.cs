using UnityEngine;

public class BlockadeObject : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Player clicked the blockade!");
        MapProgressionManager.Instance.PlayerClickedExitBlocker();
        gameObject.SetActive(false);
    }
}

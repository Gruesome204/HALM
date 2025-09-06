using UnityEngine;

public class ForgeBuilding : MonoBehaviour, IInteractable
{
    public GameObject forgeBuildingUI;

    public void Interact()
    {
        forgeBuildingUI.SetActive(true); // Open turret upgrade UI
        Debug.Log("Load Forge Test");
    }
}

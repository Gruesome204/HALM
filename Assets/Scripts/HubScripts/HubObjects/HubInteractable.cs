using UnityEngine;
using UnityEngine.SceneManagement;

public class HubInteractable : MonoBehaviour
{
    public string interactionName = "Unnamed Hub Object";

    // Event to notify listeners of an interaction
    public event System.Action<string> OnInteractedEvent;
    [Header("Glow")]
    [SerializeField] private GameObject glowObject;


    private void OnMouseEnter()
    {
        if (glowObject != null)
            glowObject.SetActive(true);
    }

    private void OnMouseExit()
    {
        if (glowObject != null)
            glowObject.SetActive(false);
    }
    private void OnMouseDown()
    {
        Interact();
    }

    public void Interact()
    {
        Debug.Log($"Interacted with {interactionName}");
        OnInteractedEvent?.Invoke(interactionName);

        if (interactionName == "DungeonEntrance" && InGameMenuManager.Instance.CheckForAnOpenMenu("one") == false)
        {
            InGameMenuManager.Instance.CloseAllMenus();
            InGameMenuManager.Instance.OpenOrCloseOneMenu("EnterDungeonDoc", true);
        }

        if (interactionName == "Markt" && InGameMenuManager.Instance.CheckForAnOpenMenu("one") == false)
        {
            InGameMenuManager.Instance.OpenOrCloseOneMenu("MarktMenuDoc", true);
        }

        if (interactionName == "Schmiede" && InGameMenuManager.Instance.CheckForAnOpenMenu("one") == false)
        {
            InGameMenuManager.Instance.OpenOrCloseOneMenu("BuildmasterModifyDoc", true);
        }

        if (interactionName == "Werkstatt" && InGameMenuManager.Instance.CheckForAnOpenMenu("one") == false)
        {
            InGameMenuManager.Instance.OpenOrCloseOneMenu("WorkshopDoc", true);
        }
    }

}

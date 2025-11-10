using UnityEngine;
using UnityEngine.SceneManagement;

public class HubInteractable : MonoBehaviour
{
    public string interactionName = "Unnamed Hub Object";

    // Event to notify listeners of an interaction
    public event System.Action<string> OnInteractedEvent;

    private void OnMouseDown()
    {
        Interact();
    }

    public void Interact()
    {
        Debug.Log($"Interacted with {interactionName}");
        OnInteractedEvent?.Invoke(interactionName);

        if (interactionName == "DungeonEntrance")
        {
            SceneManager.LoadScene("GameScene");
        }
    }

    //Activate after Implementation of HubUIController
    //private void OnEnable()
    //{
    //    // Notify a manager that this interactable exists
    //    HubUIController.RegisterInteractable(this);
    //}

    //private void OnDisable()
    //{
    //    HubUIController.UnregisterInteractable(this);
    //}
}

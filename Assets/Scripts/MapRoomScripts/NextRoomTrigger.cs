using UnityEngine;

public class NextRoomTrigger : MonoBehaviour
{
    private void Start()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Player"))
        {
            Debug.Log("[ExitTrigger] Player entered, loading next room.");
            MapProgressionManager.Instance.PlayerTriggerNextRoom();

            // Optional: disable trigger to prevent multiple triggers
            gameObject.SetActive(false);
        }
    }
}

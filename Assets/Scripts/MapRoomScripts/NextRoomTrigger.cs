using UnityEngine;

public class NextRoomTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            MapProgressionManager.Instance.PlayerTriggerNextRoom();
        }
    }
}

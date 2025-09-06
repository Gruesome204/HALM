using UnityEngine;

public class DungeonEntrance : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        // Load dungeon scene
        //UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        Debug.Log("Load Dungeon Test");
    }
}

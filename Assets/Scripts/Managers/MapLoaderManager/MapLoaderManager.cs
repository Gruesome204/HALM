using UnityEngine;

public class MapLoaderManager : MonoBehaviour
{
    public static MapLoaderManager Instance { get; private set; }

    [Header("Map Prefabs")]
    public GameObject[] mapPrefabs;

    [Header("Instances")]
    public Transform mapParent;

    private GameObject currentMap;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void LoadMap(int index)
    {
        if (index < 0 || index >= mapPrefabs.Length)
        {
            Debug.LogError("Map index out of range!");
            return;
        }

        // Destroy old map
        if (currentMap != null)
            Destroy(currentMap);

        // Load new map prefab
        currentMap = Instantiate(mapPrefabs[index], mapParent);

        // Optional: Update GridManager origin to match map
        GridManager.Instance.originPosition = currentMap.transform.position;
    }
}

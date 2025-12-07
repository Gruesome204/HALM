using UnityEngine;
using UnityEngine.Tilemaps;

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

        Tilemap[] tilemaps = currentMap.GetComponentsInChildren<Tilemap>();
        Tilemap floorTilemap = GetFloorTileMap();

    }

    public Tilemap GetFloorTileMap()
    {
        Tilemap[] tilemaps = currentMap.GetComponentsInChildren<Tilemap>();
        Tilemap floorTilemap;
        foreach (Tilemap t in tilemaps)
        {
            if (t.gameObject.name.ToLower().Contains("floor"))
            {
                floorTilemap = t;
                return floorTilemap;
            }
        }
        return null;

    }
}

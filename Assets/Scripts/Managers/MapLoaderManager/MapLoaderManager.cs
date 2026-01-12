using System;
using System.Collections.Generic;
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
    public GameObject CurrentMap => currentMap;

    [Header("Spawn Points (auto-detected)")]
    public Transform playerSpawnPoint;
    public Transform bossSpawnPoint;

    [Header("Objects(auto-detected)")]
    public List<GameObject> ExitBlockerObjects = new List<GameObject>(); // Multiple clickable blockers
    [Header("Exit Object(Auto-detected)")]
    public GameObject ExitTriggerObject; // The trigger the player can walk into

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public GameObject LoadMap(int index)
    {
        if (index < 0 || index >= mapPrefabs.Length)
        {
            Debug.LogError("Map index out of range!");
            return null;
        }

        // Destroy old map
        if (currentMap != null)
            Destroy(currentMap);

        // Load new map prefab
        currentMap = Instantiate(mapPrefabs[index], mapParent);
        AssignSpawnPointsToEnemyManager();
        ApplyMapEnemySetup();


        // Auto-assign exit objects by tag
        ExitBlockerObjects = FindAllObjectsInChildrenWithTag(currentMap, "ExitBlocker");
        ExitTriggerObject = FindObjectInChildrenWithTag(currentMap, "ExitTrigger");

        if (ExitBlockerObjects == null)
            Debug.LogWarning("ExitBlockerObject not found in the map!");
        if (ExitTriggerObject == null)
            Debug.LogWarning("ExitTriggerObject not found in the map!");

        EnemySpawnManager.Instance.ResetSpawner();
        // Send spawn points to EnemySpawnManager
        AssignSpawnPointsToEnemyManager();

        SetPlayerPosition();

        return currentMap;
    }

    private void SetPlayerPosition()
    {
        PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
        if (player != null && playerSpawnPoint != null)
            player.transform.position = playerSpawnPoint.position;
    }

    private void AssignSpawnPointsToEnemyManager()
    {
        Transform spawnRoot = currentMap.transform.Find("EnemySpawnPoints");

        if (spawnRoot == null)
        {
            Debug.LogWarning("No 'SpawnPoints' child found in map prefab!");
            EnemySpawnManager.Instance.spawnPoints = new Transform[0];
            return;
        }

        Transform[] spawnPoints = new Transform[spawnRoot.childCount];
        for (int i = 0; i < spawnRoot.childCount; i++)
        {
            spawnPoints[i] = spawnRoot.GetChild(i);
        }

        EnemySpawnManager.Instance.spawnPoints = spawnPoints;


        // Player spawn point
        Transform pSpawn = currentMap.transform.Find("PlayerSpawnPoint");

        if (pSpawn == null)
        {
            Debug.LogError("PlayerSpawnPoint missing in map prefab!");
            playerSpawnPoint = null;
        }
        else
        {
            playerSpawnPoint = pSpawn;
            Debug.Log($"Player spawn point loaded at {playerSpawnPoint.position}");
        }

        // Assign BossSpawnPoint
        bossSpawnPoint = currentMap.transform.Find("BossSpawnPoint");
        if (bossSpawnPoint == null)
            Debug.LogWarning("BossSpawnPoint missing.");
        else
            Debug.Log($"Boss Spawn = {bossSpawnPoint.position}");
    }

    private GameObject FindObjectInChildrenWithTag(GameObject parent, string tag)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.CompareTag(tag))
                return child.gameObject;
        }
        return null;
    }

    private List<GameObject> FindAllObjectsInChildrenWithTag(GameObject parent, string tag)
    {
        List<GameObject> result = new List<GameObject>();
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.CompareTag(tag))
                result.Add(child.gameObject);
        }
        return result;
    }

    private void ApplyMapEnemySetup()
    {
        if (currentMap == null) return;

        MapEnemySetup setup = currentMap.GetComponent<MapEnemySetup>();
        if (setup == null)
        {
            Debug.LogWarning("MapEnemySetup missing on map!");
            return;
        }

        EnemySpawnManager spawner = EnemySpawnManager.Instance;
        spawner.isBossRoom = setup.isBossRoom;
        if (setup.enemyPrefabs != null && setup.enemyPrefabs.Count > 0)
            spawner.enemyPrefabs = new List<GameObject>(setup.enemyPrefabs); ;

        spawner.bossPrefab = setup.bossPrefab;
    }
}

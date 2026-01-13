using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapLoaderManager : MonoBehaviour
{
    public static MapLoaderManager Instance { get; private set; }

    [Header("Map Prefabs")]
    public GameObject[] normalMapPrefabs;
    public GameObject bossMapPrefab;

    private List<GameObject> mapSequence = new List<GameObject>();
    private int currentMapIndex = -1;

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

    private GameObject LoadMapFromPrefab(GameObject prefab)
    {
        if (EnemySpawnManager.Instance == null)
        {
            Debug.LogError("EnemySpawnManager missing!");
            return null;
        }

        if (currentMap != null)
            Destroy(currentMap);

        currentMap = Instantiate(prefab, mapParent);

        ApplyMapEnemySetup();

        ExitBlockerObjects = FindAllObjectsInChildrenWithTag(currentMap, "ExitBlocker");
        ExitTriggerObject = FindObjectInChildrenWithTag(currentMap, "ExitTrigger");

        EnemySpawnManager.Instance.ResetSpawner();
        AssignSpawnPointsToEnemyManager();
        SetPlayerPosition();

        return currentMap;
    }
    public GameObject LoadNextMap()
    {
        currentMapIndex++;

        if (currentMapIndex >= mapSequence.Count)
        {
            Debug.Log("No more maps to load.");
            return null;
        }

        return LoadMapFromPrefab(mapSequence[currentMapIndex]);
    }

    public void GenerateMapSequence()
    {
        mapSequence.Clear();

        List<GameObject> shuffledMaps = new List<GameObject>(normalMapPrefabs);

        // Fisher–Yates shuffle
        for (int i = 0; i < shuffledMaps.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, shuffledMaps.Count);
            (shuffledMaps[i], shuffledMaps[r]) = (shuffledMaps[r], shuffledMaps[i]);
        }

        mapSequence.AddRange(shuffledMaps);

        if (bossMapPrefab != null)
            mapSequence.Add(bossMapPrefab);

        currentMapIndex = -1;
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
    }
}

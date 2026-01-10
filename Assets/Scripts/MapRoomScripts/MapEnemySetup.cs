using System.Collections.Generic;
using UnityEngine;

public class MapEnemySetup : MonoBehaviour
{
    [Header("Room Enemy Setup")]
    public bool isBossRoom;

    public List<GameObject> enemyPrefabs;

    [Tooltip("-1 uses default from EnemySpawnManager")]
    public int spawnAmount = -1;

    [Tooltip("-1 uses default from EnemySpawnManager")]
    public float spawnInterval = -1f;



    [Header("Boss")]
    public GameObject bossPrefab;
}

using System.Collections.Generic;
using UnityEngine;

public class MapEnemySetup : MonoBehaviour
{
    [Header("Room Enemy Setup")]
    public bool isBossRoom;

    public List<GameObject> enemyPrefabs;
    public int spawnAmountOverride = -1; // -1 = use default
    public float spawnIntervalOverride = -1f;

    [Header("Boss")]
    public GameObject bossPrefab;
}

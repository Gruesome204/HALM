using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public float spawnInterval = 2f;
    public int spawnAmount = 5;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;
    public bool useRandomSpawnPoint = true;

    private float spawnTimer = 0f;

    void Update()
    {
        if (GameManager.Instance.IsPaused()) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            TrySpawnEnemy();
        }
    }

    void TrySpawnEnemy()
    {

        if (spawnAmount <= 0) return;

        SpawnEnemy();
    }

    void SpawnEnemy()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning($"No spawn points assigned on {name}, spawning at spawner position instead.");
            SpawnAtPoint(transform.position);
            return;
        }

        // Choose which spawn point to use
        Transform chosenPoint = useRandomSpawnPoint
            ? spawnPoints[Random.Range(0, spawnPoints.Length)]
            : spawnPoints[0];

        SpawnAtPoint(chosenPoint.position);
    }

    void SpawnAtPoint(Vector3 position)
    {
        GameObject spawnedEnemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        spawnedEnemy.GetComponent<EnemyMovement>().target = FindAnyObjectByType<PlayerMovement>().gameObject;

        spawnAmount--;
    }

}
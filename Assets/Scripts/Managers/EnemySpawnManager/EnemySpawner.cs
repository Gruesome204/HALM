using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnInterval = 2f;
    private float spawnTimer = 0f;

    void Update()
    {
        if (GameManager.Instance.IsPaused()) return;
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            SpawnEnemy();
            spawnTimer = 0f;
        }
    }


    void SpawnEnemy()
    {
       GameObject spawnedEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
       spawnedEnemy.GetComponent<EnemyMovement>().target  = FindAnyObjectByType<PlayerMovement>().gameObject;   
    }
}
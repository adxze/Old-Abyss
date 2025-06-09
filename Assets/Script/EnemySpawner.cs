using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public float minSpawnTime = 5f;
    public float maxSpawnTime = 15f;
    public int maxEnemiesAlive = 5;
    public bool spawnOnStart = true;
    
    [Header("Enemy Variations")]
    public bool randomizeStats = true;
    public float healthVariation = 0.2f;  // ±20% health variation
    public float speedVariation = 0.3f;   // ±30% speed variation
    public float damageVariation = 0.15f; // ±15% damage variation
    
    private bool isSpawning = false;
    private int currentEnemyCount = 0;
    
    void Start()
    {
        if (spawnOnStart)
        {
            StartSpawning();
        }
    }
    
    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            StartCoroutine(SpawnRoutine());
        }
    }
    
    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }
    
    private IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            if (currentEnemyCount < maxEnemiesAlive)
            {
                SpawnEnemy();
                currentEnemyCount++;
            }
            
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(waitTime);
        }
    }
    
    private void SpawnEnemy()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points defined for enemy spawner!");
            return;
        }
        
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        
        EnemyController enemy = enemyObj.GetComponent<EnemyController>();
        
        if (enemy != null && randomizeStats)
        {
            float healthMod = 1 + Random.Range(-healthVariation, healthVariation);
            float speedMod = 1 + Random.Range(-speedVariation, speedVariation);
            float damageMod = 1 + Random.Range(-damageVariation, damageVariation);
            
            enemy.maxHealth *= healthMod;
            enemy.currentHealth = enemy.maxHealth;
            enemy.moveSpeed *= speedMod;
            enemy.attackDamage *= damageMod;
        }
        
        StartCoroutine(CheckEnemyDestruction(enemyObj));
    }
    
    private IEnumerator CheckEnemyDestruction(GameObject enemy)
    {
        while (enemy != null)
        {
            yield return new WaitForSeconds(1f);
        }
        
        currentEnemyCount--;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (spawnPoints != null)
        {
            Gizmos.color = Color.blue;
            
            foreach (Transform point in spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.5f);
                }
            }
        }
    }
}
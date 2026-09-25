using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject foodPrefab;
    public float spawnInterval = 2f;
    public int maxFood = 50;

    [Header("Spawn Area (Rectangular)")]
    public Vector2 areaSize = new Vector2(20, 20);

    [Header("Clustering Settings")]
    public float clusterRadius = 3f; 

    private float time = 0f;
    private Vector2 lastSpawnPos;
    private bool hasLastSpawn = false;

    public void Simulate(float h)
    {
        time += h;

        if (time >= spawnInterval)
        {
            time = 0f;

            if (CountFood() < maxFood)
            {
                SpawnFood();
            }
        }
    }

    void SpawnFood()
    {
       
        if (CountFood() == 0)
        {
            hasLastSpawn = false;
        }

        Vector2 spawnPos;

        if (!hasLastSpawn)
        {
            // Primera comida: punto aleatorio dentro de toda el área
            spawnPos = new Vector2(
                Random.Range(-areaSize.x / 2f, areaSize.x / 2f),
                Random.Range(-areaSize.y / 2f, areaSize.y / 2f)
            ) + (Vector2)transform.position;

            hasLastSpawn = true;
        }
        else
        {
          
            Vector2 randomOffset = Random.insideUnitCircle * clusterRadius;
            spawnPos = lastSpawnPos + randomOffset;

            // Restringir dentro de los límites del área para que no se salga
            float minX = transform.position.x - (areaSize.x / 2f);
            float maxX = transform.position.x + (areaSize.x / 2f);
            float minY = transform.position.y - (areaSize.y / 2f);
            float maxY = transform.position.y + (areaSize.y / 2f);

            spawnPos.x = Mathf.Clamp(spawnPos.x, minX, maxX);
            spawnPos.y = Mathf.Clamp(spawnPos.y, minY, maxY);
        }

        // Actualizar la última posición
        lastSpawnPos = spawnPos;

        Instantiate(foodPrefab, spawnPos, Quaternion.identity);
    }

    int CountFood()
    {
        return FindObjectsByType<Food>(FindObjectsSortMode.InstanceID).Length;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, areaSize.y, 1));

        if (hasLastSpawn)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(lastSpawnPos, clusterRadius);
        }
    }
}
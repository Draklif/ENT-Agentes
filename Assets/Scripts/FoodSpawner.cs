using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject foodPrefab;
    public float baseSpawnInterval = 2f; // 🔄 MODIFICADO: base
    public int baseMaxFood = 50;         // 🔄 MODIFICADO: base

    [Header("Spawn Area (Rectangular)")]
    public Vector2 areaSize = new Vector2(20, 20);

    private float time = 0f;
    private WeatherSystem weather; // ✅ NUEVO: Referencia al sistema climático

    private void Start()
    {
        weather = FindFirstObjectByType<WeatherSystem>(); // ✅ NUEVO: Buscar sistema climático
    }

    public void Simulate(float h)
    {
        time += h;

        // ✅ MODIFICADO: Ajustar según clima
        float currentSpawnInterval = baseSpawnInterval;
        int currentMaxFood = baseMaxFood;

        if (weather != null)
        {
            currentSpawnInterval /= weather.GetFoodSpawnMultiplier();
            currentMaxFood = (int)(baseMaxFood * weather.GetFoodSpawnMultiplier());
        }

        if (time >= currentSpawnInterval)
        {
            time = 0f;

            if (CountFood() < currentMaxFood)
            {
                SpawnFood();
            }
        }
    }

    void SpawnFood()
    {
        Vector2 spawnPos = new Vector2(
            Random.Range(-areaSize.x / 2f, areaSize.x / 2f),
            Random.Range(-areaSize.y / 2f, areaSize.y / 2f)
        );

        spawnPos += (Vector2)transform.position;

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
    }
}
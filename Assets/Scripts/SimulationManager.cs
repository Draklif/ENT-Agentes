using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject predatorPrefab;
    public GameObject rabbitPrefab;
    public GameObject foodPrefab;
    public GameObject climateManagerPrefab;

    [Header("Cantidad inicial")]
    public int predatorCount = 1;
    public int rabbitCount = 1;
    public int foodCount = 50;

    [Header("Área de generación")]
    public Vector2 spawnArea = new Vector2(20, 20);

    private ClimateManager climate;

    void Start()
    {
        // Instanciar clima
        GameObject cmObj = Instantiate(climateManagerPrefab);
        climate = cmObj.GetComponent<ClimateManager>();

        // Instanciar depredadores, conejos y comida
        for (int i = 0; i < predatorCount; i++)
            Spawn(predatorPrefab);
        for (int i = 0; i < rabbitCount; i++)
            Spawn(rabbitPrefab);
        for (int i = 0; i < foodCount; i++)
            Spawn(foodPrefab);
    }

    void Update()
    {
       
    }

    void Spawn(GameObject prefab)
    {
        // Genera los objetos dentro de un área centrada en el origen
        Vector3 pos = new Vector3(
            Random.Range(-spawnArea.x / 2f, spawnArea.x / 2f),
            Random.Range(-spawnArea.y / 2f, spawnArea.y / 2f),
            0
        );

        Instantiate(prefab, pos, Quaternion.identity);
    }
}


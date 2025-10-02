using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimulationManager : MonoBehaviour
{
    public float secondsPerIteration = 1.0f;
    private float time = 0f;

    public List<Bunny> bunnies = new List<Bunny>();
    public List<Predator> predators = new List<Predator>();
    public FoodSpawner foodSpawner;
    public WeatherSystem weatherSystem; // ✅ NUEVO: Referencia al sistema climático

    [Header("UI Elements")]
    public Text weatherInfoText;
    public Text populationInfoText;

    void Start()
    {
        Bunny[] foundBunnies = FindObjectsByType<Bunny>(FindObjectsSortMode.InstanceID);
        bunnies = new List<Bunny>(foundBunnies);

        Predator[] foundPredators = FindObjectsByType<Predator>(FindObjectsSortMode.InstanceID);
        predators = new List<Predator>(foundPredators);

        foodSpawner = FindFirstObjectByType<FoodSpawner>();
        weatherSystem = FindFirstObjectByType<WeatherSystem>(); // ✅ NUEVO: Buscar sistema climático
    }

    void Update()
    {
        time += Time.deltaTime;

        if (time >= secondsPerIteration)
        {
            time = 0f;
            Simulate();
        }

        UpdateUI(); // ✅ NUEVO: Actualizar UI
    }

    void Simulate()
    {
        // ✅ NUEVO: Avanzar tiempo climático
        if (weatherSystem != null)
        {
            weatherSystem.AdvanceTime(secondsPerIteration);
        }

        foreach (Bunny b in bunnies.ToArray())
        {
            if (b != null && b.isAlive)
            {
                b.Simulate(secondsPerIteration);
            }
            else
            {
                bunnies.Remove(b);
            }
        }

        foreach (Predator p in predators.ToArray())
        {
            if (p != null && p.isAlive)
            {
                p.Simulate(secondsPerIteration);
            }
            else
            {
                predators.Remove(p);
            }
        }

        if (foodSpawner != null) foodSpawner.Simulate(secondsPerIteration);

        UpdatePopulationLists(); // ✅ NUEVO: Actualizar listas
    }

    void UpdatePopulationLists()
    {
        Bunny[] currentBunnies = FindObjectsByType<Bunny>(FindObjectsSortMode.InstanceID);
        bunnies = new List<Bunny>(currentBunnies);

        Predator[] currentPredators = FindObjectsByType<Predator>(FindObjectsSortMode.InstanceID);
        predators = new List<Predator>(currentPredators);
    }

    void UpdateUI()
    {
        if (weatherInfoText != null && weatherSystem != null)
        {
            weatherInfoText.text = weatherSystem.GetWeatherInfo();
        }

        if (populationInfoText != null)
        {
            int aliveBunnies = 0;
            int alivePredators = 0;

            foreach (Bunny b in bunnies)
            {
                if (b != null && b.isAlive) aliveBunnies++;
            }

            foreach (Predator p in predators)
            {
                if (p != null && p.isAlive) alivePredators++;
            }

            populationInfoText.text = $"Conejos: {aliveBunnies} | Depredadores: {alivePredators}";
        }
    }
}
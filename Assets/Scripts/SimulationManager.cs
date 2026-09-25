using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    [Header("Bunny Limits")]
    public int maxBunnies = 20;

    [Header("Lists & Spawners")]
    public List<Bunny> bunnies = new List<Bunny>();
    public List<Predator> predators = new List<Predator>();
    public FoodSpawner foodSpawner;

    void Start()
    {
        Bunny[] foundBunnies = FindObjectsByType<Bunny>(FindObjectsSortMode.InstanceID);
        bunnies = new List<Bunny>(foundBunnies);

        Predator[] foundPredators = FindObjectsByType<Predator>(FindObjectsSortMode.InstanceID);
        predators = new List<Predator>(foundPredators);

        foodSpawner = FindFirstObjectByType<FoodSpawner>();
    }

    void Update()
    {
        // Se ejecuta la simulación cuadro a cuadro usando Time.deltaTime
        // para lograr un movimiento continuo y lineal sin saltos por segundo.
        Simulate(Time.deltaTime);
    }

    void Simulate(float deltaTime)
    {
        // 1. Limpieza y simulación de conejos
        bunnies.RemoveAll(b => b == null || !b.isAlive);
        for (int i = bunnies.Count - 1; i >= 0; i--)
        {
            if (bunnies[i] != null && bunnies[i].isAlive)
            {
                bunnies[i].Simulate(deltaTime);
            }
        }

        // 2. Limpieza y simulación de depredadores
        predators.RemoveAll(p => p == null || !p.isAlive);
        for (int i = predators.Count - 1; i >= 0; i--)
        {
            if (predators[i] != null && predators[i].isAlive)
            {
                predators[i].Simulate(deltaTime);
            }
        }

        // 3. Generación de alimento
        if (foodSpawner != null)
        {
            foodSpawner.Simulate(deltaTime);
        }
    }

    /// <summary>
    /// Consulta si la población actual permite más conejos.
    /// </summary>
    public bool CanSpawnBunny()
    {
        bunnies.RemoveAll(b => b == null || !b.isAlive);
        return bunnies.Count < maxBunnies;
    }

    /// <summary>
    /// Registrar nuevos conejos nacidos en la escena validando el límite.
    /// </summary>
    public bool RegisterBunny(Bunny newBunny)
    {
        bunnies.RemoveAll(b => b == null || !b.isAlive);

        if (bunnies.Count >= maxBunnies)
        {
            if (newBunny != null && !bunnies.Contains(newBunny))
            {
                Destroy(newBunny.gameObject);
            }
            return false;
        }

        if (newBunny != null && !bunnies.Contains(newBunny))
        {
            bunnies.Add(newBunny);
            return true;
        }

        return false;
    }
}
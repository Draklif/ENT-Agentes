using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    [Header("Simulation Settings")]
    [Min(0.01f)]
    public float secondsPerIteration = 1.0f;

    private float time = 0f;

    private List<Bunny> bunnies = new List<Bunny>();
    private List<Predator> predators = new List<Predator>();

    private FoodSpawner foodSpawner;
    private DroughtEvent droughtEvent;
    private BunnyAgility[] bunnyAgilities;

    private void Start()
    {
        Bunny[] foundBunnies =
            FindObjectsByType<Bunny>(FindObjectsSortMode.InstanceID);

        bunnies = new List<Bunny>(foundBunnies);

        Predator[] foundPredators =
            FindObjectsByType<Predator>(FindObjectsSortMode.InstanceID);

        predators = new List<Predator>(foundPredators);

        foodSpawner = FindFirstObjectByType<FoodSpawner>();

        droughtEvent = FindFirstObjectByType<DroughtEvent>();

        bunnyAgilities =
            FindObjectsByType<BunnyAgility>(FindObjectsSortMode.InstanceID);
    }

    private void Update()
    {
        time += Time.deltaTime;

        if (time >= secondsPerIteration)
        {
            time = 0f;

            Simulate();
        }
    }

    private void Simulate()
    {
        // Simular conejos
        foreach (Bunny bunny in bunnies)
        {
            if (bunny != null && bunny.isAlive)
            {
                bunny.Simulate(secondsPerIteration);
            }
        }

        // Simular agilidad de los conejos
        foreach (BunnyAgility agility in bunnyAgilities)
        {
            if (agility != null)
            {
                agility.Simulate(secondsPerIteration);
            }
        }

        // Simular depredadores
        foreach (Predator predator in predators)
        {
            if (predator != null && predator.isAlive)
            {
                predator.Simulate(secondsPerIteration);
            }
        }

        // Simular generación de comida
        if (foodSpawner != null)
        {
            foodSpawner.Simulate(secondsPerIteration);
        }

        // Simular evento de sequía
        if (droughtEvent != null)
        {
            droughtEvent.Simulate(secondsPerIteration);
        }
    }
}
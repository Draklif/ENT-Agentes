using UnityEngine;

public class Simulate : MonoBehaviour
{
    public float simulationSpeed = 1f;

    void Update()
    {
        float h = Time.deltaTime * simulationSpeed;

        // Actualizar todos los conejos
        Bunny[] bunnies = FindObjectsOfType<Bunny>();
        foreach (Bunny b in bunnies)
            b.Simulate(h);

        // Actualizar todos los depredadores
        Predator[] predators = FindObjectsOfType<Predator>();
        foreach (Predator p in predators)
            p.Simulate(h);

        // Actualizar clima (si fuera necesario)
        ClimateManager cm = FindFirstObjectByType<ClimateManager>();
        // En tu caso, ya no hace falta cm.Simulate(h)
    }
}


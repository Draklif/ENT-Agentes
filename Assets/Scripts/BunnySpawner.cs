using UnityEngine;
using UnityEngine.InputSystem;

public class BunnySpawner : MonoBehaviour
{
    public GameObject bunnyPrefab;

    void Update()
    {
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            SpawnBunny();
        }
    }

    void SpawnBunny()
    {
        Vector3 pos = transform.position + new Vector3(
            Random.Range(-2f, 2f),
            0,
            Random.Range(-2f, 2f)
        );

        GameObject obj = Instantiate(bunnyPrefab, pos, Quaternion.identity);

        Bunny newBunny = obj.GetComponent<Bunny>();

        SimulationManager sim = FindFirstObjectByType<SimulationManager>();
        sim.bunnies.Add(newBunny);
    }
}
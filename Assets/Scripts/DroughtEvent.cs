using UnityEngine;

[RequireComponent(typeof(FoodSpawner))]
public class DroughtEvent : MonoBehaviour
{
    [Header("Drought Settings")]
    [Min(0f)]
    public float timeBeforeDrought = 10f;

    [Min(0.1f)]
    public float droughtDuration = 5f;

    public bool IsDroughtActive { get; private set; }

    private FoodSpawner foodSpawner;
    private int originalMaxFood;

    private float timer = 0f;
    private bool droughtFinished = false;

    private void Awake()
    {
        foodSpawner = GetComponent<FoodSpawner>();
        originalMaxFood = foodSpawner.maxFood;
    }

    public void Simulate(float deltaTime)
    {
        if (droughtFinished)
            return;

        timer += deltaTime;

        // Esperar hasta que llegue el momento de iniciar la sequía
        if (!IsDroughtActive && timer >= timeBeforeDrought)
        {
            StartDrought();
            return;
        }

        // Terminar la sequía después del tiempo configurado
        if (IsDroughtActive && timer >= droughtDuration)
        {
            EndDrought();
        }
    }

    private void StartDrought()
    {
        originalMaxFood = foodSpawner.maxFood;

        foodSpawner.maxFood = 0;

        IsDroughtActive = true;
        timer = 0f;

        Debug.Log("SEQUÍA INICIADA: no aparecerá comida nueva.");
    }

    private void EndDrought()
    {
        foodSpawner.maxFood = originalMaxFood;

        IsDroughtActive = false;
        droughtFinished = true;
        timer = 0f;

        Debug.Log("SEQUÍA FINALIZADA: vuelve a aparecer comida.");
    }

    private void OnDisable()
    {
        // Evita dejar el spawner bloqueado si se desactiva
        // el componente mientras la sequía está activa.
        if (IsDroughtActive && foodSpawner != null)
        {
            foodSpawner.maxFood = originalMaxFood;
            IsDroughtActive = false;
        }
    }
}
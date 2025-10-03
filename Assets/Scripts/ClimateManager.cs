using UnityEngine;
using System; // Necesario para usar Action

public enum ClimateState
{
    Normal,
    Rain,
    Storm,
    Drought
}

public class ClimateManager : MonoBehaviour
{
    public ClimateState currentClimate = ClimateState.Normal;
    public float eventInterval = 15f; // cada 15 segundos cambia el clima
    private float timer;
    public static ClimateManager Instance;

    // Evento que notifica cuando cambia el clima
    public event Action<ClimateState> ClimateChanged;

    // multiplicadores según el clima
    public float rainVisionMultiplier = 0.7f;   // reduce visión un 30%
    public float stormVisionMultiplier = 0.4f; // reduce visión un 60%

    private void Awake()
    {
        // Implementar Singleton básico
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= eventInterval)
        {
            timer = 0f;
            ChangeClimate();
        }
    }

    void ChangeClimate()
    {
        int rand = UnityEngine.Random.Range(0, 4); // 0 = Normal, 1 = Lluvia, 2 = Tormenta, 3 = Sequía
        currentClimate = (ClimateState)rand;
        Debug.Log("Nuevo clima: " + currentClimate);

        // Llamamos al evento para avisar a los suscriptores
        ClimateChanged?.Invoke(currentClimate);
    }

    public float GetVisionMultiplier()
    {
        switch (currentClimate)
        {
            case ClimateState.Rain: return rainVisionMultiplier;
            case ClimateState.Storm: return stormVisionMultiplier;
            default: return 1f;
        }
    }
}

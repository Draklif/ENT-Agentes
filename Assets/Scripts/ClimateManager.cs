using UnityEngine;

public enum ClimateState
{
    Normal,
    Rain,
    Storm
}

public class ClimateManager : MonoBehaviour
{
    public ClimateState currentClimate = ClimateState.Normal;
    public float eventInterval = 15f; // cada 15 segundos cambia el clima
    private float timer;

    // multiplicadores según el clima
    public float rainVisionMultiplier = 0.7f;   // reduce visión un 30%
    public float stormVisionMultiplier = 0.4f; // reduce visión un 60%

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
        int rand = Random.Range(0, 3); // 0 = Normal, 1 = Lluvia, 2 = Tormenta
        currentClimate = (ClimateState)rand;
        Debug.Log("Nuevo clima: " + currentClimate);
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

using UnityEngine;
using System;

public enum ClimateState
{
    Normal,
    Rain,
    Storm,
    Drought
}

public class ClimateManager : MonoBehaviour
{
    public static ClimateManager Instance;

    public ClimateState currentClimate = ClimateState.Normal;
    public float eventInterval = 7f;
    private float timer;

    public event Action<ClimateState> OnClimateChanged;

    void Awake()
    {
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
    
    public float visionMultiplier = 1f;
    public float GetVisionMultiplier()
        {
            return visionMultiplier;
        }
    

    void ChangeClimate()
    {
        int rand = UnityEngine.Random.Range(0, 4); // 0=Normal,1=Lluvia,2=Tormenta,3=Sequía
        currentClimate = (ClimateState)rand;

        Debug.Log("Nuevo clima: " + currentClimate);
        OnClimateChanged?.Invoke(currentClimate);
    }
}

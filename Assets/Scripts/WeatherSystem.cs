using UnityEngine;
using UnityEngine.UI;

public class WeatherSystem : MonoBehaviour
{
    [Header("Day/Night Cycle - 2D")]
    public float dayDurationInSeconds = 300f;
    public float currentTimeOfDay = 0.25f;

    [Header("Seasons")]
    public Season currentSeason = Season.Spring;
    public int currentDay = 1;
    public int daysPerSeason = 10;

    [Header("Season Modifiers")]
    public float foodSpawnMultiplier = 1f;
    public float animalSpeedMultiplier = 1f;
    public float visionRangeMultiplier = 1f;
    public float energyConsumptionMultiplier = 1f;

    [Header("2D Visual Settings")]
    public Camera mainCamera;
    public Color dayColor = Color.white;
    public Color nightColor = new Color(0.3f, 0.3f, 0.5f);
    public Color winterColor = new Color(0.8f, 0.9f, 1f);
    public Color springColor = new Color(0.9f, 1f, 0.9f);

    private float timeAccumulator = 0f;

    public enum Season
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        Update2DVisuals();
    }

    public void AdvanceTime(float deltaTime)
    {
        timeAccumulator += deltaTime;

        currentTimeOfDay += deltaTime / dayDurationInSeconds;

        if (currentTimeOfDay >= 1f)
        {
            currentTimeOfDay = 0f;
            currentDay++;

            if (currentDay > daysPerSeason * 4)
            {
                currentDay = 1;
            }
            UpdateSeason();
        }
    }

    void UpdateSeason()
    {
        if (currentDay <= daysPerSeason)
        {
            currentSeason = Season.Spring;
        }
        else if (currentDay <= daysPerSeason * 2)
        {
            currentSeason = Season.Summer;
        }
        else if (currentDay <= daysPerSeason * 3)
        {
            currentSeason = Season.Autumn;
        }
        else
        {
            currentSeason = Season.Winter;
        }

        UpdateSeasonalEffects();
        Debug.Log($"✅ Estación cambiada a: {currentSeason}, Día: {currentDay}");
    }

    void UpdateSeasonalEffects()
    {
        switch (currentSeason)
        {
            case Season.Spring:
                foodSpawnMultiplier = 1.5f;
                animalSpeedMultiplier = 1f;
                visionRangeMultiplier = 1f;
                energyConsumptionMultiplier = 1f;
                break;

            case Season.Summer:
                foodSpawnMultiplier = 1.2f;
                animalSpeedMultiplier = 1f;
                visionRangeMultiplier = 1f;
                energyConsumptionMultiplier = 1f;
                break;

            case Season.Autumn:
                foodSpawnMultiplier = 0.8f;
                animalSpeedMultiplier = 0.9f;
                visionRangeMultiplier = 0.9f;
                energyConsumptionMultiplier = 1.1f;
                break;

            case Season.Winter:
                foodSpawnMultiplier = 0.4f;
                animalSpeedMultiplier = 0.6f;
                visionRangeMultiplier = 0.8f;
                energyConsumptionMultiplier = 1.25f;
                break;
        }
    }

    void Update2DVisuals()
    {
        if (mainCamera != null)
        {
            Color targetColor = dayColor;

            // Aplicar color de estación
            switch (currentSeason)
            {
                case Season.Winter:
                    targetColor = winterColor;
                    break;
                case Season.Spring:
                    targetColor = springColor;
                    break;
            }

            // Aplicar efecto nocturno
            if (IsNight())
            {
                targetColor = Color.Lerp(targetColor, nightColor, 0.7f);
            }

            mainCamera.backgroundColor = targetColor;
        }
    }

    public float GetFoodSpawnMultiplier() { return foodSpawnMultiplier; }
    public float GetAnimalSpeedMultiplier() { return animalSpeedMultiplier; }
    public float GetVisionRangeMultiplier() { return visionRangeMultiplier; }
    public float GetEnergyConsumptionMultiplier() { return energyConsumptionMultiplier; }

    public bool IsNight()
    {
        return currentTimeOfDay > 0.75f || currentTimeOfDay < 0.25f;
    }

    public string GetWeatherInfo()
    {
        string timeOfDay = IsNight() ? "Noche" : "Día";
        return $"Día {currentDay} | {currentSeason} | {timeOfDay} | Hora: {(int)(currentTimeOfDay * 24f)}:00";
    }
}
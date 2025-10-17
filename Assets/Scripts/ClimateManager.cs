using UnityEngine;
using System;

public class ClimateManager : MonoBehaviour
{
    public static ClimateManager Instance { get; private set; }

    public enum ClimateState { Clear, Rain, Storm, Drought }
    public ClimateState CurrentClimate { get; private set; } = ClimateState.Clear;

    public event Action<ClimateState> OnClimateChanged;

    [Header("Duración de cada clima (segundos)")]
    public float durationPerClimate = 30f;

    private float timer;

    [Header("Efectos de Partículas y Luz")]
    public ParticleSystem rainParticles;
    public ParticleSystem stormParticles;
    public UnityEngine.UI.Image lightningImage; // imagen blanca para el rayo

    private bool isFlashing = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        timer = durationPerClimate;
        SetClimate(ClimateState.Clear);
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            NextClimate();
            timer = durationPerClimate;
        }
    }

    void NextClimate()
    {
        int next = ((int)CurrentClimate + 1) % Enum.GetNames(typeof(ClimateState)).Length;
        SetClimate((ClimateState)next);
    }

    void SetClimate(ClimateState newClimate)
    {
        CurrentClimate = newClimate;

        // Apagar todos los efectos primero
        if (rainParticles != null) rainParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (stormParticles != null) stormParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (lightningImage != null) lightningImage.color = new Color(1, 1, 1, 0);

        // Activar según el clima
        switch (newClimate)
        {
            case ClimateState.Clear:
                Debug.Log("Clima despejado");
                break;

            case ClimateState.Rain:
                Debug.Log("Lluvia ligera");
                if (rainParticles != null) rainParticles.Play();
                break;

            case ClimateState.Storm:
                Debug.Log("Tormenta con rayos");
                if (stormParticles != null) stormParticles.Play();
                if (lightningImage != null)
                    StartCoroutine(LightningFlash());
                break;

            case ClimateState.Drought:
                Debug.Log("Sequía: se detiene el spawn de comida");
                break;
        }

        OnClimateChanged?.Invoke(newClimate);
    }

    System.Collections.IEnumerator LightningFlash()
    {
        while (CurrentClimate == ClimateState.Storm)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(3f, 6f));

            if (lightningImage != null)
            {
                // rayo activo
                lightningImage.color = new Color(1, 1, 1, 1);
                yield return new WaitForSeconds(0.2f);
                // rayo apagado
                lightningImage.color = new Color(1, 1, 1, 0);
            }
        }
    }

    public float GetVisionMultiplier()
    {
        switch (CurrentClimate)
        {
            case ClimateState.Rain:
                return 0.8f; // 80% del rango normal
            case ClimateState.Storm:
                return 0.5f; // 50% del rango normal
            case ClimateState.Drought:
            case ClimateState.Clear:
            default:
                return 1f; // visión normal
        }
    }

    // dentro de ClimateManager.cs
    public float GetSpeedMultiplier()
    {
        switch (CurrentClimate)
        {
            case ClimateState.Rain:
                return 0.9f;   // lluvia ligera reduce velocidad un 10%
            case ClimateState.Storm:
                return 0.7f;   // tormenta reduce velocidad bastante
            case ClimateState.Drought:
                return 1f;     // sequía no afecta movimiento
            case ClimateState.Clear:
            default:
                return 1f;     // clima normal
        }
    }


}





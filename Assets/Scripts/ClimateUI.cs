using UnityEngine;
using TMPro; // Si usas TextMeshPro

public class ClimateUI : MonoBehaviour
{
    [SerializeField] private TMP_Text climateText;

    void Start()
    {
        // Te suscribes al evento del ClimateManager
        ClimateManager.Instance.ClimateChanged += UpdateClimateText;

        // Inicializa con el clima actual al inicio
        UpdateClimateText(ClimateManager.Instance.currentClimate);
    }

    void UpdateClimateText(ClimateState newClimate)
    {
        switch (newClimate)
        {
            case ClimateState.Normal:
                climateText.text = "Clima: Normal";
                break;
            case ClimateState.Rain:
                climateText.text = "Clima: Lluvia";
                break;
            case ClimateState.Storm:
                climateText.text = "Clima: Tormenta";
                break;
            case ClimateState.Drought:
                climateText.text = "Sequía: No aparece comida nueva";
                break;
        }
    }
}


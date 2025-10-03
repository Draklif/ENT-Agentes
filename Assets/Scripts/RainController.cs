using UnityEngine;

public class RainController : MonoBehaviour
{
    public ParticleSystem lluvia;
    public ParticleSystem tormenta;

    void Start()
    {
        // Nos suscribimos al cambio de clima
        ClimateManager.Instance.OnClimateChanged += HandleClimateChange;

        // Asegurar que arranquen apagados
        lluvia.Stop();
        tormenta.Stop();
    }

    private void HandleClimateChange(ClimateState state)
    {
        switch (state)
        {
            case ClimateState.Rain:
                if (!lluvia.isPlaying) lluvia.Play();
                tormenta.Stop();
                break;

            case ClimateState.Storm:
                if (!tormenta.isPlaying) tormenta.Play();
                lluvia.Stop();
                break;

            default: // Normal o Sequía
                lluvia.Stop();
                tormenta.Stop();
                break;
        }
    }
}

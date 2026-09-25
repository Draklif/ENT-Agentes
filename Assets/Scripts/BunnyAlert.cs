using UnityEngine;

// Aviso entre conejos. No mueve ni cambia estados.
// Guarda si un vecino avisó de un depredador y dónde estaba.
public class BunnyAlert : MonoBehaviour
{
    [Header("Comunicación")]
    public float radius = 4f;

    public bool warned;
    public Vector3 threatPosition;

    public void Warn(Vector3 predatorPosition)
    {
        warned = true;
        threatPosition = predatorPosition;
    }

    // Marca a los otros conejos que estén dentro del radio.
    public void WarnNearby(Vector3 predatorPosition)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            radius,
            LayerMask.GetMask("Bunnies")
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject)
            {
                continue;
            }

            BunnyAlert other = hit.GetComponent<BunnyAlert>();
            if (other != null)
            {
                other.Warn(predatorPosition);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}

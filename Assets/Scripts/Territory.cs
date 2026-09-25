using UnityEngine;

// Zona fija de patrulla de un depredador.
// No cambia estados ni movimiento: solo recuerda el centro y el radio.
public class Territory : MonoBehaviour
{
    [Header("Territorio")]
    public float radius = 4f;

    public Vector3 center;

    void Start()
    {
        // El centro queda donde apareció este depredador y no se vuelve a mover.
        center = transform.position;
    }

    // Si el punto ya está dentro, lo devuelve igual.
    // Si se sale del círculo, lo recorta al borde.
    public Vector3 ClampPoint(Vector3 point)
    {
        Vector3 offset = point - center;
        offset.z = 0f;

        if (offset.magnitude <= radius)
        {
            return new Vector3(point.x, point.y, 0f);
        }

        Vector3 clamped = center + offset.normalized * radius;
        clamped.z = 0f;
        return clamped;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Vector3 drawCenter = Application.isPlaying ? center : transform.position;
        Gizmos.DrawWireSphere(drawCenter, radius);
    }
}

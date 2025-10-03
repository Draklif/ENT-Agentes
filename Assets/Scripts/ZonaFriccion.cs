using UnityEngine;

public class ZonaFriccion : MonoBehaviour
{
    [Header("Configuración de Fricción")]
    [Range(0.1f, 0.9f)]
    public float factorFriccion = 0.5f;

    [Header("Configuración Visual")]
    public Color colorVisual = new Color(0.8f, 0.6f, 0.2f, 0.3f);

    private void OnDrawGizmos()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Gizmos.color = colorVisual;
            Gizmos.DrawCube(transform.position, collider.bounds.size);
        }
    }
}
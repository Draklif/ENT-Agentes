using UnityEngine;

public class CuerpoAgua : MonoBehaviour
{
    [Header("Configuración del Agua")]
    public bool conejosPuedenCruzar = false;
    public bool depredadoresPuedenCruzar = true;
    public float factorVelocidadDepredadores = 0.3f;

    [Header("Configuración Visual")]
    public Color colorAgua = new Color(0.2f, 0.4f, 0.8f, 0.4f);

    private void OnDrawGizmos()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Gizmos.color = colorAgua;
            Gizmos.DrawCube(transform.position, collider.bounds.size);
        }
    }
}
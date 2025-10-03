using UnityEngine;

public class DetectorTerreno : MonoBehaviour
{
    [Header("Configuración de Velocidad")]
    public float velocidadNormal = 1f;
    public float velocidadActual = 1f;
    
    [Header("Configuración de Detección")]
    public float radioDeteccion = 0.3f;
    public LayerMask capaFriccion;
    public LayerMask capaAgua;
    
    private bool enAgua = false;
    private bool enFriccion = false;
    private float factorFriccionActual = 1f;

    void Update()
    {
        DetectarTerreno();
        ActualizarVelocidad();
    }

    void DetectarTerreno()
    {
        // Detectar si está en agua
        Collider2D hitAgua = Physics2D.OverlapCircle(transform.position, radioDeteccion, capaAgua);
        bool estabaEnAgua = enAgua;
        enAgua = hitAgua != null;

        // Detectar si está en zona de fricción
        Collider2D hitFriccion = Physics2D.OverlapCircle(transform.position, radioDeteccion, capaFriccion);
        enFriccion = hitFriccion != null;
        
        if (hitFriccion != null)
        {
            ZonaFriccion zona = hitFriccion.GetComponent<ZonaFriccion>();
            if (zona != null)
            {
                factorFriccionActual = zona.factorFriccion;
            }
        }
        else
        {
            factorFriccionActual = 1f;
        }

        // Si acaba de entrar al agua y es conejo, cambiar dirección
        if (enAgua && !estabaEnAgua && gameObject.CompareTag("Bunny"))
        {
            CuerpoAgua cuerpoAgua = hitAgua.GetComponent<CuerpoAgua>();
            if (cuerpoAgua != null && !cuerpoAgua.conejosPuedenCruzar)
            {
                CambiarDireccionLejosDeAgua(hitAgua.transform.position);
            }
        }
    }

    void ActualizarVelocidad()
    {
        if (enAgua)
        {
            Collider2D hitAgua = Physics2D.OverlapCircle(transform.position, radioDeteccion, capaAgua);
            if (hitAgua != null)
            {
                CuerpoAgua cuerpoAgua = hitAgua.GetComponent<CuerpoAgua>();
                if (cuerpoAgua != null)
                {
                    if (gameObject.CompareTag("Bunny") && !cuerpoAgua.conejosPuedenCruzar)
                    {
                        velocidadActual = 0f; // Bloquear completamente
                        return;
                    }
                    else if (gameObject.CompareTag("Predator") && cuerpoAgua.depredadoresPuedenCruzar)
                    {
                        velocidadActual = velocidadNormal * cuerpoAgua.factorVelocidadDepredadores;
                        return;
                    }
                }
            }
        }

        if (enFriccion)
        {
            velocidadActual = velocidadNormal * factorFriccionActual;
        }
        else
        {
            velocidadActual = velocidadNormal;
        }
    }

    void CambiarDireccionLejosDeAgua(Vector3 posicionAgua)
    {
        // Buscar componente Bunny para cambiar su destino
        Bunny bunny = GetComponent<Bunny>();
        if (bunny != null)
        {
            // Usar reflexión para acceder al campo destination
            System.Reflection.FieldInfo field = typeof(Bunny).GetField("destination", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                Vector3 direccionSegura = (transform.position - posicionAgua).normalized;
                Vector3 nuevoDestino = transform.position + direccionSegura * 3f;
                field.SetValue(bunny, nuevoDestino);
            }
        }
    }

    public float ObtenerVelocidad()
    {
        return velocidadActual;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = enAgua ? Color.blue : (enFriccion ? Color.yellow : Color.green);
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}
using UnityEngine;
using System.Collections.Generic;

public class ManagerTerreno : MonoBehaviour
{
    [Header("Configuración")]
    public float intervaloActualizacion = 0.1f;
    
    private List<DetectorTerreno> detectores = new List<DetectorTerreno>();
    private float tiempoAcumulado = 0f;

    void Start()
    {
        // Encontrar todos los agentes en escena y agregar detectores
        InicializarDetectores();
    }

    void Update()
    {
        tiempoAcumulado += Time.deltaTime;
        
        if (tiempoAcumulado >= intervaloActualizacion)
        {
            tiempoAcumulado = 0f;
            ActualizarVelocidades();
        }
    }

    void InicializarDetectores()
    {
        // Agregar detectores a todos los conejos
        Bunny[] conejos = FindObjectsOfType<Bunny>();
        foreach (Bunny bunny in conejos)
        {
            DetectorTerreno detector = bunny.gameObject.AddComponent<DetectorTerreno>();
            detector.velocidadNormal = bunny.speed;
            detector.capaFriccion = LayerMask.GetMask("Friction");
            detector.capaAgua = LayerMask.GetMask("Water");
            detectores.Add(detector);
        }

        // Agregar detectores a todos los depredadores
        Predator[] depredadores = FindObjectsOfType<Predator>();
        foreach (Predator depredador in depredadores)
        {
            DetectorTerreno detector = depredador.gameObject.AddComponent<DetectorTerreno>();
            detector.velocidadNormal = depredador.speed;
            detector.capaFriccion = LayerMask.GetMask("Friction");
            detector.capaAgua = LayerMask.GetMask("Water");
            detectores.Add(detector);
        }
    }

    void ActualizarVelocidades()
    {
        foreach (DetectorTerreno detector in detectores)
        {
            if (detector != null)
            {
                // Actualizar velocidad en los componentes originales
                ActualizarVelocidadAgente(detector);
            }
        }
    }

    void ActualizarVelocidadAgente(DetectorTerreno detector)
    {
        float velocidadActual = detector.ObtenerVelocidad();
        
        // Actualizar Bunny
        Bunny bunny = detector.GetComponent<Bunny>();
        if (bunny != null)
        {
            // Usar reflexión para modificar la velocidad sin cambiar el script base
            System.Reflection.FieldInfo field = typeof(Bunny).GetField("speed", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(bunny, velocidadActual);
            }
        }

        // Actualizar Predator
        Predator predator = detector.GetComponent<Predator>();
        if (predator != null)
        {
            System.Reflection.FieldInfo field = typeof(Predator).GetField("speed", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(predator, velocidadActual);
            }
        }
    }

    public void AgregarDetector(DetectorTerreno detector)
    {
        if (!detectores.Contains(detector))
        {
            detectores.Add(detector);
        }
    }
}
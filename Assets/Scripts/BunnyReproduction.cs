using UnityEngine;

[RequireComponent(typeof(Bunny))]
public class BunnyReproduction : MonoBehaviour
{
    [Header("Configuración de Reproducción")]
    public GameObject bunnyPrefab;

    [Tooltip("Tiempo de espera en segundos entre reproducciones (y al nacer)")]
    public float reproductionCooldown = 15f; 
    public float reproCooldownTimer = 0f;

    [Header("Requisitos y Costos")]
    [Tooltip("Energía mínima requerida para procrear")]
    public float minEnergyToReproduce = 5f;

    [Tooltip("Energía que pierde cada padre al reproducirse")]
    public float energyCost = 3.5f;

    public int gender
    {
        get
        {
            if (bunny == null) bunny = GetComponent<Bunny>();
            return bunny != null ? bunny.gender : 0;
        }
    }

    private Bunny bunny;

    private void Awake()
    {
        bunny = GetComponent<Bunny>();
    }

    private void Start()
    {
        // Al nacer (recientes), inician con el cooldown activo para no reproducirse de inmediato
        reproCooldownTimer = reproductionCooldown;
    }

    public void SimulateCooldown(float h)
    {
        if (reproCooldownTimer > 0f)
        {
            reproCooldownTimer -= h;
        }
    }

    public bool CanReproduce()
    {
        if (bunny == null) bunny = GetComponent<Bunny>();
        return reproCooldownTimer <= 0f && 
               bunny != null && 
               bunny.isAlive && 
               bunny.energy >= minEnergyToReproduce;
    }

    public BunnyReproduction FindNearestPartner(float visionRange)
    {
        if (bunny == null) bunny = GetComponent<Bunny>();

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, visionRange, LayerMask.GetMask("Bunnies"));
        BunnyReproduction nearest = null;
        float minDist = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == this.gameObject) continue;

            BunnyReproduction partnerRepro = hit.GetComponent<BunnyReproduction>();
            
            if (partnerRepro == null || !partnerRepro.CanReproduce()) continue;

            Bunny partnerBunny = partnerRepro.GetComponent<Bunny>();

            if (partnerBunny != null && partnerBunny.isAlive)
            {
                if (bunny.gender != partnerBunny.gender)
                {
                    float dist = Vector2.Distance(transform.position, partnerRepro.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = partnerRepro;
                    }
                }
            }
        }
        return nearest;
    }

    public bool TryReproduceWith(BunnyReproduction partner)
    {
        if (!CanReproduce() || partner == null || !partner.CanReproduce()) return false;

        SimulationManager manager = FindAnyObjectByType<SimulationManager>();

        if (bunnyPrefab != null)
        {
            int numChildren = Random.Range(1, 6);

            for (int i = 0; i < numChildren; i++)
            {
                if (manager != null && manager.bunnies.Count >= manager.maxBunnies)
                {
                    break;
                }

                // Genera la posición validando que no atraviese ni quede dentro de muros
                Vector3 spawnPos = GetValidSpawnPosition();
                GameObject newBunnyGO = Instantiate(bunnyPrefab, spawnPos, Quaternion.identity);

                Bunny newBunny = newBunnyGO.GetComponent<Bunny>();
                if (manager != null && newBunny != null)
                {
                    manager.RegisterBunny(newBunny);
                }
            }
        }
        else
        {
            Debug.LogError("¡ATENCIÓN!: 'bunnyPrefab' está sin asignar en BunnyReproduction.");
            return false;
        }

        this.reproCooldownTimer = reproductionCooldown;
        partner.reproCooldownTimer = partner.reproductionCooldown;

        if (bunny != null) 
            bunny.energy = Mathf.Max(0f, bunny.energy - energyCost);
        
        Bunny partnerBunny = partner.GetComponent<Bunny>();
        if (partnerBunny != null) 
            partnerBunny.energy = Mathf.Max(0f, partnerBunny.energy - energyCost);

        return true;
    }

    /// <summary>
    /// Calcula un punto de aparición seguro verificando colisiones con muros.
    /// </summary>
    private Vector3 GetValidSpawnPosition()
    {
        int obstacleLayer = LayerMask.GetMask("Obstacles");
        float spawnRadius = 0.5f;

        for (int attempt = 0; attempt < 5; attempt++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            
            // 1. Revisa si la trayectoria desde el padre hacia la nueva posición cruza un muro
            RaycastHit2D hit = Physics2D.Raycast(transform.position, randomOffset.normalized, randomOffset.magnitude, obstacleLayer);
            
            if (hit.collider == null)
            {
                // 2. Revisa que el punto final no esté superpuesto con una pared
                Vector2 targetPos = (Vector2)transform.position + randomOffset;
                if (Physics2D.OverlapCircle(targetPos, 0.15f, obstacleLayer) == null)
                {
                    return targetPos;
                }
            }
        }

        // Si todos los intentos fallaron (ej. rincón muy estrecho), nace exactamente en la posición del padre
        return transform.position;
    }

    private void OnDrawGizmos()
    {
        if (bunny == null) bunny = GetComponent<Bunny>();
        if (bunny != null)
        {
            Gizmos.color = (bunny.gender == 0) ? Color.cyan : Color.magenta;
            Gizmos.DrawSphere(transform.position + Vector3.up * 0.5f, 0.15f);
        }
    }
}
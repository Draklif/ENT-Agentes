using UnityEngine;

public class Bunny : MonoBehaviour
{
    [Header("Bunny Settings")]
    [Tooltip("0 = Macho, 1 = Hembra")]
    public int gender;
    public float energy = 10f;
    public float maxEnergy = 10f;
    public float age = 0f;
    public float maxAge = 25f;
    public float speed = 1.5f;
    public float visionRange = 6f;

    [Header("Bunny States")]
    public bool isAlive = true;
    public BunnyState currentState = BunnyState.Exploring;

    private Vector3 destination;
    private float h;
    private BunnyReproduction reproComponent;

    private void Awake()
    {
        reproComponent = GetComponent<BunnyReproduction>();

        // Asigna 0 (Macho) o 1 (Hembra) aleatoriamente al iniciar
        gender = (Random.value >= 0.5f) ? 0 : 1;
    }

    private void Start()
    {
        destination = transform.position;

        SimulationManager manager = FindAnyObjectByType<SimulationManager>();
        if (manager != null)
        {
            manager.RegisterBunny(this);
        }
    }

    private void Update()
    {
        if (!isAlive) return;

        // Si está buscando reproducirse, sigue a la pareja en tiempo real
        if (currentState == BunnyState.Reproduccion && reproComponent != null)
        {
            BunnyReproduction partner = reproComponent.FindNearestPartner(visionRange);
            if (partner != null)
            {
                destination = partner.transform.position;
            }
        }

        // Movimiento fluido continuo
        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            speed * Time.deltaTime
        );
    }

    public void Simulate(float h)
    {
        if (!isAlive) return;

        this.h = h;

        if (reproComponent != null)
        {
            reproComponent.SimulateCooldown(h);
        }

        // 1. Evalúa el estado basándose en prioridades estrictas
        EvaluateState();

        // 2. Ejecuta la acción del estado seleccionado
        switch (currentState)
        {
            case BunnyState.Exploring:
                Explore();
                break;
            case BunnyState.SearchingFood:
                SearchFood();
                break;
            case BunnyState.Eating:
                Eat();
                break;
            case BunnyState.Fleeing:
                Flee();
                break;
            case BunnyState.Reproduccion:
                Reproduce();
                break;
        }

        energy -= 0.2f * h;
        Age();
        CheckState();
    }

    void EvaluateState()
    {
        // 1. PRIORIDAD MÁXIMA: Depredador cerca -> Huir
        if (PredatorInRange())
        {
            currentState = BunnyState.Fleeing;
            return;
        }

        // 2. SEGUNDA PRIORIDAD: Si ya pisó comida -> Comer
        Collider2D foodHit = Physics2D.OverlapCircle(transform.position, 0.3f, LayerMask.GetMask("Food"));
        if (foodHit != null)
        {
            Food food = foodHit.GetComponent<Food>();
            if (food != null)
            {
                currentState = BunnyState.Eating;
                return;
            }
        }

        // 3. TERCERA PRIORIDAD: Hambre Crítica (< 5.0) -> Buscar Comida
        if (energy < 5.0f)
        {
            Food nearestFood = FindNearestFood();
            if (nearestFood != null)
            {
                currentState = BunnyState.SearchingFood;
                destination = nearestFood.transform.position;
                return;
            }
        }

        // 4. CUARTA PRIORIDAD: Reproducción (Si tiene energía suficiente y hay pareja lista)
        if (reproComponent != null && reproComponent.CanReproduce())
        {
            BunnyReproduction partner = reproComponent.FindNearestPartner(visionRange);
            if (partner != null)
            {
                currentState = BunnyState.Reproduccion;
                destination = partner.transform.position;
                return;
            }
        }

        // 5. QUINTA PRIORIDAD: Comida Oportunista (Si no está completamente lleno y ve comida)
        if (energy < maxEnergy - 1.0f)
        {
            Food nearestFood = FindNearestFood();
            if (nearestFood != null)
            {
                currentState = BunnyState.SearchingFood;
                destination = nearestFood.transform.position;
                return;
            }
        }

        // 6. ÚLTIMA PRIORIDAD: Explorar libremente
        currentState = BunnyState.Exploring;
    }

    void Explore()
    {
        if (Vector3.Distance(transform.position, destination) < 0.2f)
        {
            SelectNewDestination();
        }
    }

    void SearchFood()
    {
        Food nearestFood = FindNearestFood();
        if (nearestFood == null)
        {
            currentState = BunnyState.Exploring;
            return;
        }

        destination = nearestFood.transform.position;

        if (Vector3.Distance(transform.position, nearestFood.transform.position) < 0.3f)
        {
            currentState = BunnyState.Eating;
        }
    }

    void Eat()
    {
        Collider2D foodHit = Physics2D.OverlapCircle(transform.position, 0.3f, LayerMask.GetMask("Food"));
        if (foodHit != null)
        {
            Food food = foodHit.GetComponent<Food>();
            if (food != null)
            {
                energy = Mathf.Min(maxEnergy, energy + food.nutrition);
                Destroy(food.gameObject);
            }
        }

        currentState = BunnyState.Exploring;
    }

    void Flee()
    {
        Vector3 fleeDir = (transform.position - GetNearestPredatorPosition()).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, fleeDir, visionRange, LayerMask.GetMask("Obstacles"));

        if (hit.collider != null)
        {
            float offset = transform.localScale.magnitude * 0.5f;
            destination = hit.point - (Vector2)fleeDir * offset;
        }
        else
        {
            destination = transform.position + fleeDir * visionRange;
        }
    }

    void Reproduce()
    {
        if (reproComponent == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.8f, LayerMask.GetMask("Bunnies"));

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == this.gameObject) continue;

            BunnyReproduction partnerRepro = hit.GetComponent<BunnyReproduction>();

            if (partnerRepro != null && partnerRepro.gender != reproComponent.gender)
            {
                if (reproComponent.TryReproduceWith(partnerRepro))
                {
                    SelectNewDestination();
                    currentState = BunnyState.Exploring;
                    return;
                }
            }
        }

        BunnyReproduction targetPartner = reproComponent.FindNearestPartner(visionRange);
        if (targetPartner != null)
        {
            destination = targetPartner.transform.position;
        }
        else
        {
            currentState = BunnyState.Exploring;
        }
    }

    void SelectNewDestination()
    {
        Vector3 direction = new Vector3(
            Random.Range(-visionRange, visionRange),
            Random.Range(-visionRange, visionRange),
            0
        );

        Vector3 targetPoint = transform.position + direction;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, visionRange, LayerMask.GetMask("Obstacles"));

        if (hit.collider != null)
        {
            float offset = transform.localScale.magnitude * 0.5f;
            destination = hit.point - (Vector2)direction.normalized * offset;
        }
        else
        {
            destination = targetPoint;
        }
    }

    void Age()
    {
        age += h;
    }

    void CheckState()
    {
        if (energy <= 0 || age > maxAge)
        {
            isAlive = false;
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(destination, 0.2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, destination);
    }

    bool PredatorInRange()
    {
        Collider2D predator = Physics2D.OverlapCircle(transform.position, visionRange, LayerMask.GetMask("Foxes"));
        return predator != null;
    }

    Vector3 GetNearestPredatorPosition()
    {
        Collider2D[] predators = Physics2D.OverlapCircleAll(transform.position, visionRange, LayerMask.GetMask("Foxes"));
        float minDist = Mathf.Infinity;
        Vector3 pos = transform.position;

        foreach (var p in predators)
        {
            float dist = Vector2.Distance(transform.position, p.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                pos = p.transform.position;
            }
        }

        return pos;
    }

    Food FindNearestFood()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, visionRange, LayerMask.GetMask("Food"));
        Food nearest = null;
        float minDist = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            Food food = hit.GetComponent<Food>();
            if (food != null)
            {
                float dist = Vector2.Distance(transform.position, food.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = food;
                }
            }
        }

        return nearest;
    }
}
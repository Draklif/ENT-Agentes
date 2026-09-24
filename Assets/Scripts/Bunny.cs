using UnityEngine;

public class Bunny : MonoBehaviour
{
    [Header("Bunny Settings")]
    public float energy = 10;
    public float age = 0;
    public float maxAge = 20;
    public float speed = 1f;
    public float visionRange = 5f;

    [Header("Bunny States")]
    public bool isAlive = true;
    public BunnyState currentState = BunnyState.Exploring;

    private Vector3 destination;
    private float h;

    // NUEVO: Guarda desde dónde viene el peligro cuando otro conejo avisa
    private Vector3 fleeFromPosition;

    // NUEVO: Indica si este conejo recibió la alerta de otro conejo
    private bool receivedFleeAlert = false;

    private void Start()
    {
        destination = transform.position;

        // NUEVO: Inicializa la posición del peligro
        fleeFromPosition = transform.position;
    }

    public void Simulate(float h)
    {
        if (!isAlive) return;

        this.h = h;

        EvaluateState();

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
        }

        Move();
        Age();
        CheckState();
    }

    void EvaluateState()
    {
        // 1. Si hay un depredador cerca -> huir
        if (PredatorInRange())
        {
            Debug.Log($"Bunny {name} DETECTÓ AL ZORRO");

            currentState = BunnyState.Fleeing;

            // NUEVO: Guarda la posición del depredador que detectó directamente
            fleeFromPosition = GetNearestPredatorPosition();

            // NUEVO: Si detecta un depredador, avisa a los conejos cercanos
            if (!receivedFleeAlert)
            {
                AlertNearbyBunnies(fleeFromPosition);
            }

            return;
        }

        // NUEVO: Si recibió una alerta de otro conejo, mantiene el estado de huida
        // aunque el depredador esté fuera de su propio rango de visión
        if (receivedFleeAlert)
        {
            Debug.Log($"Bunny {name} RECIBIÓ UNA ALERTA");

            currentState = BunnyState.Fleeing;
            return;
        }

        // 2. Si la energía está baja -> buscar comida
        if (energy < 500f)
        {
            Food nearestFood = FindNearestFood();
            if (nearestFood != null)
            {
                currentState = BunnyState.SearchingFood;
                destination = nearestFood.transform.position;
                return;
            }
        }

        // 3. Si está encima de la comida -> comer
        Collider2D foodHit = Physics2D.OverlapCircle(transform.position, 0.2f, LayerMask.GetMask("Food"));
        if (foodHit != null)
        {
            Food food = foodHit.GetComponent<Food>();
            if (food != null)
            {
                currentState = BunnyState.Eating;
                return;
            }
        }

        // 4. Si no pasa nada -> explorar
        if (currentState == BunnyState.Eating == false)
        {
            currentState = BunnyState.Exploring;
        }
    }

    void Explore()
    {
        // Si hay comida a la vista, cambiar de estado
        Food nearestFood = FindNearestFood();
        if (nearestFood != null)
        {
            currentState = BunnyState.SearchingFood;
            destination = nearestFood.transform.position;
            return;
        }

        // Si ya llegó al destino, elegir uno nuevo
        if (Vector3.Distance(transform.position, destination) < 0.1f)
        {
            SelectNewDestination();
        }
    }

    void SearchFood()
    {
        Food nearestFood = FindNearestFood();
        if (nearestFood == null)
        {
            // Si no hay comida, volver a explorar
            currentState = BunnyState.Exploring;
            return;
        }

        destination = nearestFood.transform.position;

        // Si está suficientemente cerca, pasar a comer
        if (Vector3.Distance(transform.position, nearestFood.transform.position) < 0.2f)
        {
            currentState = BunnyState.Eating;
        }
    }

    void Eat()
    {
        Collider2D foodHit = Physics2D.OverlapCircle(transform.position, 0.2f, LayerMask.GetMask("Food"));
        if (foodHit != null)
        {
            Food food = foodHit.GetComponent<Food>();
            if (food != null)
            {
                energy += food.nutrition;
                Destroy(food.gameObject);
            }
        }

        // Después de comer vuelve a explorar
        currentState = BunnyState.Exploring;
    }

    void Flee()
    {
        Debug.Log($"Bunny {name} ESTÁ HUYENDO");

        // NUEVO: Si ve al depredador usa su posición; si recibió una alerta,
        // utiliza la posición del depredador que le comunicó el otro conejo
        Vector3 predatorPosition;

        if (PredatorInRange())
        {
            predatorPosition = GetNearestPredatorPosition();
        }
        else
        {
            predatorPosition = fleeFromPosition;
        }

        // Elegir dirección contraria al depredador
        Vector3 fleeDir = (transform.position - predatorPosition).normalized;
        destination = transform.position + fleeDir * visionRange;



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

        // NUEVO: La alerta ya fue atendida
        receivedFleeAlert = false;
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

    void Move()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            speed * h
        );

        energy -= speed * h;
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
        Debug.Log($"Bunny {name} encontró {hits.Length} colliders en su rango");
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

    // NUEVO: Busca los conejos cercanos para comunicarles el peligro
    void AlertNearbyBunnies(Vector3 predatorPosition)
    {
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(
            transform.position,
            visionRange
        );

        foreach (Collider2D obj in nearbyObjects)
        {
            Bunny otherBunny = obj.GetComponent<Bunny>();

            if (otherBunny != null && otherBunny != this)
            {
                // NUEVO: Los conejos cercanos reciben la alerta y huyen
                otherBunny.fleeFromPosition = predatorPosition;

                Debug.Log($"Bunny {name} AVISÓ A Bunny {otherBunny.name}: ¡ZORRO!");

                otherBunny.StartFleeing();
            }
        }
    }

    // NUEVO: Permite que otro conejo le comunique que debe huir
    public void StartFleeing()
    {
        currentState = BunnyState.Fleeing;

        // NUEVO: Marca que la huida fue provocada por la comunicación
        receivedFleeAlert = true;

        Debug.Log($"Bunny {name} RECIBIÓ LA ALERTA Y VA A HUIR");
    }
}

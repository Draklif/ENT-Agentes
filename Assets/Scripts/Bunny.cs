using UnityEngine;

public class Bunny : MonoBehaviour
{
    [Header("Bunny Settings")]
    public float energy = 10;
    public float age = 0;
    public float maxAge = 20;
    public float speed = 1f;
    public float visionRange = 5f;


    [Header("Terrain Friction")]
    public float actualSpeed;
    private float speedMultiplier = 1f;


    [Header("Bunny States")]
    public bool isAlive = true;
    public BunnyState currentState = BunnyState.Exploring;

    private Vector3 destination;
    private float h;
    private BunnyAlert alert;

    private void Start()
    {
        destination = transform.position;
        actualSpeed = speed;
        alert = GetComponent<BunnyAlert>();
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
            if (alert != null)
            {
                alert.WarnNearby(GetNearestPredatorPosition());
            }

            currentState = BunnyState.Fleeing;
            return;
        }

        // Un vecino aviso: mismo modo de huida, sin ver al depredador.
        if (alert != null && alert.warned)
        {
            currentState = BunnyState.Fleeing;
            return;
        }

        // 2. Si la energa est baja -> buscar comida
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

        // 3. Si est encima de la comida -> comer
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

        // Si ya lleg al destino, elegir uno nuevo
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

        // Si est suficientemente cerca, pasar a comer
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

        // Despues de comer vuelve a explorar
        currentState = BunnyState.Exploring;
    }

    void Flee()
    {
        // Elegir direccion contraria al depredador
        Vector3 predatorPos = GetNearestPredatorPosition();
        if (alert != null && alert.warned && predatorPos == transform.position)
        {
            predatorPos = alert.threatPosition;
        }

        if (alert != null)
        {
            alert.warned = false;
        }

        Vector3 fleeDir = (transform.position - predatorPos).normalized;
        destination = transform.position + fleeDir * visionRange;

        // Despues de huir vuelve a explorar
        currentState = BunnyState.Exploring;

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
        actualSpeed = speed * speedMultiplier;

        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            actualSpeed * h
        );

        energy -= actualSpeed * h;
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
        Debug.Log($"Bunny {name} encontr {hits.Length} colliders en su rango");
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



    private void OnTriggerEnter2D(Collider2D other)
    {

        TerrainFriction terrain = other.GetComponent<TerrainFriction>();

        if (terrain != null)
        {
            speedMultiplier = terrain.speedMultiplier;

        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
      
        TerrainFriction terrain = other.GetComponent<TerrainFriction>();

        if (terrain != null)
        {
            speedMultiplier = 1f;

        }
    }
}

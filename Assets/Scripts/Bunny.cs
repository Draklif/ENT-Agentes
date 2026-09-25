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

    [Header("Reproduction Settings")]
    public float minimumReproductionAge = 5f;
    public float reproductionRange = 2f;
    public float reproductionCooldown = 5f;
    public float reproductionEnergyCost = 2f;

    private Vector3 destination;
    private float h;
    private float reproductionTimer = 0f;

    private void Start()
    {
        destination = transform.position;
    }

    public void Simulate(float h)
    {
        if (!isAlive) return;

        this.h = h;

        if (reproductionTimer > 0f)
        {
            reproductionTimer -= h;
        }

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
        TryReproduce();
        CheckState();
    }

    void EvaluateState()
    {
        // 1. Si hay un depredador cerca -> huir
        if (PredatorInRange())
        {
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
        // Elegir dirección contraria al depredador
        Vector3 fleeDir = (transform.position - GetNearestPredatorPosition()).normalized;
        destination = transform.position + fleeDir * visionRange;

        // Después de huir vuelve a explorar
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

    void TryReproduce()
    {
        //Verifica si el conejo tiene la edad y energía necesarias
        if (age < minimumReproductionAge ||
            energy <= reproductionEnergyCost ||
            reproductionTimer > 0f)
        {
            return;
        }

        Bunny[] allBunnies = FindObjectsByType<Bunny>(
            FindObjectsSortMode.InstanceID
        );

        foreach (Bunny otherBunny in allBunnies)
        {
            //Evita reproducirse consigo mismo
            if (otherBunny == this)
            {
                continue;
            }

            //Verifica que el otro conejo pueda reproducirse
            if (!otherBunny.isAlive ||
                otherBunny.age < otherBunny.minimumReproductionAge ||
                otherBunny.energy <= otherBunny.reproductionEnergyCost ||
                otherBunny.reproductionTimer > 0f)
            {
                continue;
            }

            float distance = Vector3.Distance(
                transform.position,
                otherBunny.transform.position
            );

            //Verifica que los conejos estén suficientemente cerca
            if (distance > reproductionRange)
            {
                continue;
            }

            Vector3 childPosition =
                (transform.position + otherBunny.transform.position) / 2f;

            GameObject childObject = Instantiate(
                gameObject,
                childPosition,
                Quaternion.identity
            );

            Bunny child = childObject.GetComponent<Bunny>();

            if (child != null)
            {
                child.age = 0f;
                child.isAlive = true;
                child.currentState = BunnyState.Exploring;
                child.destination = child.transform.position;

                //Aplica las características y mutaciones de los padres
                BunnyMutation.ApplyMutation(child, this, otherBunny);

                child.reproductionTimer = child.reproductionCooldown;
            }

            //Los padres gastan energía al reproducirse
            energy -= reproductionEnergyCost;
            otherBunny.energy -= otherBunny.reproductionEnergyCost;

            //Activa el tiempo de espera de ambos padres
            reproductionTimer = reproductionCooldown;
            otherBunny.reproductionTimer =
                otherBunny.reproductionCooldown;

            return;
        }
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
}
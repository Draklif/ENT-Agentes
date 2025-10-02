using UnityEngine;

public class Predator : MonoBehaviour
{
    [Header("Predator Settings")]
    public float energy = 10;
    public float age = 0;
    public float maxAge = 20;
    public float baseSpeed = 1f;
    public float baseVisionRange = 5f;

    [Header("Predator States")]
    public bool isAlive = true;
    public PredatorState currentState = PredatorState.Exploring;

    private Vector3 destination;
    private float h;
    private WeatherSystem weather; // ✅ NUEVO: Referencia al sistema climático

    private void Start()
    {
        destination = transform.position;
        weather = FindFirstObjectByType<WeatherSystem>(); // ✅ NUEVO: Buscar sistema climático
    }

    public void Simulate(float h)
    {
        if (!isAlive) return;

        this.h = h;

        switch (currentState)
        {
            case PredatorState.Exploring:
                Explore();
                break;
            case PredatorState.SearchingFood:
                SearchFood();
                break;
            case PredatorState.Eating:
                Eat();
                break;
        }

        Move();
        Age();
        CheckState();
    }

    void Explore()
    {
        Bunny nearestBunny = FindNearestBunny();
        if (nearestBunny != null)
        {
            currentState = PredatorState.SearchingFood;
            destination = nearestBunny.transform.position;
            return;
        }

        if (Vector3.Distance(transform.position, destination) < 0.1f)
        {
            SelectNewDestination();
        }
    }

    void SearchFood()
    {
        Bunny nearestBunny = FindNearestBunny();
        if (nearestBunny == null)
        {
            currentState = PredatorState.Exploring;
            return;
        }

        destination = nearestBunny.transform.position;

        if (Vector3.Distance(transform.position, nearestBunny.transform.position) < 0.2f)
        {
            currentState = PredatorState.Eating;
        }
    }

    void Eat()
    {
        Collider2D foodHit = Physics2D.OverlapCircle(transform.position, 0.2f, LayerMask.GetMask("Bunnies"));
        if (foodHit != null)
        {
            Bunny food = foodHit.GetComponent<Bunny>();
            if (food != null)
            {
                energy += food.age;
                Destroy(food.gameObject);
            }
        }

        currentState = PredatorState.Exploring;
    }

    void Flee()
    {
        SelectNewDestination();
        currentState = PredatorState.Exploring;
    }

    void SelectNewDestination()
    {
        Vector3 direction = new Vector3(
            Random.Range(-GetCurrentVisionRange(), GetCurrentVisionRange()),
            Random.Range(-GetCurrentVisionRange(), GetCurrentVisionRange()),
            0
        );

        Vector3 targetPoint = transform.position + direction;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, GetCurrentVisionRange(), LayerMask.GetMask("Obstacles"));

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
            GetCurrentSpeed() * h // ✅ MODIFICADO: Usar velocidad afectada por clima
        );

        energy -= GetCurrentSpeed() * h * GetCurrentEnergyConsumption(); // ✅ MODIFICADO: Consumo afectado por clima
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
        Gizmos.DrawWireSphere(transform.position, GetCurrentVisionRange());

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(destination, 0.2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, destination);
    }

    Bunny FindNearestBunny()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, GetCurrentVisionRange(), LayerMask.GetMask("Bunnies"));
        Bunny nearest = null;
        float minDist = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            Bunny food = hit.GetComponent<Bunny>();
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

    // ✅✅✅ NUEVOS MÉTODOS PARA EFECTOS CLIMÁTICOS ✅✅✅
    float GetCurrentSpeed()
    {
        float speed = baseSpeed;

        if (weather != null)
        {
            speed *= weather.GetAnimalSpeedMultiplier();

            if (weather.IsNight())
            {
                speed *= 0.7f;
            }
        }

        return speed;
    }

    float GetCurrentVisionRange()
    {
        float vision = baseVisionRange;

        if (weather != null)
        {
            vision *= weather.GetVisionRangeMultiplier();

            if (weather.IsNight())
            {
                vision *= 0.6f;
            }
        }

        return vision;
    }

    float GetCurrentEnergyConsumption()
    {
        float consumption = 1f;

        if (weather != null)
        {
            consumption *= weather.GetEnergyConsumptionMultiplier();
        }

        return consumption;
    }
}
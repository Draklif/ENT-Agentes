using UnityEngine;

public class Predator : MonoBehaviour
{
    [Header("Predator Settings")]
    public float energy = 10;
    public float age = 0;
    public float maxAge = 20;
    public float speed = 1f;
    public float visionRange = 5f;
    public float huntDuration = 7f; // tiempo máximo para cazar
    public float huntTimer = 0f;

    [Header("Territory Settings")]
    public Territorio myTerritory;  // referencia a su territorio
    public Vector3 territoryCenter;   // centro del territorio
    public float territoryRadius = 5f; // radio del territorio
    public bool isOutsideAtStart = true; // si empieza fuera

    [Header("Predator States")]
    public bool isAlive = true;
    public PredatorState currentState = PredatorState.Exploring;

    private Vector3 destination;
    private float h;

    private void Start()
    {
        if (myTerritory == null)
        {
            Territorio[] allTerritories = FindObjectsByType<Territorio>(FindObjectsSortMode.None); // busca todos los territorios
            float minDist = Mathf.Infinity;

            foreach (var t in allTerritories) // elige el más cercano
            {
                float dist = Vector3.Distance(transform.position, t.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    myTerritory = t;
                }
            }
        }

        // ahora configuro su centro y radio
        territoryCenter = myTerritory.transform.position;
        territoryRadius = myTerritory.radius;

        if (isOutsideAtStart)
        {
            transform.position = territoryCenter + (Vector3)(Random.insideUnitCircle.normalized * (territoryRadius * 2f));
            currentState = PredatorState.ReturningToTerritory;
        }
    }

    public void Simulate(float h)
    {
        if (!isAlive) return;

        this.h = h;

        switch (currentState) // máquina de estados
        {
            case PredatorState.Exploring: // patrulla su territorio
                Explore();
                break;
            case PredatorState.SearchingFood: // si ve una presa, la persigue
                SearchFood();
                break;
            case PredatorState.Eating: // si llega a la presa, come
                Eat();
                break;
            case PredatorState.ReturningToTerritory: // si está fuera, vuelve
                ReturnToTerritory();
                break;
            case PredatorState.HuntingFailed: // si falla, pierde energía y vuelve a explorar
                HuntingFailed();
                break;
        }

        Move();
        Age();
        CheckState();
    }

    void Explore()
    {
        // primero revisa si está fuera de su territorio
        if (!IsInsideTerritory())
        {
            currentState = PredatorState.ReturningToTerritory;
            return;
        }

        // si ve una presa
        Bunny nearestBunny = FindNearestBunny();
        if (nearestBunny != null)
        {
            bool preyInside = IsPointInsideTerritory(nearestBunny.transform.position);

            if (preyInside || Random.value < 0.5f) // 50% chance de salir
            {
                currentState = PredatorState.SearchingFood;
                destination = nearestBunny.transform.position;
            }
            return;
        }

        // patrullaje normal
        if (Vector3.Distance(transform.position, destination) < 0.1f)
        {
            SelectNewDestinationWithinTerritory();
        }
    }

    void SearchFood()
    {
        Bunny nearestBunny = FindNearestBunny(); // busca la presa más cercana
        huntTimer += h; // incrementa el temporizador de caza

        if (nearestBunny == null)
        {
            currentState = PredatorState.Exploring;
            huntTimer = 0f;
            return;
        }

        destination = nearestBunny.transform.position;

        if (Vector3.Distance(transform.position, nearestBunny.transform.position) < 0.2f)
        {
            currentState = PredatorState.Eating;
            huntTimer = 0f; // resetea el temporizador
            return; // llegó a la presa
        }

        if (huntTimer >= huntDuration)
        {
            currentState = PredatorState.Exploring;
            huntTimer = 0f; // resetea el temporizador
            return; // se rinde
        }
    }

    void HuntingFailed()
    {
        energy -= 2f; // penalización por fallo
        currentState = PredatorState.Exploring; // vuelve a explorar
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

    void ReturnToTerritory()
    {
        destination = territoryCenter;

        if (Vector3.Distance(transform.position, territoryCenter) < territoryRadius * 0.9f)
        {
            currentState = PredatorState.Exploring;
        }
    }

    void SelectNewDestinationWithinTerritory()
    {
        Vector2 randomPos = (Vector2)territoryCenter + Random.insideUnitCircle * territoryRadius;
        destination = new Vector3(randomPos.x, randomPos.y, 0);
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

    void Die()
    {
        isAlive = false;
        Destroy(gameObject);
    }

    bool IsInsideTerritory()
    {
        return Vector3.Distance(transform.position, territoryCenter) <= territoryRadius;
    }

    bool IsPointInsideTerritory(Vector3 point)
    {
        return Vector3.Distance(point, territoryCenter) <= territoryRadius;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(destination, 0.2f);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(territoryCenter, territoryRadius);
    }

    Bunny FindNearestBunny()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, visionRange, LayerMask.GetMask("Bunnies"));
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
}

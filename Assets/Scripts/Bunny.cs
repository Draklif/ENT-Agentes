using UnityEngine;

public class Bunny : MonoBehaviour
{
    [Header("Bunny Settings")]
    public float energy = 10;
    public float age = 0;
    public float maxAge = 20;
    public float baseSpeed = 20f;
    public float visionRange = 5f;

    [Header("Movement Area")] //límites para mantenerlos dentro del mapa
    public Vector2 areaCenter = Vector2.zero;
    public Vector2 areaSize = new Vector2(20, 20);

    [Header("Bunny States")]
    public bool isAlive = true;
    public BunnyState currentState = BunnyState.Exploring;

    private Vector3 destination;
    private float h;

    private void Start()
    {
        destination = transform.position;
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
        if (PredatorInRange())
        {
            currentState = BunnyState.Fleeing;
            return;
        }

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

        if (currentState == BunnyState.Eating == false)
        {
            currentState = BunnyState.Exploring;
        }
    }

    void Explore()
    {
        Food nearestFood = FindNearestFood();
        if (nearestFood != null)
        {
            currentState = BunnyState.SearchingFood;
            destination = nearestFood.transform.position;
            return;
        }

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
            currentState = BunnyState.Exploring;
            return;
        }

        destination = nearestFood.transform.position;

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

        currentState = BunnyState.Exploring;
    }

    void Flee()
    {
        Vector3 fleeDir = (transform.position - GetNearestPredatorPosition()).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, fleeDir, EffectiveVision(), LayerMask.GetMask("Obstacles"));
        if (hit.collider != null)
        {
            float offset = transform.localScale.magnitude * 0.5f;
            destination = hit.point - (Vector2)fleeDir * offset;
        }
        else
        {
            destination = transform.position + fleeDir * EffectiveVision();
        }

        //limitar a área válida
        destination = ClampToArea(destination);

        currentState = BunnyState.Exploring;
    }

    void SelectNewDestination()
    {
        Vector3 direction = new Vector3(
            Random.Range(-visionRange, visionRange),
            Random.Range(-visionRange, visionRange),
            0
        );

        Vector3 targetPoint = transform.position + direction;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, EffectiveVision(), LayerMask.GetMask("Obstacles"));

        if (hit.collider != null)
        {
            float offset = transform.localScale.magnitude * 0.5f;
            destination = hit.point - (Vector2)direction.normalized * offset;
        }
        else
        {
            destination = targetPoint;
        }

        // mantener dentro del área
        destination = ClampToArea(destination);
    }

    void Move()
    {
        ClimateManager cm = FindFirstObjectByType<ClimateManager>();
        float climateMultiplier = (cm != null ? cm.GetSpeedMultiplier() : 1f);

        float finalSpeed = baseSpeed * climateMultiplier; //separar baseSpeed
        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            finalSpeed * h
        );

        energy -= finalSpeed * h;
    }

    void Age()
    {
        age += h;

        float ageFactor = Mathf.Clamp01(age / maxAge);
        baseSpeed = Mathf.Lerp(1f, 0.3f, ageFactor);
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
        Gizmos.DrawWireSphere(transform.position, EffectiveVision());
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(destination, 0.2f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, destination);

        //mostrar área de movimiento
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(areaCenter, new Vector3(areaSize.x, areaSize.y, 0));
    }

    bool PredatorInRange()
    {
        Collider2D predator = Physics2D.OverlapCircle(transform.position, EffectiveVision(), LayerMask.GetMask("Foxes"));
        return predator != null;
    }

    Vector3 GetNearestPredatorPosition()
    {
        Collider2D[] predators = Physics2D.OverlapCircleAll(transform.position, EffectiveVision(), LayerMask.GetMask("Foxes"));
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
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, EffectiveVision(), LayerMask.GetMask("Food"));
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

    float EffectiveVision()
    {
        ClimateManager cm = FindFirstObjectByType<ClimateManager>();
        return visionRange * (cm != null ? cm.GetVisionMultiplier() : 1f);
    }

    // mantener el conejo dentro del área de simulación
    Vector3 ClampToArea(Vector3 pos)
    {
        float halfX = areaSize.x / 2f;
        float halfY = areaSize.y / 2f;

        pos.x = Mathf.Clamp(pos.x, areaCenter.x - halfX, areaCenter.x + halfX);
        pos.y = Mathf.Clamp(pos.y, areaCenter.y - halfY, areaCenter.y + halfY);

        return pos;
    }
}

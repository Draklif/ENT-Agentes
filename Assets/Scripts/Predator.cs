using UnityEngine;

//Con ayuda de la ia para generar este codigo(CLAUDE)


public class Predator : MonoBehaviour
{
    [Header("Predator Settings")]
    public float energy = 10;
    public float age = 0;
    public float maxAge = 20;
    public float speed = 1f;
    public float visionRange = 5f;

    [Header("Resting - Descanso en madrigueras")]
    // Si no se asigna una madriguera (den) en el Inspector, el depredador
    // descansará en el mismo punto donde se encuentre. La lógica vive en
    // la clase DescansoDepredador (ver DescansoDepredador.cs).
    public Transform den;
    public float restTime = 5f;
    private DescansoDepredador resting;

    [Header("Attacks - Ataques fallidos")]
    // Probabilidad (0 a 1) de que el depredador falle su ataque aunque
    // haya alcanzado al conejo. La lógica vive en la clase PredatorAttack
    // (ver PredatorAttack.cs).
    [Range(0f, 1f)]
    public float attackMissChance = 0.25f;
    private PredatorAttack attack;

    // Tras fallar un ataque, el depredador no puede re-atacar de inmediato:
    // debe esperar este tiempo (en segundos de simulación), dándole al
    // conejo una ventana real para alejarse en vez de ser re-atacado en
    // el siguiente tick.
    public float missedAttackCooldown = 2f;
    private float attackCooldownTimer = 0f;

    [Header("Predator States")]
    public bool isAlive = true;
    public PredatorState currentState = PredatorState.Exploring;

    private Vector3 destination;
    private float h;

    private void Start()
    {
        destination = transform.position;

        // Se instancian aquí (y no como campos inicializados en la
        // declaración) porque dependen de valores configurados en el
        // Inspector (den, restTime, attackMissChance).
        resting = new DescansoDepredador(den, restTime);
        attack = new PredatorAttack(attackMissChance);
    }

    public void Simulate(float h)
    {
        if (!isAlive) return;

        this.h = h;

        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= h;
        }

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
            case PredatorState.Resting:
                Rest();
                break;
        }

        Move();
        Age();
        CheckState();
    }

    void Explore()
    {
        // Si hay comida a la vista, cambiar de estado
        Bunny nearestBunny = FindNearestBunny();
        if (nearestBunny != null)
        {
            currentState = PredatorState.SearchingFood;
            destination = nearestBunny.transform.position;
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
        Bunny nearestBunny = FindNearestBunny();
        if (nearestBunny == null)
        {
            // Si no hay comida, volver a explorar
            currentState = PredatorState.Exploring;
            return;
        }

        destination = nearestBunny.transform.position;

        // Solo pasa a Eating (intentar atacar) si está lo bastante cerca
        // Y ya no está en cooldown por un ataque fallido reciente. Así el
        // conejo tiene una ventana real para alejarse tras un fallo, en
        // vez de que el depredador reintente en el siguiente tick.
        if (Vector3.Distance(transform.position, nearestBunny.transform.position) < 0.2f
            && attackCooldownTimer <= 0f)
        {
            currentState = PredatorState.Eating;
        }
    }

    // FEATURE: Ataques fallidos -> delega en PredatorAttack.AttackFails()
    void Eat()
    {
        Collider2D foodHit = Physics2D.OverlapCircle(transform.position, 0.2f, LayerMask.GetMask("Bunnies"));
        if (foodHit != null)
        {
            Bunny food = foodHit.GetComponent<Bunny>();
            if (food != null)
            {
                if (attack.AttackFails())
                {
                    Debug.Log($"{name} falló el ataque contra {food.name}, el conejo escapa.");
                    food.currentState = BunnyState.Fleeing;
                    attackCooldownTimer = missedAttackCooldown;

                    // Si el depredador quedó prácticamente encima del conejo
                    // (distancia casi 0), Bunny.Flee() no tiene una dirección
                    // válida hacia dónde huir (el vector se normaliza a ~0 y
                    // el conejo se queda quieto). Lo empujamos un poco lejos
                    // del depredador para asegurar que sí tenga a dónde huir.
                    Vector3 pushDir = food.transform.position - transform.position;
                    pushDir = pushDir.sqrMagnitude > 0.0001f
                        ? pushDir.normalized
                        : Random.insideUnitCircle.normalized;
                    food.transform.position += pushDir * 0.5f;
                }
                else
                {
                    Debug.Log($"{name} cazó con éxito a {food.name}.");
                    energy += food.age;
                    Destroy(food.gameObject);
                }
            }
        }

        // Después de intentar comer (haya fallado o no) vuelve a explorar
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

        // Mientras descansa en la madriguera no gasta energía por
        // "movimiento": si no fuera así, la energía seguiría bajando
        // incluso quieto y el depredador nunca lograría recuperarse ni
        // alejarse de la madriguera antes de volver a quedar bajo el
        // umbral de descanso (quedaría en bucle entrando y saliendo).
        if (currentState != PredatorState.Resting)
        {
            energy -= speed * h;
        }
    }

    void Age()
    {
        age += h;
    }

    void CheckState()
    {
        // Si está bajo de energía y no está ya descansando → ir a descansar
        if (energy < 3f && currentState != PredatorState.Resting)
        {
            currentState = PredatorState.Resting;
            return;
        }

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

    // FEATURE: Descanso en madrigueras -> delega en DescansoDepredador
    void Rest()
    {
        Vector3 restPoint = resting.GetRestPoint(transform.position);
        bool hasArrived = Vector3.Distance(transform.position, restPoint) <= 0.2f;

        if (!hasArrived)
        {
            destination = restPoint;
            return;
        }

        destination = transform.position;

        bool finishedResting = resting.Tick(h, hasArrived: true);
        if (finishedResting)
        {
            energy = 15f; // recupera energía al terminar de descansar
            currentState = PredatorState.Exploring;
            Debug.Log($"{name} terminó de descansar en la madriguera.");
        }
    }
}


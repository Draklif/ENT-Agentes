using UnityEngine;

public class RabbitBehavior : MonoBehaviour
{
    public enum State { Normal, Flee, Rest }

    [Header("Movimiento")]
    [SerializeField] float wanderSpeed = 2.5f;
    [SerializeField] float fleeSpeed = 5.5f;
    [SerializeField] float turnSpeed = 180f;

    [Header("Energía")]
    [SerializeField] float maxEnergy = 100f;
    [SerializeField] float energy = 100f;
    [SerializeField] float energyDrainPerSecond = 4f;      // gasto al moverse
    [SerializeField] float restRecoveryPerSecond = 8f;     // recuperación al descansar
    [SerializeField] float restEnterThreshold = 20f;       // <20 entra a descanso
    [SerializeField] float restExitThreshold = 50f;        // >=50 sale de descanso

    [Header("Percepción y Comunicación")]
    [SerializeField] float predatorDetectRadius = 10f;     // si depredador entra aquí -> huye
    [SerializeField] float communicationRadius = 12f;      // avisa a conejos cercanos
    [SerializeField] float fleeDurationOnAlert = 3.0f;     // tiempo huyendo por “aviso”
    [SerializeField] LayerMask whatIsRabbit;               // capa o Default, se usa con tags también

    [Header("Depredadores")]
    [SerializeField] string predatorTag = "Predator";

    // Estado interno
    State state = State.Normal;
    float fleeTimer = 0f;
    Vector3 wanderDir;

    void Start()
    {
        // Dirección de deambular inicial
        wanderDir = Random.insideUnitSphere;
        wanderDir.y = 0f;
        wanderDir.Normalize();

        // Normaliza energía al inicio
        energy = Mathf.Clamp(energy, 0f, maxEnergy);
    }

    void Update()
    {
        // 1) Si ve depredador -> Flee + comunicar alerta
        if (DetectPredator(out Vector3 awayDir))
        {
            EnterFlee();
            BroadcastAlert();
            Move(awayDir, fleeSpeed);
            DrainEnergy();
            return;
        }

        // 2) Si recibió alerta previamente, mantener huida un rato
        if (state == State.Flee)
        {
            fleeTimer -= Time.deltaTime;
            if (fleeTimer <= 0f)
            {
                state = State.Normal;
            }
            Move(transform.forward, fleeSpeed); // sigue avanzando rápido
            DrainEnergy();
            return;
        }

        // 3) Reglas de Descanso por energía
        if (energy < restEnterThreshold)
        {
            state = State.Rest;
        }
        if (state == State.Rest)
        {
            Rest();
            return;
        }
        if (energy >= restExitThreshold && state == State.Rest)
        {
            state = State.Normal;
        }

        // 4) Comportamiento Normal (deambular)
        Wander();
        DrainEnergy();
    }

    bool DetectPredator(out Vector3 awayDir)
    {
        awayDir = Vector3.zero;
        Collider[] hits = Physics.OverlapSphere(transform.position, predatorDetectRadius);
        float closest = float.MaxValue;
        Transform closestPred = null;

        foreach (var h in hits)
        {
            if (h.CompareTag(predatorTag))
            {
                float d = Vector3.Distance(transform.position, h.transform.position);
                if (d < closest)
                {
                    closest = d;
                    closestPred = h.transform;
                }
            }
        }

        if (closestPred != null)
        {
            Vector3 dir = (transform.position - closestPred.position);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f) awayDir = dir.normalized;
            return true;
        }
        return false;
    }

    void BroadcastAlert()
    {
        // Comunica a otros conejos cercanos que deben huir
        Collider[] hits = Physics.OverlapSphere(transform.position, communicationRadius);
        foreach (var h in hits)
        {
            if (h != null && h.gameObject != this.gameObject && h.CompareTag("Rabbit"))
            {
                var other = h.GetComponent<RabbitBehavior>();
                if (other != null) other.ReceiveAlert();
            }
        }
    }

    public void ReceiveAlert()
    {
        if (state != State.Flee)
        {
            EnterFlee();
        }
    }

    void EnterFlee()
    {
        state = State.Flee;
        fleeTimer = fleeDurationOnAlert;
    }

    void Move(Vector3 dir, float speed)
    {
        if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;
        // gira hacia la dirección objetivo
        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        // avanza
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void Wander()
    {
        // cambiar ligeramente la dirección con el tiempo
        Vector2 jitter = Random.insideUnitCircle * 0.6f * Time.deltaTime;
        wanderDir += new Vector3(jitter.x, 0f, jitter.y);
        wanderDir.y = 0;
        wanderDir.Normalize();

        Move(wanderDir, wanderSpeed);
    }

    void Rest()
    {
        // No moverse; recuperar energía
        energy = Mathf.Min(maxEnergy, energy + restRecoveryPerSecond * Time.deltaTime);
    }

    void DrainEnergy()
    {
        // Gasta energía si se mueve (no en descanso)
        if (state != State.Rest)
        {
            energy = Mathf.Max(0f, energy - energyDrainPerSecond * Time.deltaTime);
        }
    }

    // Gizmos para ver radios en el editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, predatorDetectRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, communicationRadius);
    }
}

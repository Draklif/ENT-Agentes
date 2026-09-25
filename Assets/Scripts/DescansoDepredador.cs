using UnityEngine;

// ---------------------------------------------------------------------
// FEATURE: Descanso en madrigueras
//
// Encapsula la lógica de a dónde debe dirigirse el depredador para
// descansar y cuánto tiempo debe permanecer quieto antes de recuperar
// energía. Si no se le asigna una madriguera (den), el depredador
// descansa en su propia posición actual en vez de quedar bloqueado.
// ---------------------------------------------------------------------
public class DescansoDepredador
{
    private readonly Transform den;
    private readonly float restTime;
    private float restTimer = 0f;

    public DescansoDepredador(Transform den, float restTime)
    {
        this.den = den;
        this.restTime = restTime;
    }

    // Punto al que el depredador debe moverse mientras descansa.
    public Vector3 GetRestPoint(Vector3 currentPosition)
    {
        return den != null ? den.position : currentPosition;
    }

    // Debe llamarse una vez por Simulate() mientras currentState == Resting.
    // hasArrived indica si el depredador ya está sobre el punto de descanso.
    // Devuelve true cuando terminó de descansar (y reinicia el temporizador).
    public bool Tick(float deltaSimTime, bool hasArrived)
    {
        if (!hasArrived)
        {
            // Aún viajando hacia la madriguera: no acumula tiempo de descanso.
            return false;
        }

        restTimer += deltaSimTime;

        if (restTimer >= restTime)
        {
            restTimer = 0f;
            return true;
        }

        return false;
    }
}

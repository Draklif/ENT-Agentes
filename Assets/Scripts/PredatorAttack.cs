using UnityEngine;

// ---------------------------------------------------------------------
// FEATURE: Ataques fallidos
//
// Encapsula la lógica de si un ataque del depredador contra un conejo
// tiene éxito o falla. No depende de Predator ni de Bunny directamente:
// solo recibe una probabilidad de fallo y responde con un booleano,
// lo que la hace fácil de reutilizar o testear por separado.
// ---------------------------------------------------------------------
public class PredatorAttack
{
    // Probabilidad (0 a 1) de que el ataque falle aunque el depredador
    // haya alcanzado al conejo.
    private readonly float missChance;

    public PredatorAttack(float missChance)
    {
        this.missChance = Mathf.Clamp01(missChance);
    }

    // Devuelve true si el ataque falla (el conejo debería escapar).
    // Devuelve false si el ataque tiene éxito (el conejo es cazado).
    public bool AttackFails()
    {
        return Random.value < missChance;
    }
}

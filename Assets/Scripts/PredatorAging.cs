using UnityEngine;

public class PredatorAging : MonoBehaviour
{
    public float maxAge = 60f; // segundos de vida
    private float age = 0f;

    void Update()
    {
        age += Time.deltaTime;

        if (age >= maxAge)
        {
            Destroy(gameObject); // muere por envejecimiento
        }
    }
}


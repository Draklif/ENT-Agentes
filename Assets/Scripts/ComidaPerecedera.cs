using UnityEngine;

public class ComidaPerecedde : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float lifetime = 10f; // Tiempo en segundos antes de desaparecer

    private float timer;

    void Start()
    {
        timer = lifetime;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            Destroy(gameObject); // Destruye la comida cuando caduca
        }
    }
}

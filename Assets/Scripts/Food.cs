using UnityEngine;

public class Food : MonoBehaviour
{
    [Header("Food Settings")]
    public int nutrition = 5;
    public float lifetimeSeconds = 5f;

    [Header("Food Status")]
    public bool isRotten = false;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Después de X segundos, la comida se pudre.
        Invoke(nameof(RotFood), lifetimeSeconds);
    }

    void RotFood()
    {
        isRotten = true;
        nutrition = 0;

        // La comida cambia a color marrón para mostrar que está podrida.
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.45f, 0.25f, 0.10f);
        }
    }
}
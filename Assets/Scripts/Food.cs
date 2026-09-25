using UnityEngine;

public class Food : MonoBehaviour
{
    public float nutrition = 5; //Cantidad de energía que recupera el conejo
    public float lifetimeSeconds = 10f; //Tiempo que tarda la comida en pudrirse
    public bool isRotten = false; //Indica si la comida está podrida

    private float lifetimeTimer = 0f; //Contador del tiempo de vida de la comida
    private SpriteRenderer spriteRenderer; //Sprite de la comida

    private void Start()
    {
        //Obtiene el componente encargado de mostrar el sprite
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        //Solo aumenta el tiempo mientras la comida esté fresca
        if (!isRotten)
        {
            lifetimeTimer += Time.deltaTime;

            //Cuando alcanza el tiempo establecido, la comida se pudre
            if (lifetimeTimer >= lifetimeSeconds)
            {
                BecomeRotten();
            }
        }
    }

    private void BecomeRotten()
    {
        isRotten = true; //Marca la comida como podrida
        nutrition = 0f; //La comida podrida ya no aporta energía

        //Cambia el color para mostrar que la comida está podrida
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.4f, 0.2f, 0.1f);
        }
    }
}
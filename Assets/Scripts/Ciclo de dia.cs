using UnityEngine;

public class Ciclodedia : MonoBehaviour
{
  
    public float dayLength = 20f;          // valor del dia
    public SpriteRenderer overlay;         // sprite para colocar el color
    public Color dayColor = new Color(0, 0, 0, 0.6f);
    public Color nightColor = new Color(0, 0, 0, 0.6f);
    public bool isDay;

    private float time;

    public void Simulate(float h)
    {
        time = (time + h) % (dayLength * 2);   // tiempo que se transcurre para el dia y la noche

        // para saber si es de dia o noche, cuando el valor de dia es menor se vueleve de noche
        if (time < dayLength)
        {
            isDay = true;
        }
        else
        {
            isDay = false;
        }

        if (!isDay)
        {
            overlay.color = nightColor;
        }
        else
        {
            overlay.color = dayColor;
        }

    }
}

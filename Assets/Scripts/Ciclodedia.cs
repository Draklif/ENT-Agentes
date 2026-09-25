using UnityEngine;

public class Ciclodedia : MonoBehaviour
{
  
    public float dayLength = 20f; // valor del dia
    public SpriteRenderer overlay; // sprite para colocar el color
    public Color dayColor = new Color(156, 159, 34, 177);
    public Color nightColor = new Color(69, 69, 180, 224);
    public bool esDia;

    private float time;

    public void CiclodeDia()
    {
        time = (time + Time.deltaTime) % (dayLength * 2); // tiempo que se transcurre para el dia y la noche

        // para saber si es de dia o noche, cuando el valor de dia es menor se vuelve de noche
        if (time < dayLength)
        {
            esDia = true;
        }
        else
        {
            esDia = false;
        }

        if (!esDia)
        {
            overlay.color = nightColor;
        }
        else
        {
            overlay.color = dayColor;
        }

    }
}

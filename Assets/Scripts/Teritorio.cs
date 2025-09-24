using UnityEngine;

public class Territorio : MonoBehaviour
{
    public float radius = 5f; 

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius); // Dibujar el circulo del territorio.
    }
}

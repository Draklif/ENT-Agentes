using UnityEngine;

public class BunnyAge : MonoBehaviour
{
    public float minSpeed= 0.3f;  
    private Bunny bunny;
    private float baseSpeed;

    void Start()
    {
        bunny = GetComponent<Bunny>();
        baseSpeed = bunny.speed;
    }

    void Update()
    {
        bunny.speed = Mathf.Lerp(baseSpeed, baseSpeed * minSpeed, bunny.age / bunny.maxAge);
    }
}

using UnityEngine;

public class ComidaPerecedera : MonoBehaviour
{
    [Header("Duración en ticks antes de desaparecer")]
    public int lifetimeTicks = 10;
    private int ticksAlive = 0;

    public void Simulate()
    {
        ticksAlive++;

        if (ticksAlive >= lifetimeTicks)
        {
            Destroy(gameObject);
        }
    }
}


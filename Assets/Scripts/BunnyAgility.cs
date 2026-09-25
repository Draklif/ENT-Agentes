using UnityEngine;

[RequireComponent(typeof(Bunny))]
public class BunnyAgility : MonoBehaviour
{
    [Header("Agility Settings")]
    [Min(1f)]
    public float speedMultiplier = 2f;

    [Min(0.1f)]
    public float agilityDuration = 3f;

    public bool IsAgilityActive { get; private set; }

    private Bunny bunny;

    private float normalSpeed;
    private float remainingTime;

    private bool predatorWasInRange = false;

    private int foxLayerMask;

    private void Awake()
    {
        bunny = GetComponent<Bunny>();

        normalSpeed = bunny.speed;

        foxLayerMask = LayerMask.GetMask("Foxes");
    }

    public void Simulate(float deltaTime)
    {
        if (bunny == null || !bunny.isAlive)
            return;

        bool predatorInRange = Physics2D.OverlapCircle(
            (Vector2)transform.position,
            bunny.visionRange,
            foxLayerMask
        ) != null;

        // El depredador acaba de entrar al rango del conejo
        if (predatorInRange &&
            !predatorWasInRange &&
            !IsAgilityActive)
        {
            ActivateAgility();
        }

        predatorWasInRange = predatorInRange;

        if (IsAgilityActive)
        {
            remainingTime -= deltaTime;

            if (remainingTime <= 0f)
            {
                DeactivateAgility();
            }
        }
    }

    private void ActivateAgility()
    {
        normalSpeed = bunny.speed;

        bunny.speed = normalSpeed * speedMultiplier;

        remainingTime = agilityDuration;

        IsAgilityActive = true;

        Debug.Log(
            $"AGILIDAD ACTIVADA - Velocidad: {normalSpeed} -> {bunny.speed}"
        );
    }

    private void DeactivateAgility()
    {
        bunny.speed = normalSpeed;

        IsAgilityActive = false;

        Debug.Log(
            $"AGILIDAD FINALIZADA - Velocidad restaurada a {normalSpeed}"
        );
    }

    private void OnDisable()
    {
        if (IsAgilityActive && bunny != null)
        {
            bunny.speed = normalSpeed;
            IsAgilityActive = false;
        }
    }
}
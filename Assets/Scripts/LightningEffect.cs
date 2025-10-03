using UnityEngine;
using UnityEngine.UI;

public class LightningEffect : MonoBehaviour
{
    private Image img;
    public float minDelay = 3f;
    public float maxDelay = 8f;
    public float flashDuration = 0.2f;

    private float timer;
    private bool isActive = false;

    void Start()
    {
        img = GetComponent<Image>();
        SetAlpha(0);

        
        ClimateManager.Instance.OnClimateChanged += HandleClimateChange;
    }

    void Update()
    {
        if (!isActive) return; // solo funciona si hay tormenta

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            StartCoroutine(Flash());
            timer = Random.Range(minDelay, maxDelay);
        }
    }

    private void HandleClimateChange(ClimateState state)
    {
        if (state == ClimateState.Storm)
        {
            isActive = true;
            timer = Random.Range(minDelay, maxDelay);
        }
        else
        {
            isActive = false;
            SetAlpha(0); // por si queda a medio flash
        }
    }

    private System.Collections.IEnumerator Flash()
    {
        SetAlpha(1f);
        yield return new WaitForSeconds(flashDuration);
        SetAlpha(0f);
    }

    void SetAlpha(float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}



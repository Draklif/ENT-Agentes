using UnityEngine;

public class FertilidadFoodSpawner : FoodSpawner
{
    [Header("Zonas de Fertilidad")]
    public FertilidadZonas[] zones;

    // 🔹 Sobrescribe el SpawnFood del padre para usar zonas
    protected override void SpawnFood()
    {
        if (zones == null || zones.Length == 0)
        {
            base.SpawnFood(); // fallback a spawn normal
            return;
        }

        // Elegir una zona según fertilidad (peso probabilístico)
        FertilidadZonas selectedZone = ChooseZone();

        // Elegir un punto aleatorio dentro de esa zona
        float x = Random.Range(selectedZone.area.xMin, selectedZone.area.xMax);
        float y = Random.Range(selectedZone.area.yMin, selectedZone.area.yMax);

        Vector2 spawnPos = new Vector2(x, y);

        Instantiate(foodPrefab, spawnPos, Quaternion.identity);
    }

    // Método auxiliar para elegir zona según probabilidad
    private FertilidadZonas ChooseZone()
    {
        float total = 0f;
        foreach (var zone in zones) total += zone.fertility;

        float r = Random.Range(0, total);
        float acum = 0f;

        foreach (var zone in zones)
        {
            acum += zone.fertility;
            if (r <= acum) return zone;
        }

        return zones[0]; // fallback por seguridad
    }

    // 🔹 Sobrescribimos el Gizmos del padre para dibujar también las zonas
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected(); // dibuja el área del spawner normal

        if (zones != null)
        {
            foreach (var zone in zones)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(zone.area.center, zone.area.size);
            }
        }
    }
}


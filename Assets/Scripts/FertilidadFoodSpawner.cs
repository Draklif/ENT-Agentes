using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FertilidadZona
{
    public Rect area;        // Área rectangular donde puede aparecer comida
    [Range(0, 10)]
    public int fertility;    // Nivel de fertilidad (0 = baja, 10 = alta)
}

public class FertilidadFoodSpawner : FoodSpawner
{
    [Header("Zonas de Fertilidad")]
    public List<FertilidadZona> FertilidadZonas = new List<FertilidadZona>();

    protected override void SpawnFood()
    {
        if (CountFood() >= maxFood || FertilidadZonas.Count == 0) return;

        // Elegir una zona según su fertilidad (peso)
        FertilidadZona selectedZone = ElegirZonaPorFertilidad();

        // Generar posición aleatoria dentro de esa zona
        float x = Random.Range(selectedZone.area.xMin, selectedZone.area.xMax);
        float y = Random.Range(selectedZone.area.yMin, selectedZone.area.yMax);
        Vector2 spawnPos = new Vector2(x, y);

        Instantiate(foodPrefab, spawnPos, Quaternion.identity);
    }

    private FertilidadZona ElegirZonaPorFertilidad()
    {
        int totalWeight = 0;
        foreach (var zone in FertilidadZonas)
            totalWeight += zone.fertility;

        int rand = Random.Range(0, totalWeight);
        int cumulative = 0;

        foreach (var zone in FertilidadZonas)
        {
            cumulative += zone.fertility;
            if (rand < cumulative)
                return zone;
        }

        return FertilidadZonas[0]; // fallback
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (FertilidadZonas != null)
        {
            foreach (var zone in FertilidadZonas)
            {
                // Colorear según fertilidad
                if (zone.fertility >= 5)
                    Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // Verde (alta fertilidad)
                else
                    Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // Amarillo (baja fertilidad)

                Gizmos.DrawCube(zone.area.center, zone.area.size);

                // Contorno blanco
                Gizmos.color = Color.white;
                Gizmos.DrawWireCube(zone.area.center, zone.area.size);
            }
        }
    }
}




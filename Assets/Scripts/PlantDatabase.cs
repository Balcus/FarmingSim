using UnityEngine;
using System.Collections.Generic;

public class PlantDatabase : MonoBehaviour
{
    public static PlantDatabase Instance;

    public PlantData[] plants;

    private Dictionary<string, PlantData> lookup =
        new Dictionary<string, PlantData>();

    private void Awake()
    {
        Instance = this;

        if (plants == null) return;

        foreach (PlantData plant in plants)
        {
            if (plant == null || string.IsNullOrEmpty(plant.plantName)) continue;
            lookup[plant.plantName] = plant;
        }
    }

    public PlantData GetPlant(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        lookup.TryGetValue(name, out PlantData plant);
        return plant;
    }
}

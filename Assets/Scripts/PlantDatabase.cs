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

        foreach (PlantData plant in plants)
            lookup.Add(plant.plantName, plant);
    }

    public PlantData GetPlant(string name)
    {
        return lookup[name];
    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "PlantData", menuName = "Garden/Plant Data")]
public class PlantData : ScriptableObject
{
    public string plantName;
    [TextArea] public string description;
    public Sprite seedPacketSprite;

    [Header("Growth")]
    public int   growthDays      = 10;  // days from seed to harvest
    public int   harvestWindow   = 5;   // days before it rots
    public float fertilizerBoost = 0.3f; // 30% faster growth

    [Header("Planting Season")]
    public int plantMonthStart = 3;
    public int plantMonthEnd   = 5;

    [Header("Requirements")]
    public bool needsFullSun    = true;
    public int  wateringEveryDays = 2; // how often it needs water

    [Header("Prefabs (assign 3D models)")]
    public GameObject seedlingPrefab;
    public GameObject growingPrefab;
    public GameObject ripePrefab;
    public GameObject rottingPrefab;

    [Header("Companions")]
    public string[] companionPlants; // names of companion plants

    [Header("Fun Fact")]
    [TextArea] public string funFact;
}

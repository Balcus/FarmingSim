using UnityEngine;

public enum TileState { Unplowed, Plowed, Seeded, Growing, Harvestable, Rotting, Dead }

/// <summary>
/// Step 3.5 from the guide: one farming tile with its own state machine.
/// Attach this to existing dirt objects or to the generated GardenTile prefab.
/// </summary>
public class GardenTile : MonoBehaviour
{
    [Header("State")]
    public TileState state = TileState.Unplowed;
    public PlantData currentPlant;

    [Header("Timers")]
    [SerializeField] private float growthProgress = 0f;
    private int daysSinceRipe = 0;
    private int daysSinceWatered = 0;

    [Header("Flags")]
    public bool hasBugs = false;
    public bool isFertilized = false;
    public bool isWatered = true;

    [Header("Soil Quality")]
    [Range(0, 100)] public float soilQuality = 100f;
    public string lastPlantName = "";

    [Header("References")]
    public MeshRenderer soilRenderer;
    public Material unplowedMat;
    public Material plowedMat;
    private GameObject plantModel;

    [Header("Indicators")]
    public GameObject bugIndicator;
    public GameObject waterIndicator;

    private void Awake()
    {
        if (soilRenderer == null) soilRenderer = GetComponent<MeshRenderer>();
        RefreshVisuals();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.TimeChangedEvent += OnDayPassed;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.TimeChangedEvent -= OnDayPassed;
    }

    public bool TryPlow()
    {
        if (state != TileState.Unplowed)
        {
            Message("This tile is already prepared.");
            return false;
        }

        state = TileState.Plowed;
        isWatered = false;
        RefreshVisuals();
        Message("Soil plowed.");
        return true;
    }

    public bool TryPlant(PlantData plant)
    {
        if (plant == null)
        {
            Message("Select a seed first.");
            return false;
        }

        if (state != TileState.Plowed)
        {
            Message("Plow the soil before planting.");
            return false;
        }

        int month = GameManager.Instance != null ? GameManager.Instance.currentMonth : 3;
        if (month < plant.plantMonthStart || month > plant.plantMonthEnd)
        {
            Message(plant.plantName + " grows best in months " + plant.plantMonthStart + "-" + plant.plantMonthEnd + ".");
            return false;
        }

        if (lastPlantName == plant.plantName)
            Message("Crop rotation warning: repeated crops reduce soil health.");

        currentPlant = plant;
        state = TileState.Seeded;
        growthProgress = 0f;
        daysSinceRipe = 0;
        daysSinceWatered = 0;
        isWatered = false;
        hasBugs = false;
        isFertilized = false;
        lastPlantName = plant.plantName;
        SpawnModel(plant.seedlingPrefab, "Seedling");
        RefreshVisuals();
        Message("Planted " + plant.plantName + ".");
        return true;
    }

    public void ApplyWater()
    {
        if (state == TileState.Unplowed || state == TileState.Dead)
        {
            Message("There is nothing useful to water here.");
            return;
        }

        isWatered = true;
        daysSinceWatered = 0;
        UpdateIndicators();
        Message("Watered tile.");
    }

    public bool TryApplyInsecticide()
    {
        if (!hasBugs)
        {
            Message("No bugs on this plant.");
            return false;
        }

        hasBugs = false;
        UpdateIndicators();
        Message("Bugs removed.");
        return true;
    }

    public bool TryApplyFertilizer()
    {
        if (state == TileState.Unplowed || state == TileState.Harvestable || state == TileState.Rotting || state == TileState.Dead)
        {
            Message("Fertilizer works on planted/growing crops, not empty or dead soil.");
            return false;
        }

        isFertilized = true;
        Message("Fertilizer applied.");
        return true;
    }

    public bool TryHarvest()
    {
        if (state != TileState.Harvestable)
        {
            Message("Nothing ready to harvest yet.");
            return false;
        }

        string cropName = currentPlant != null ? currentPlant.plantName : "crop";
        soilQuality = Mathf.Clamp(soilQuality - 10f, 0f, 100f);
        ResetTileAfterHarvest();
        Message("Harvested " + cropName + ".");
        return true;
    }

    private void OnDayPassed(int d, int m, int y)
    {
        if (state == TileState.Unplowed || state == TileState.Plowed || state == TileState.Dead || currentPlant == null)
            return;

        HandleWatering();
        HandleBugs();
        HandleGrowth();
        RefreshVisuals();
    }

    private void HandleWatering()
    {
        if (WeatherSystem.Instance != null && WeatherSystem.Instance.IsRaining())
        {
            isWatered = true;
            daysSinceWatered = 0;
            return;
        }

        daysSinceWatered++;
        if (daysSinceWatered >= currentPlant.wateringEveryDays)
            isWatered = false;
    }

    private void HandleBugs()
    {
        if (!hasBugs && Random.value < 0.05f)
        {
            hasBugs = true;
            Message(currentPlant.plantName + " has bugs. Use insecticide.");
        }
    }

    private void HandleGrowth()
    {
        if (hasBugs || !isWatered) return;

        float growthSpeed = 1f;
        if (isFertilized) growthSpeed += currentPlant.fertilizerBoost;
        growthSpeed *= Mathf.Clamp01(soilQuality / 100f);
        growthProgress += growthSpeed;
        isFertilized = false;

        if (state == TileState.Seeded && growthProgress >= currentPlant.growthDays / 3f)
        {
            state = TileState.Growing;
            SpawnModel(currentPlant.growingPrefab, "Growing");
        }

        if (state == TileState.Growing && growthProgress >= currentPlant.growthDays)
        {
            state = TileState.Harvestable;
            SpawnModel(currentPlant.ripePrefab, "Ripe");
            Message(currentPlant.plantName + " is ready to harvest.");
        }

        if (state == TileState.Harvestable)
        {
            daysSinceRipe++;
            if (daysSinceRipe >= currentPlant.harvestWindow)
            {
                state = TileState.Rotting;
                SpawnModel(currentPlant.rottingPrefab, "Rotting");
                Message(currentPlant.plantName + " is rotting. Harvest sooner next time.");
            }
        }
        else if (state == TileState.Rotting)
        {
            daysSinceRipe++;
            if (daysSinceRipe >= currentPlant.harvestWindow + 3)
            {
                state = TileState.Dead;
                DestroyPlantVisual();
                Message(currentPlant.plantName + " died.");
            }
        }
    }

    private void SpawnModel(GameObject prefab, string fallbackName)
    {
        DestroyPlantVisual();

        if (prefab != null)
        {
            plantModel = Instantiate(prefab, transform.position + Vector3.up * 0.12f, Quaternion.identity, transform);
            plantModel.transform.localScale = Vector3.one * 0.35f;
            return;
        }

        plantModel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plantModel.name = "Fallback_" + fallbackName;
        plantModel.transform.SetParent(transform, false);
        plantModel.transform.localPosition = Vector3.up * 0.18f;
        plantModel.transform.localScale = fallbackName == "Ripe" ? new Vector3(0.35f, 0.35f, 0.35f) : new Vector3(0.2f, 0.2f, 0.2f);
    }

    private void DestroyPlantVisual()
    {
        if (plantModel != null)
        {
            if (Application.isPlaying) Destroy(plantModel);
            else DestroyImmediate(plantModel);
        }
    }

    private void RefreshVisuals()
    {
        if (soilRenderer != null)
        {
            if (state == TileState.Unplowed && unplowedMat != null) soilRenderer.sharedMaterial = unplowedMat;
            else if (plowedMat != null) soilRenderer.sharedMaterial = plowedMat;
        }

        UpdateIndicators();
    }

    private void UpdateIndicators()
    {
        if (bugIndicator != null) bugIndicator.SetActive(hasBugs);
        if (waterIndicator != null) waterIndicator.SetActive(currentPlant != null && !isWatered && state != TileState.Harvestable && state != TileState.Dead);
    }

    private void ResetTileAfterHarvest()
    {
        DestroyPlantVisual();
        currentPlant = null;
        state = TileState.Plowed;
        growthProgress = 0f;
        daysSinceRipe = 0;
        daysSinceWatered = 0;
        hasBugs = false;
        isWatered = false;
        isFertilized = false;
        RefreshVisuals();
    }

    private void Message(string text)
    {
        if (UIManager.Instance != null) UIManager.Instance.ShowMessage(text);
        else Debug.Log(text);
    }
}

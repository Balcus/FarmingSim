using UnityEngine;

public enum TileState { Unplowed, Plowed, Seeded, Growing, Harvestable, Rotting, Dead }

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
        AudioManager.Instance?.PlayPlant();
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

        if (currentPlant == null)
        {
            Message("This crop is missing its plant data.");
            return false;
        }

        if (InventoryManager.Instance == null)
        {
            Message("Inventory is not ready.");
            return false;
        }

        int harvestAmount =
            Random.Range(
                currentPlant.minHarvestAmount,
                currentPlant.maxHarvestAmount + 1);

        string cropName = currentPlant.plantName;

        InventoryManager.Instance.AddVegetable(
            cropName,
            harvestAmount);

        soilQuality = Mathf.Clamp(
            soilQuality - 10f,
            0f,
            100f);

        ResetTileAfterHarvest();

        Message(
            "Harvested " +
            harvestAmount +
            " " +
            cropName +
            "!");

        AudioManager.Instance?.PlayHarvest();

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
            plantModel = Instantiate(prefab, transform);
            plantModel.name = GetPlantVisualName(fallbackName);
            PreparePlantVisual(plantModel);
            ConfigurePlantVisual(plantModel.transform, fallbackName);
            return;
        }

        plantModel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plantModel.name = "Fallback_" + fallbackName;
        plantModel.transform.SetParent(transform, false);
        ConfigurePlantVisual(plantModel.transform, fallbackName);
        TintFallbackPlant(plantModel, fallbackName);
    }

    private string GetPlantVisualName(string stageName)
    {
        string cropName = currentPlant != null ? currentPlant.plantName : "Plant";
        return cropName + "_" + stageName + "_Visual";
    }

    private void PreparePlantVisual(GameObject visual)
    {
        foreach (Rigidbody body in visual.GetComponentsInChildren<Rigidbody>())
        {
            body.useGravity = false;
            body.isKinematic = true;
            body.detectCollisions = false;
        }

        foreach (Collider plantCollider in visual.GetComponentsInChildren<Collider>())
            plantCollider.enabled = false;
    }

    private void ConfigurePlantVisual(Transform visual, string stageName)
    {
        string cropName = currentPlant != null ? currentPlant.plantName.ToLowerInvariant() : "";
        bool isSeedling = stageName == "Seedling";
        bool isGrowing = stageName == "Growing";
        bool isRipe = stageName == "Ripe";
        bool isRotting = stageName == "Rotting";

        Vector3 localPosition = new Vector3(0f, 0.12f, 0f);
        Vector3 localScale = Vector3.one * 0.42f;
        Quaternion localRotation = Quaternion.Euler(0f, GetStableYaw(), 0f);

        if (cropName.Contains("tomato"))
        {
            localPosition = new Vector3(0f, isRipe ? 0.22f : 0.08f, 0f);
            localScale = Vector3.one * (isSeedling ? 0.32f : isGrowing ? 0.58f : isRotting ? 0.38f : 0.78f);
        }
        else if (cropName.Contains("carrot"))
        {
            localPosition = new Vector3(0f, isRipe ? 0.18f : 0.08f, 0f);
            localScale = Vector3.one * (isSeedling ? 0.42f : isGrowing ? 0.58f : isRotting ? 0.4f : 0.8f);
            if (isRipe)
                localRotation *= Quaternion.Euler(0f, 0f, -12f);
        }
        else if (cropName.Contains("lettuce") || cropName.Contains("cabbage"))
        {
            localPosition = new Vector3(0f, isRipe ? 0.14f : 0.08f, 0f);
            localScale = Vector3.one * (isSeedling ? 0.35f : isGrowing ? 0.62f : isRotting ? 0.45f : 0.76f);
        }

        visual.localPosition = localPosition;
        visual.localRotation = localRotation;
        visual.localScale = localScale;
    }

    private float GetStableYaw()
    {
        float seed = transform.position.x * 17f + transform.position.z * 23f;
        return Mathf.Repeat(seed, 360f);
    }

    private void TintFallbackPlant(GameObject visual, string stageName)
    {
        Renderer visualRenderer = visual.GetComponent<Renderer>();
        if (visualRenderer == null) return;

        string cropName = currentPlant != null ? currentPlant.plantName.ToLowerInvariant() : "";
        Color color = Color.green;

        if (stageName == "Rotting") color = new Color(0.38f, 0.25f, 0.12f);
        else if (cropName.Contains("tomato") && stageName == "Ripe") color = new Color(0.9f, 0.08f, 0.04f);
        else if (cropName.Contains("carrot") && stageName == "Ripe") color = new Color(1f, 0.45f, 0.05f);
        else if (cropName.Contains("lettuce") || cropName.Contains("cabbage")) color = new Color(0.35f, 0.78f, 0.28f);

        visualRenderer.material.color = color;
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

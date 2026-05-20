#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class GG_Step4Installer
{
    private const string Root = "Assets/GardenGuideStep4";
    private const string Generated = Root + "/Generated";
    private const string Materials = Generated + "/Materials";
    private const string Prefabs = Generated + "/Prefabs";
    private const string Plants = Generated + "/PlantData";

    [MenuItem("Garden Guide/Step 4/Install On Current Scene - Existing Dirt Only")]
    public static void InstallOnCurrentScene()
    {
        EnsureFolders();
        EnsureLayer("GardenTile");
        Material unplowed = CreateMaterial("GG_UnplowedSoil", new Color(0.33f, 0.23f, 0.15f));
        Material plowed = CreateMaterial("GG_PlowedSoil", new Color(0.23f, 0.13f, 0.08f));
        Material water = CreateMaterial("GG_WaterBlue", new Color(0.18f, 0.48f, 1.0f));
        Material bug = CreateMaterial("GG_BugRed", new Color(0.85f, 0.1f, 0.08f));
        Material leaf = CreateMaterial("GG_LeafGreen", new Color(0.18f, 0.65f, 0.22f));
        Material ripeRed = CreateMaterial("GG_TomatoRed", new Color(0.85f, 0.05f, 0.04f));
        Material carrotOrange = CreateMaterial("GG_CarrotOrange", new Color(0.95f, 0.38f, 0.08f));
        Material lettuceGreen = CreateMaterial("GG_LettuceGreen", new Color(0.5f, 0.85f, 0.3f));
        Material rot = CreateMaterial("GG_RottingBrown", new Color(0.20f, 0.10f, 0.04f));
        Material dark = CreateMaterial("GG_DarkTransparentHint", new Color(0.05f, 0.04f, 0.035f));

        GameObject tomatoSeedling = CreatePlantStagePrefab("Tomato_Seedling", leaf, null, 0.18f);
        GameObject tomatoGrowing = CreatePlantStagePrefab("Tomato_Growing", leaf, "Tomato", 0.24f);
        GameObject tomatoRipe = CreatePlantStagePrefab("Tomato_Ripe", ripeRed, "Tomato", 0.35f);
        GameObject tomatoRot = CreatePlantStagePrefab("Tomato_Rotting", rot, "Tomato", 0.30f);

        GameObject carrotSeedling = CreatePlantStagePrefab("Carrot_Seedling", leaf, null, 0.18f);
        GameObject carrotGrowing = CreatePlantStagePrefab("Carrot_Growing", leaf, "Carrot", 0.25f);
        GameObject carrotRipe = CreatePlantStagePrefab("Carrot_Ripe", carrotOrange, "Carrot", 0.35f);
        GameObject carrotRot = CreatePlantStagePrefab("Carrot_Rotting", rot, "Carrot", 0.30f);

        GameObject lettuceSeedling = CreatePlantStagePrefab("Lettuce_Seedling", leaf, null, 0.18f);
        GameObject lettuceGrowing = CreatePlantStagePrefab("Lettuce_Growing", lettuceGreen, "Cabbage", 0.25f);
        GameObject lettuceRipe = CreatePlantStagePrefab("Lettuce_Ripe", lettuceGreen, "Cabbage", 0.35f);
        GameObject lettuceRot = CreatePlantStagePrefab("Lettuce_Rotting", rot, "Cabbage", 0.30f);

        PlantData tomato = CreatePlantData("Tomato", "Full sun crop. Keep watering every 2 days.", 4, 8, 12, 6, 2, true, tomatoSeedling, tomatoGrowing, tomatoRipe, tomatoRot,
            "Tomatoes are fruits botanically, but they are usually cooked like vegetables.");
        PlantData carrot = CreatePlantData("Carrot", "Root crop. Good starter crop for spring.", 3, 6, 10, 5, 3, false, carrotSeedling, carrotGrowing, carrotRipe, carrotRot,
            "Carrot roots store sugar, which is why carrots taste sweet.");
        PlantData lettuce = CreatePlantData("Lettuce", "Fast crop that prefers cooler spring weather.", 3, 5, 7, 4, 2, false, lettuceSeedling, lettuceGrowing, lettuceRipe, lettuceRot,
            "Lettuce can become bitter when the weather gets too hot.");

        GameObject tilePrefab = CreateGardenTilePrefab(unplowed, plowed, bug, water);
        GameObject rainPrefab = CreateRainParticlesPrefab();
        GameObject playerPrefab = CreateGuidePlayerPrefab(tilePrefab, tomato);
        GameObject insecticidePrefab = CreateSimpleToolPrefab("GG_InsecticideBottle", new Color(0.1f, 0.8f, 0.25f), PrimitiveType.Cylinder);
        GameObject fertilizerPrefab = CreateSimpleToolPrefab("GG_FertilizerBag", new Color(0.75f, 0.55f, 0.22f), PrimitiveType.Cube);
        GameObject seedPouchPrefab = CreateSimpleToolPrefab("GG_SeedPouch", new Color(0.55f, 0.35f, 0.16f), PrimitiveType.Sphere);

        SetupExistingDirtTiles(unplowed, plowed, bug, water);
        GameObject rainInstance = SetupRainInScene(rainPrefab);
        SetupUIInScene(dark);
        SetupPlayerInScene(tomato, carrot, lettuce, insecticidePrefab, fertilizerPrefab, seedPouchPrefab);
        SetupBootstrap(rainInstance);

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Step 4 installed", "Added Step 4 systems to the current scene using only existing Normaldirt/Ploweddirt objects as farming tiles. No random grid was generated.", "OK");
    }

    [MenuItem("Garden Guide/Step 4/Create Prefabs Only")]
    public static void CreatePrefabsOnly()
    {
        EnsureFolders();
        EnsureLayer("GardenTile");
        Material unplowed = CreateMaterial("GG_UnplowedSoil", new Color(0.33f, 0.23f, 0.15f));
        Material plowed = CreateMaterial("GG_PlowedSoil", new Color(0.23f, 0.13f, 0.08f));
        Material water = CreateMaterial("GG_WaterBlue", new Color(0.18f, 0.48f, 1.0f));
        Material bug = CreateMaterial("GG_BugRed", new Color(0.85f, 0.1f, 0.08f));
        CreateGardenTilePrefab(unplowed, plowed, bug, water);
        CreateRainParticlesPrefab();
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Step 4 prefabs", "Generated Step 4 prefab assets only. Scene was not touched.", "OK");
    }

    private static void EnsureFolders()
    {
        CreateFolderIfMissing(Root);
        CreateFolderIfMissing(Generated);
        CreateFolderIfMissing(Materials);
        CreateFolderIfMissing(Prefabs);
        CreateFolderIfMissing(Plants);
    }

    private static void CreateFolderIfMissing(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace("\\", "/");
        string name = Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent)) CreateFolderIfMissing(parent);
        AssetDatabase.CreateFolder(parent, name);
    }

    private static Material CreateMaterial(string name, Color color)
    {
        string path = Materials + "/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.color = color;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static GameObject CreateGardenTilePrefab(Material unplowed, Material plowed, Material bugMat, Material waterMat)
    {
        string path = Prefabs + "/GG_GardenTile.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
        tile.name = "GG_GardenTile";
        tile.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        tile.layer = LayerMask.NameToLayer("GardenTile");
        MeshRenderer renderer = tile.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = unplowed;
        MeshCollider collider = tile.GetComponent<MeshCollider>();
        if (collider == null) tile.AddComponent<MeshCollider>();

        GardenTile gardenTile = tile.AddComponent<GardenTile>();
        gardenTile.soilRenderer = renderer;
        gardenTile.unplowedMat = unplowed;
        gardenTile.plowedMat = plowed;

        GameObject bug = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bug.name = "BugIndicator";
        bug.transform.SetParent(tile.transform, false);
        bug.transform.localPosition = new Vector3(-0.35f, 0.06f, 0.35f);
        bug.transform.localScale = Vector3.one * 0.12f;
        bug.GetComponent<Renderer>().sharedMaterial = bugMat;
        bug.SetActive(false);
        gardenTile.bugIndicator = bug;

        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        water.name = "WaterIndicator";
        water.transform.SetParent(tile.transform, false);
        water.transform.localPosition = new Vector3(0.35f, 0.06f, 0.35f);
        water.transform.localScale = new Vector3(0.1f, 0.14f, 0.1f);
        water.GetComponent<Renderer>().sharedMaterial = waterMat;
        water.SetActive(false);
        gardenTile.waterIndicator = water;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(tile, path);
        Object.DestroyImmediate(tile);
        return prefab;
    }

    private static GameObject CreateGuidePlayerPrefab(GameObject tilePrefab, PlantData defaultSeed)
    {
        string path = Prefabs + "/GG_Player.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "GG_Player";
        player.AddComponent<CharacterController>();
        PlayerController controller = player.AddComponent<PlayerController>();
        controller.selectedSeed = defaultSeed;
        controller.tomatoSeed = defaultSeed;
        controller.tileLayer = LayerMask.GetMask("GardenTile");

        GameObject camera = new GameObject("Main Camera");
        camera.tag = "MainCamera";
        camera.transform.SetParent(player.transform, false);
        camera.transform.localPosition = new Vector3(0f, 1.5f, -0.25f);
        camera.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
        camera.AddComponent<Camera>();
        camera.AddComponent<AudioListener>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(player, path);
        Object.DestroyImmediate(player);
        return prefab;
    }

    private static GameObject CreatePlantStagePrefab(string name, Material mat, string existingModelName, float scale)
    {
        string path = Prefabs + "/GG_" + name + ".prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        GameObject root = new GameObject("GG_" + name);

        GameObject modelPrefab = FindPrefabByName(existingModelName);
        if (modelPrefab != null)
        {
            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
            model.name = existingModelName + "_Model";
            model.transform.SetParent(root.transform, false);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one * scale;
        }
        else
        {
            GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "Stem";
            stem.transform.SetParent(root.transform, false);
            stem.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            stem.transform.localScale = new Vector3(0.04f, 0.12f, 0.04f);
            stem.GetComponent<Renderer>().sharedMaterial = mat;

            GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leaf.name = "LeafCluster";
            leaf.transform.SetParent(root.transform, false);
            leaf.transform.localPosition = new Vector3(0f, 0.24f, 0f);
            leaf.transform.localScale = new Vector3(scale, 0.05f, scale);
            leaf.transform.localRotation = Quaternion.Euler(0f, 35f, 0f);
            leaf.GetComponent<Renderer>().sharedMaterial = mat;
        }

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static PlantData CreatePlantData(string name, string description, int monthStart, int monthEnd, int growthDays, int harvestWindow, int wateringEveryDays, bool needsSun, GameObject seedling, GameObject growing, GameObject ripe, GameObject rotting, string fact)
    {
        string path = Plants + "/" + name + ".asset";
        PlantData data = AssetDatabase.LoadAssetAtPath<PlantData>(path);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<PlantData>();
            AssetDatabase.CreateAsset(data, path);
        }

        data.plantName = name;
        data.description = description;
        data.growthDays = growthDays;
        data.harvestWindow = harvestWindow;
        data.fertilizerBoost = 0.3f;
        data.plantMonthStart = monthStart;
        data.plantMonthEnd = monthEnd;
        data.needsFullSun = needsSun;
        data.wateringEveryDays = wateringEveryDays;
        data.seedlingPrefab = seedling;
        data.growingPrefab = growing;
        data.ripePrefab = ripe;
        data.rottingPrefab = rotting;
        data.funFact = fact;
        EditorUtility.SetDirty(data);
        return data;
    }

    private static GameObject CreateRainParticlesPrefab()
    {
        string path = Prefabs + "/GG_RainParticles.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        GameObject go = new GameObject("GG_RainParticles");
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 2f;
        main.startSpeed = 10f;
        main.startSize = 0.035f;
        main.startColor = new Color(0.65f, 0.82f, 1f, 0.65f);
        main.loop = true;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 500f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(50f, 1f, 50f);

        ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 2f;
        renderer.velocityScale = 0.25f;

        go.SetActive(false);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static GameObject CreateSimpleToolPrefab(string name, Color color, PrimitiveType primitive)
    {
        string path = Prefabs + "/" + name + ".prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        Material mat = CreateMaterial(name + "_Mat", color);
        GameObject root = new GameObject(name);
        GameObject body = GameObject.CreatePrimitive(primitive);
        body.name = "Body";
        body.transform.SetParent(root.transform, false);
        body.transform.localScale = primitive == PrimitiveType.Cylinder ? new Vector3(0.12f, 0.35f, 0.12f) : new Vector3(0.28f, 0.22f, 0.12f);
        body.transform.localRotation = Quaternion.Euler(65f, 0f, 0f);
        body.GetComponent<Renderer>().sharedMaterial = mat;
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static void SetupExistingDirtTiles(Material unplowed, Material plowed, Material bugMat, Material waterMat)
    {
        GameObject[] candidates = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int count = 0;
        foreach (GameObject go in candidates)
        {
            if (!IsExistingDirtObject(go)) continue;
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            Collider collider = go.GetComponent<Collider>();
            if (renderer == null || collider == null) continue;

            GardenTile tile = go.GetComponent<GardenTile>();
            if (tile == null) tile = go.AddComponent<GardenTile>();
            tile.soilRenderer = renderer;
            tile.unplowedMat = unplowed;
            tile.plowedMat = plowed;
            tile.state = go.name.ToLower().Contains("plowed") ? TileState.Plowed : TileState.Unplowed;
            go.layer = LayerMask.NameToLayer("GardenTile");

            if (tile.bugIndicator == null) tile.bugIndicator = CreateIndicator(go.transform, "GG_BugIndicator", bugMat, new Vector3(-0.28f, 0.08f, 0.28f), new Vector3(0.08f, 0.08f, 0.08f));
            if (tile.waterIndicator == null) tile.waterIndicator = CreateIndicator(go.transform, "GG_WaterIndicator", waterMat, new Vector3(0.28f, 0.08f, 0.28f), new Vector3(0.07f, 0.11f, 0.07f));
            tile.bugIndicator.SetActive(false);
            tile.waterIndicator.SetActive(false);
            EditorUtility.SetDirty(go);
            count++;
        }

        if (count == 0)
        {
            EditorUtility.DisplayDialog("No existing dirt found", "I could not find Normaldirt or Ploweddirt objects in the current scene. Open SampleScene and run the installer again.", "OK");
        }
    }

    private static bool IsExistingDirtObject(GameObject go)
    {
        string n = go.name.ToLowerInvariant();
        return n == "normaldirt" || n == "ploweddirt" || n.Contains("normaldirt") || n.Contains("ploweddirt");
    }

    private static GameObject CreateIndicator(Transform parent, string name, Material mat, Vector3 localPos, Vector3 localScale)
    {
        Transform existing = parent.Find(name);
        if (existing != null) return existing.gameObject;
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        indicator.name = name;
        indicator.transform.SetParent(parent, false);
        indicator.transform.localPosition = localPos;
        indicator.transform.localScale = localScale;
        indicator.GetComponent<Renderer>().sharedMaterial = mat;
        return indicator;
    }

    private static GameObject SetupRainInScene(GameObject rainPrefab)
    {
        GameObject existing = GameObject.Find("GG_Step4_RainParticles");
        if (existing != null) return existing;
        GameObject rain = (GameObject)PrefabUtility.InstantiatePrefab(rainPrefab);
        rain.name = "GG_Step4_RainParticles";
        GameObject garden = GameObject.Find("Garden");
        if (garden != null) rain.transform.position = garden.transform.position + Vector3.up * 12f;
        else rain.transform.position = Vector3.up * 15f;
        rain.SetActive(false);
        return rain;
    }

    private static void SetupBootstrap(GameObject rainInstance)
    {
        GameObject root = GameObject.Find("GG_Step4_Systems");
        if (root == null) root = new GameObject("GG_Step4_Systems");
        GG_Step4SceneBootstrap boot = root.GetComponent<GG_Step4SceneBootstrap>();
        if (boot == null) boot = root.AddComponent<GG_Step4SceneBootstrap>();
        boot.rainParticles = rainInstance;
        Light sun = Object.FindFirstObjectByType<Light>();
        boot.sceneSun = sun;
        boot.safeDaysPerRealSecond = 0.1f;
    }

    private static void SetupPlayerInScene(PlantData tomato, PlantData carrot, PlantData lettuce, GameObject insecticide, GameObject fertilizer, GameObject seedPouch)
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            EditorUtility.DisplayDialog("No Player found", "I could not find the existing Player object. The scripts and prefabs were generated, but the player interactor was not attached.", "OK");
            return;
        }

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc == null) pc = player.AddComponent<PlayerController>();
        pc.tileLayer = LayerMask.GetMask("GardenTile");
        pc.tomatoSeed = tomato;
        pc.carrotSeed = carrot;
        pc.lettuceSeed = lettuce;
        pc.selectedSeed = tomato;
        pc.insecticidePrefab = insecticide;
        pc.fertilizerPrefab = fertilizer;
        pc.seedPouchPrefab = seedPouch;
        pc.hoePrefab = FindPrefabByName("Hoe");
        pc.wateringCanPrefab = FindPrefabByName("Bucket");
        pc.harvestPrefab = FindPrefabByName("Sickle");
        GameObject refill = GameObject.Find("RefilPoint") ?? GameObject.Find("RefillPoint") ?? GameObject.Find("WaterWell");
        if (refill != null) pc.waterRefillPoint = refill.transform;
        EditorUtility.SetDirty(player);
    }

    private static void SetupUIInScene(Material darkMaterial)
    {
        GameObject old = GameObject.Find("GG_Step4_UI_Canvas");
        if (old != null) Object.DestroyImmediate(old);

        GameObject canvasGO = new GameObject("GG_Step4_UI_Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();
        UIManager ui = canvasGO.AddComponent<UIManager>();

        GameObject hudPanel = CreatePanel(canvasGO.transform, "HUD Panel", new Vector2(20, -20), new Vector2(420, 190), new Color(0f, 0f, 0f, 0.42f), TextAnchor.UpperLeft);
        ui.dateText = CreateTMP(hudPanel.transform, "DateText", new Vector2(18, -18), new Vector2(380, 30), 28, FontStyles.Bold, TextAlignmentOptions.Left, "Day 1 | Month 3 | Year 1");
        ui.seasonText = CreateTMP(hudPanel.transform, "SeasonText", new Vector2(18, -55), new Vector2(380, 30), 25, FontStyles.Normal, TextAlignmentOptions.Left, "Season: Spring");
        ui.weatherText = CreateTMP(hudPanel.transform, "WeatherText", new Vector2(18, -90), new Vector2(380, 30), 25, FontStyles.Normal, TextAlignmentOptions.Left, "Weather: Sunny");
        ui.toolText = CreateTMP(hudPanel.transform, "ToolText", new Vector2(18, -125), new Vector2(380, 30), 25, FontStyles.Normal, TextAlignmentOptions.Left, "Tool: Plow");
        ui.waterText = CreateTMP(hudPanel.transform, "WaterText", new Vector2(18, -160), new Vector2(380, 30), 25, FontStyles.Normal, TextAlignmentOptions.Left, "Water: 10/10");

        TextMeshProUGUI cross = CreateTMP(canvasGO.transform, "Crosshair", Vector2.zero, new Vector2(60, 60), 34, FontStyles.Bold, TextAlignmentOptions.Center, "+");
        cross.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        cross.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        cross.rectTransform.anchoredPosition = Vector2.zero;
        TextMeshProUGUI target = CreateTMP(canvasGO.transform, "CrosshairTarget", new Vector2(0, -42), new Vector2(300, 30), 18, FontStyles.Normal, TextAlignmentOptions.Center, "No tile");
        target.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        target.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        ui.crosshairTargetText = target;

        ui.messageText = CreateTMP(canvasGO.transform, "MessageText", new Vector2(0, 155), new Vector2(1000, 60), 32, FontStyles.Bold, TextAlignmentOptions.Center, "");
        ui.messageText.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        ui.messageText.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        ui.messageText.gameObject.SetActive(false);

        GameObject seedPanel = CreatePanel(canvasGO.transform, "Seed Packet Panel", new Vector2(-40, -280), new Vector2(420, 270), new Color(0.08f, 0.045f, 0.03f, 0.62f), TextAnchor.UpperRight);
        ui.seedPacketPanel = seedPanel;
        ui.seedName = CreateTMP(seedPanel.transform, "SeedName", new Vector2(24, -22), new Vector2(370, 38), 30, FontStyles.Bold, TextAlignmentOptions.Left, "Tomato");
        ui.seedMonths = CreateTMP(seedPanel.transform, "SeedMonths", new Vector2(24, -76), new Vector2(370, 30), 23, FontStyles.Normal, TextAlignmentOptions.Left, "Plant: Month 4 - 8");
        ui.seedDays = CreateTMP(seedPanel.transform, "SeedDays", new Vector2(24, -116), new Vector2(370, 30), 23, FontStyles.Normal, TextAlignmentOptions.Left, "Harvest in: 12 days");
        ui.seedFunFact = CreateTMP(seedPanel.transform, "SeedFact", new Vector2(24, -156), new Vector2(370, 88), 19, FontStyles.Normal, TextAlignmentOptions.Left, "7/8/9 select seeds. 1-6 select tools. E interacts.");

        GameObject controlsPanel = CreatePanel(canvasGO.transform, "Controls Panel", new Vector2(0, 24), new Vector2(900, 44), new Color(0f, 0f, 0f, 0.36f), TextAnchor.LowerCenter);
        CreateTMP(controlsPanel.transform, "ControlsText", Vector2.zero, new Vector2(870, 38), 18, FontStyles.Normal, TextAlignmentOptions.Center,
            "1 Hoe   2 Water   3 Insecticide   4 Fertilizer   5 Harvest   6 Seeds   7/8/9 Crops   E Use   R Refill   T Next Day");

        EditorUtility.SetDirty(canvasGO);
    }

    private static GameObject CreatePanel(Transform parent, string name, Vector2 anchoredPos, Vector2 size, Color color, TextAnchor anchor)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        SetAnchor(rt, anchor);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        Image img = panel.AddComponent<Image>();
        img.color = color;
        return panel;
    }

    private static TextMeshProUGUI CreateTMP(Transform parent, string name, Vector2 anchoredPos, Vector2 size, int fontSize, FontStyles style, TextAlignmentOptions align, string text)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = align;
        tmp.color = Color.white;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        return tmp;
    }

    private static void SetAnchor(RectTransform rt, TextAnchor anchor)
    {
        if (anchor == TextAnchor.UpperLeft)
        {
            rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(0f, 1f); rt.pivot = new Vector2(0f, 1f);
        }
        else if (anchor == TextAnchor.UpperRight)
        {
            rt.anchorMin = new Vector2(1f, 1f); rt.anchorMax = new Vector2(1f, 1f); rt.pivot = new Vector2(1f, 1f);
        }
        else if (anchor == TextAnchor.LowerCenter)
        {
            rt.anchorMin = new Vector2(0.5f, 0f); rt.anchorMax = new Vector2(0.5f, 0f); rt.pivot = new Vector2(0.5f, 0f);
        }
    }

    private static GameObject FindPrefabByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        string[] guids = AssetDatabase.FindAssets(name + " t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null && prefab.name.ToLowerInvariant().Contains(name.ToLowerInvariant())) return prefab;
        }
        return null;
    }

    private static void EnsureLayer(string layerName)
    {
        if (LayerMask.NameToLayer(layerName) != -1) return;
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(layer.stringValue))
            {
                layer.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                return;
            }
        }
        Debug.LogWarning("No empty user layer slot was available for " + layerName + ".");
    }
}
#endif

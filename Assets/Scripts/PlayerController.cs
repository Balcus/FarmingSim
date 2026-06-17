using System.Reflection;
using UnityEngine;

public enum PlayerTool { None, Plow, WateringCan, Insecticide, Fertilizer, Harvest }

/// <summary>
/// Step 3.6 controller adapted for the existing FarmingSim player.
/// It does not replace the team's PlayerMovement/MouseMovement. If those scripts exist, movement is left to them.
/// This script adds farming input, seed selection, water refill, tile raycasting, and visible held tools.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement fallback")]
    public float moveSpeed = 5f;
    public float turnSpeed = 200f;
    private CharacterController cc;
    private bool useExistingMovement;

    [Header("Tool")]
    public PlayerTool currentTool = PlayerTool.Plow;
    public PlantData selectedSeed;
    public PlantData tomatoSeed;
    public PlantData carrotSeed;
    public PlantData lettuceSeed;

    [Header("Watering Can")]
    public int wateringCanCapacity = 10;
    public int wateringCanCurrent = 10;
    public Transform waterRefillPoint;
    public float waterRefillRange = 5f;

    [Header("Interaction")]
    public float interactRange = 4f;
    public LayerMask tileLayer;

    [Header("Visible Tool Prefabs")]
    public GameObject hoePrefab;
    public GameObject wateringCanPrefab;
    public GameObject insecticidePrefab;
    public GameObject fertilizerPrefab;
    public GameObject harvestPrefab;
    public GameObject seedPouchPrefab;

    private Camera playerCamera;
    private Transform handRoot;
    private GameObject heldToolInstance;
    private GardenTile currentTarget;

    public LayerMask npcLayer;

    private void Start()
    {
        cc = GetComponent<CharacterController>();
        useExistingMovement = GetComponent("PlayerMovement") != null;
        playerCamera = Camera.main;
        if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();

        if (selectedSeed == null) selectedSeed = tomatoSeed;
        FindRefillPointIfMissing();
        CreateHandRoot();
        UpdateHeldToolVisual();
        UpdateUI();
        
    }

    private void Update()
    {
        if (PauseMenuManager.Instance != null && PauseMenuManager.Instance.IsPaused) return;
        if (!useExistingMovement) HandleMovementFallback();
        HandleToolSwitch();
        HandleTargetUI();

        if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0)) Interact();
        if (Input.GetKeyDown(KeyCode.R)) TryRefillWater();
        if (Input.GetKeyDown(KeyCode.T)) AdvanceDayForTesting();
        if(Input.GetKeyDown(KeyCode.F))
        {
            CheckSeller();
        }
        if(Input.GetKeyDown(KeyCode.G))
        {
            BuyPlotFromSeller();
        }
    }

    private void HandleMovementFallback()
    {
        if (cc == null) return;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = transform.forward * v + transform.right * h;
        cc.SimpleMove(move * moveSpeed);
        if (h != 0 || v != 0) transform.Rotate(0, h * turnSpeed * Time.deltaTime, 0);
    }

    private void HandleToolSwitch()
    {
        bool changed = false;

        if (Input.GetKeyDown(KeyCode.Alpha1)) { currentTool = PlayerTool.Plow; changed = true; }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { currentTool = PlayerTool.WateringCan; changed = true; }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { currentTool = PlayerTool.Insecticide; changed = true; }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { currentTool = PlayerTool.Fertilizer; changed = true; }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { currentTool = PlayerTool.Harvest; changed = true; }
        if (Input.GetKeyDown(KeyCode.Alpha6)) { currentTool = PlayerTool.None; changed = true; }

        if (Input.GetKeyDown(KeyCode.Alpha7)) { selectedSeed = tomatoSeed != null ? tomatoSeed : selectedSeed; currentTool = PlayerTool.None; changed = true; }
        if (Input.GetKeyDown(KeyCode.Alpha8)) { selectedSeed = carrotSeed != null ? carrotSeed : selectedSeed; currentTool = PlayerTool.None; changed = true; }
        if (Input.GetKeyDown(KeyCode.Alpha9)) { selectedSeed = lettuceSeed != null ? lettuceSeed : selectedSeed; currentTool = PlayerTool.None; changed = true; }

        if (changed)
        {
            UpdateHeldToolVisual();
            UpdateUI();
        }
    }

    private void Interact()
    {
        GardenTile tile = RaycastTile();
        if (tile == null)
        {
            if (IsNearWaterRefill())
            {
                TryRefillWater();
                return;
            }

            if (TryGetNearbySeller(out SellerNPC seller))
            {
                seller.SellAll();
                return;
            }

            Message("No garden tile targeted.");
            return;
        }

        switch (currentTool)
        {
            case PlayerTool.Plow:
                tile.TryPlow();
                break;
            case PlayerTool.WateringCan:
                if (wateringCanCurrent > 0)
                {
                    tile.ApplyWater();
                    wateringCanCurrent--;
                }
                else Message("Watering can is empty. Press R near the well.");
                break;
            case PlayerTool.Insecticide:
                tile.TryApplyInsecticide();
                break;
            case PlayerTool.Fertilizer:
                tile.TryApplyFertilizer();
                break;
            case PlayerTool.Harvest:
                tile.TryHarvest();
                break;
            case PlayerTool.None:
                tile.TryPlant(selectedSeed);
                break;
        }

        UpdateUI();
    }

    private GardenTile RaycastTile()
    {
        if (playerCamera == null) return null;

        int mask = tileLayer.value == 0 ? Physics.DefaultRaycastLayers : tileLayer.value;
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, mask))
        {
            GardenTile tile = hit.collider.GetComponentInParent<GardenTile>();
            if (tile != null) return tile;
        }

        // Fallback from the guide: interact with the tile under/near the player.
        Ray downRay = new Ray(transform.position + Vector3.up * 0.6f, Vector3.down);
        if (Physics.Raycast(downRay, out RaycastHit downHit, 2.2f, mask))
            return downHit.collider.GetComponentInParent<GardenTile>();

        return null;
    }

    private void HandleTargetUI()
    {
        currentTarget = RaycastTile();
        if (UIManager.Instance != null)
        {
            string label = currentTarget == null ? "No tile" : currentTarget.state.ToString();
            UIManager.Instance.UpdateCrosshairTarget(label);
        }
    }

    private void TryRefillWater()
    {
        FindRefillPointIfMissing();
        if (waterRefillPoint == null)
        {
            Message("No refill point found.");
            return;
        }

        float dist = GetWaterRefillDistance();
        if (dist <= waterRefillRange)
        {
            wateringCanCurrent = wateringCanCapacity;
            Message("Watering can refilled.");
            UpdateUI();
        }
        else Message("Go near the well to refill.");
    }

    private void FindRefillPointIfMissing()
    {
        if (waterRefillPoint != null) return;
        GameObject refill = GameObject.Find("RefillPoint")
                            ?? GameObject.Find("RefilPoint")
                            ?? GameObject.Find("WaterWell")
                            ?? FindSceneObjectByNameContains("well");
        if (refill != null) waterRefillPoint = refill.transform;
    }

    private bool IsNearWaterRefill()
    {
        FindRefillPointIfMissing();
        return waterRefillPoint != null && GetWaterRefillDistance() <= waterRefillRange;
    }

    private float GetWaterRefillDistance()
    {
        float bestDistance = float.PositiveInfinity;

        if (waterRefillPoint != null)
            bestDistance = Mathf.Min(bestDistance, DistanceToTransformOrColliders(waterRefillPoint));

        GameObject well = GameObject.Find("WaterWell");
        if (well != null && well.transform != waterRefillPoint)
            bestDistance = Mathf.Min(bestDistance, DistanceToTransformOrColliders(well.transform));

        return bestDistance;
    }

    private float DistanceToTransformOrColliders(Transform target)
    {
        if (target == null) return float.PositiveInfinity;

        float bestDistance = Vector3.Distance(transform.position, target.position);
        Collider[] colliders = target.GetComponentsInChildren<Collider>(true);
        foreach (Collider col in colliders)
        {
            if (col == null || !col.enabled) continue;
            Vector3 closestPoint = col.ClosestPoint(transform.position);
            bestDistance = Mathf.Min(bestDistance, Vector3.Distance(transform.position, closestPoint));
        }

        return bestDistance;
    }

    private GameObject FindSceneObjectByNameContains(string value)
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform candidate in transforms)
        {
            if (candidate == null || !candidate.gameObject.scene.IsValid()) continue;
            if (candidate.name.IndexOf(value, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return candidate.gameObject;
        }

        return null;
    }

    private void AdvanceDayForTesting()
    {
        if (GameManager.Instance == null) return;
        MethodInfo advance = typeof(GameManager).GetMethod("AdvanceDay", BindingFlags.Instance | BindingFlags.NonPublic);
        if (advance != null) advance.Invoke(GameManager.Instance, null);
    }

    private void CreateHandRoot()
    {
        if (playerCamera == null) return;
        Transform existing = playerCamera.transform.Find("GG_Step4_HandRoot");
        if (existing != null) { handRoot = existing; return; }

        GameObject root = new GameObject("GG_Step4_HandRoot");
        root.transform.SetParent(playerCamera.transform, false);
        root.transform.localPosition = new Vector3(0.36f, -0.32f, 0.58f);
        root.transform.localRotation = Quaternion.Euler(10f, -18f, 8f);
        root.transform.localScale = Vector3.one;
        handRoot = root.transform;
    }

    private void UpdateHeldToolVisual()
    {
        if (handRoot == null) return;
        if (heldToolInstance != null) Destroy(heldToolInstance);

        GameObject prefab = GetCurrentToolPrefab();
        if (prefab != null)
        {
            heldToolInstance = Instantiate(prefab, handRoot);
            heldToolInstance.transform.localPosition = Vector3.zero;
            heldToolInstance.transform.localRotation = Quaternion.identity;
            heldToolInstance.transform.localScale = Vector3.one * GetHeldToolScale();
        }
        else
        {
            heldToolInstance = CreateFallbackToolVisual();
        }
    }

    private GameObject GetCurrentToolPrefab()
    {
        switch (currentTool)
        {
            case PlayerTool.Plow: return hoePrefab;
            case PlayerTool.WateringCan: return wateringCanPrefab;
            case PlayerTool.Insecticide: return insecticidePrefab;
            case PlayerTool.Fertilizer: return fertilizerPrefab;
            case PlayerTool.Harvest: return harvestPrefab;
            default: return seedPouchPrefab;
        }
    }

    private float GetHeldToolScale()
    {
        switch (currentTool)
        {
            case PlayerTool.Plow:
            case PlayerTool.Harvest:
                return 0.18f;
            case PlayerTool.WateringCan:
                return 0.3f;
            case PlayerTool.Insecticide:
            case PlayerTool.Fertilizer:
                return 0.28f;
            default:
                return 0.25f;
        }
    }

    private GameObject CreateFallbackToolVisual()
    {
        GameObject holder = new GameObject("FallbackTool_" + currentTool);
        holder.transform.SetParent(handRoot, false);
        GameObject primitive = GameObject.CreatePrimitive(currentTool == PlayerTool.WateringCan ? PrimitiveType.Cube : PrimitiveType.Cylinder);
        primitive.transform.SetParent(holder.transform, false);
        primitive.transform.localScale = currentTool == PlayerTool.None ? new Vector3(0.25f, 0.18f, 0.25f) : new Vector3(0.08f, 0.45f, 0.08f);
        primitive.transform.localRotation = Quaternion.Euler(65f, 0f, 0f);
        return holder;
    }

    private void UpdateUI()
    {
        if (UIManager.Instance == null) return;
        UIManager.Instance.UpdateToolDisplay(currentTool);
        UIManager.Instance.UpdateWaterDisplay(wateringCanCurrent, wateringCanCapacity);
        if (selectedSeed != null) UIManager.Instance.ShowSeedPacket(selectedSeed);
    }

    private void Message(string msg)
    {
        if (UIManager.Instance != null) UIManager.Instance.ShowMessage(msg);
        else Debug.Log(msg);
    }

    private bool TryGetNearbySeller(out SellerNPC seller)
    {
        seller = null;
        float range = Mathf.Max(interactRange, 4f);
        Collider[] nearby = Physics.OverlapSphere(transform.position, range);
        foreach (Collider col in nearby)
        {
            seller = col.GetComponentInParent<SellerNPC>();
            if (seller == null) seller = col.GetComponentInChildren<SellerNPC>();
            if (seller != null)
                return true;
        }

        return false;
    }

    void CheckSeller()
    {
        if (TryGetNearbySeller(out SellerNPC seller))
        {
            seller.SellAll();
            return;
        }

        Message("No seller nearby.");
    }

    void BuyPlotFromSeller()
    {
        if (TryGetNearbySeller(out SellerNPC seller))
        {
            seller.BuyPlot();
            return;
        }

        Message("No seller nearby.");
    }
}

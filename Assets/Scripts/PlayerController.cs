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
        if (!useExistingMovement) HandleMovementFallback();
        HandleToolSwitch();
        HandleTargetUI();

        if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0)) Interact();
        if (Input.GetKeyDown(KeyCode.R)) TryRefillWater();
        if (Input.GetKeyDown(KeyCode.T)) AdvanceDayForTesting();
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

        float dist = Vector3.Distance(transform.position, waterRefillPoint.position);
        if (dist < 3.5f)
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
        GameObject refill = GameObject.Find("RefilPoint") ?? GameObject.Find("RefillPoint") ?? GameObject.Find("WaterWell");
        if (refill != null) waterRefillPoint = refill.transform;
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
        root.transform.localPosition = new Vector3(0.48f, -0.42f, 0.72f);
        root.transform.localRotation = Quaternion.Euler(12f, -28f, 0f);
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
            heldToolInstance.transform.localScale = Vector3.one * 0.6f;
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
}
